START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007110402_UseBirthModerationStatus') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD "ModerationStatus" integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007110402_UseBirthModerationStatus') THEN
    UPDATE "HoSoKhaiSinhs"
    SET "ModerationStatus" = CASE WHEN "DaDuyet" THEN 0 ELSE 1 END;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007110402_UseBirthModerationStatus') THEN
    ALTER TABLE "HoSoKhaiSinhs" DROP COLUMN "DaDuyet";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007110402_UseBirthModerationStatus') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD CONSTRAINT "CK_HoSoKhaiSinhs_ModerationStatus" CHECK ("ModerationStatus" IN (0, 1));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007110402_UseBirthModerationStatus') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261007110402_UseBirthModerationStatus', '9.0.2');
    END IF;
END $EF$;
COMMIT;
