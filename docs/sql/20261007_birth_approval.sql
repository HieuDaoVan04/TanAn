START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD "DaDuyet" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD "NgayDuyet" timestamp without time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD "NguoiDuyet" character varying(100) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD "NguoiDuyetId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    CREATE INDEX "IX_HoSoKhaiSinhs_NguoiDuyetId" ON "HoSoKhaiSinhs" ("NguoiDuyetId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    ALTER TABLE "HoSoKhaiSinhs" ADD CONSTRAINT "FK_HoSoKhaiSinhs_Users_NguoiDuyetId" FOREIGN KEY ("NguoiDuyetId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261007105518_AddBirthApproval') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261007105518_AddBirthApproval', '9.0.2');
    END IF;
END $EF$;
COMMIT;
