-- ============================================================
-- Seaway.Notifications - Initial Seed
-- Target: SeawayNotifications database on SPC-DB01
-- Run once after InitialCreate migration
-- Safe to re-run (idempotent)
-- ============================================================

SET NOCOUNT ON;

-- ── Apps ─────────────────────────────────────────────────────
MERGE INTO Apps AS target
USING (VALUES
    ('USPS_TRACKING',  'USPSTracking',            'USPS mail tracking and file processing application.'),
    ('MAIL_PREP',      'MailPrep',                'Mail preparation and presort application.'),
    ('FILE_WATCHER',   'USPSTracking.FileWatcher', 'File drop watcher service for USPS tracking imports.')
) AS source (AppKey, DisplayName, Description)
ON target.AppKey = source.AppKey
WHEN NOT MATCHED THEN
    INSERT (AppKey, DisplayName, Description)
    VALUES (source.AppKey, source.DisplayName, source.Description);

-- ── Notification Types ────────────────────────────────────────
MERGE INTO NotificationTypes AS target
USING (VALUES
    -- USPSTracking.FileWatcher
    ('FILE_WATCHER', 'FILE_PROC_FAILED',         'File Processing Failed',          'A file dropped into the watch folder failed to process.'),
    ('FILE_WATCHER', 'UNKNOWN_VERSION',           'Unknown File Version',            'A file was received with an unrecognised version identifier.'),
    ('FILE_WATCHER', 'LOW_LINKAGE_RATE',          'Low Linkage Rate',                'Import completed but linkage rate fell below the acceptable threshold.'),
    ('FILE_WATCHER', 'SYSTEM_ERROR',              'System Error',                    'An unexpected system-level error occurred in the file watcher.'),
    -- USPSTracking.Web
    ('USPS_TRACKING', 'TRACKING_IMPORT_FAILED',   'Tracking Import Failed',          'A tracking data import failed in the web application.'),
    ('USPS_TRACKING', 'USER_LOGIN_ANOMALY',        'User Login Anomaly',              'A suspicious or failed login attempt was detected.'),
    ('USPS_TRACKING', 'REPORT_EXPORT_FAILED',      'Report Export Failed',            'A report export request failed to complete.'),
    -- MailPrep
    ('MAIL_PREP',    'JOB_FAILED',                'Job Failed',                      'A mail preparation job failed.'),
    ('MAIL_PREP',    'JOB_COMPLETED_WITH_WARNINGS','Job Completed with Warnings',     'A job completed but encountered non-fatal warnings.'),
    ('MAIL_PREP',    'PRESORT_ERROR',             'Presort Error',                   'An error occurred during the presort process.')
) AS source (AppKey, TypeKey, DisplayName, Description)
ON target.TypeKey = source.TypeKey
    AND target.AppId = (SELECT Id FROM Apps WHERE AppKey = source.AppKey)
WHEN NOT MATCHED THEN
    INSERT (AppId, TypeKey, DisplayName, Description)
    VALUES (
        (SELECT Id FROM Apps WHERE AppKey = source.AppKey),
        source.TypeKey,
        source.DisplayName,
        source.Description
    );

PRINT 'Seed completed successfully.';