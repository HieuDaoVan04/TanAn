START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005080355_AddBirthDeclarationSnapshot') THEN
    ALTER TABLE "BienDongDanCus" ADD "HoSoKhaiSinhJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005080355_AddBirthDeclarationSnapshot') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261005080355_AddBirthDeclarationSnapshot', '9.0.2');
    END IF;
END $EF$;
COMMIT;
