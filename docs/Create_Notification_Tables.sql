-- ============================================================
-- Seaway Notifications — Standard Table Creation Script
-- Creates: NotificationTypes, NotificationSubscriptions, NotificationLog
--
-- BEFORE RUNNING:
--   Replace <<DATABASE_NAME>> with the target database name.
--   The script will fail with a clear error if the placeholder
--   is still present.
--
-- Target:  SPC-DB01
-- Run in:  SQL Server Management Studio against the target database
-- Safe:    Tables are only created if they do not already exist
-- ============================================================

USE [<<DATABASE_NAME>>];
GO

-- ── Guard: fail immediately if placeholder was not replaced ──
IF DB_NAME() = '<<DATABASE_NAME>>'
BEGIN
    RAISERROR(
        'ERROR: You must replace <<DATABASE_NAME>> with the target database name before running this script.',
        20, 1) WITH LOG;
END
GO

-- ── NotificationTypes ────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'NotificationTypes' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[NotificationTypes] (
        [Id]          INT           NOT NULL IDENTITY(1,1),
        [TypeKey]     NVARCHAR(100) NOT NULL,
        [DisplayName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive]    BIT           NOT NULL CONSTRAINT [DF_NotificationTypes_IsActive] DEFAULT (1),
        CONSTRAINT [PK_NotificationTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_NotificationTypes_TypeKey]
        ON [dbo].[NotificationTypes] ([TypeKey] ASC);

    PRINT 'Created table: NotificationTypes';
END
ELSE
BEGIN
    PRINT 'Skipped: NotificationTypes already exists';
END
GO

-- ── NotificationSubscriptions ────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'NotificationSubscriptions' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[NotificationSubscriptions] (
        [Id]                   INT           NOT NULL IDENTITY(1,1),
        [NotificationTypeId]   INT           NOT NULL,
        [Channel]              NVARCHAR(50)  NOT NULL,
        [Recipient]            NVARCHAR(500) NOT NULL,
        [IsActive]             BIT           NOT NULL CONSTRAINT [DF_NotificationSubscriptions_IsActive] DEFAULT (1),
        CONSTRAINT [PK_NotificationSubscriptions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_NotificationSubscriptions_NotificationTypes]
            FOREIGN KEY ([NotificationTypeId])
            REFERENCES [dbo].[NotificationTypes] ([Id])
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationSubscriptions_TypeId_Active]
        ON [dbo].[NotificationSubscriptions] ([NotificationTypeId] ASC, [IsActive] ASC);

    PRINT 'Created table: NotificationSubscriptions';
END
ELSE
BEGIN
    PRINT 'Skipped: NotificationSubscriptions already exists';
END
GO

-- ── NotificationLog ──────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'NotificationLog' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[NotificationLog] (
        [Id]                   INT           NOT NULL IDENTITY(1,1),
        [NotificationTypeId]   INT           NOT NULL,
        [Channel]              NVARCHAR(50)  NOT NULL,
        [Recipient]            NVARCHAR(500) NOT NULL,
        [Subject]              NVARCHAR(500) NOT NULL,
        [Success]              BIT           NOT NULL,
        [ErrorMessage]         NVARCHAR(2000) NULL,
        [SentAt]               DATETIME2     NOT NULL CONSTRAINT [DF_NotificationLog_SentAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_NotificationLog] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_NotificationLog_NotificationTypes]
            FOREIGN KEY ([NotificationTypeId])
            REFERENCES [dbo].[NotificationTypes] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationLog_TypeId_SentAt]
        ON [dbo].[NotificationLog] ([NotificationTypeId] ASC, [SentAt] DESC);

    PRINT 'Created table: NotificationLog';
END
ELSE
BEGIN
    PRINT 'Skipped: NotificationLog already exists';
END
GO

-- ── Summary ──────────────────────────────────────────────────
PRINT '------------------------------------------------------------';
PRINT 'Script complete. Verify tables in database: ' + DB_NAME();
PRINT '------------------------------------------------------------';
SELECT
    t.name                  AS TableName,
    p.rows                  AS RowCount,
    t.create_date           AS CreatedAt
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
WHERE t.name IN ('NotificationTypes','NotificationSubscriptions','NotificationLog')
  AND t.schema_id = SCHEMA_ID('dbo')
ORDER BY t.name;
GO
