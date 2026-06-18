-- ============================================================
-- NetMonitor - Roles Seed Script
-- Safe to run multiple times (ON CONFLICT DO NOTHING).
--
-- NOTE: Users are NOT seeded here because passwords require
--       PBKDF2-SHA256 hashing which cannot be done in plain SQL.
--       Test users are created automatically by DataSeeder.cs
--       when the app starts in Development mode.
--
--       Test credentials:
--         admin   / Admin@123!
--         it_user / IT@1234!
--         guest   / Guest@123!
-- ============================================================

BEGIN;

INSERT INTO roles (role_name, description)
VALUES
    ('Admin', 'Full system access.'),
    ('IT',    'IT staff — device and location management.'),
    ('Guest', 'Read-only access to network data.')
ON CONFLICT (role_name) DO NOTHING;

COMMIT;
