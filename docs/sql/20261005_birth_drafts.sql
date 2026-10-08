START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    CREATE TABLE "HoSoKhaiSinhs" (
        "Id" uuid NOT NULL,
        "MaHoSo" character varying(50) NOT NULL,
        "HoTenTre" character varying(200) NOT NULL,
        "HoTenNguoiYeuCau" character varying(200) NOT NULL,
        "MaSoHo" character varying(50) NOT NULL,
        "NgaySinh" timestamp without time zone,
        "HoGiaDinhId" uuid,
        "ApThonId" uuid,
        "NoiDungJson" text NOT NULL,
        "PhienBan" integer NOT NULL,
        "NgayTao" timestamp without time zone NOT NULL,
        "NguoiTaoId" uuid,
        "NgaySua" timestamp without time zone,
        "NguoiSuaId" uuid,
        CONSTRAINT "PK_HoSoKhaiSinhs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HoSoKhaiSinhs_ApThons_ApThonId" FOREIGN KEY ("ApThonId") REFERENCES "ApThons" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_HoSoKhaiSinhs_HoGiaDinhs_HoGiaDinhId" FOREIGN KEY ("HoGiaDinhId") REFERENCES "HoGiaDinhs" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_HoSoKhaiSinhs_Users_NguoiTaoId" FOREIGN KEY ("NguoiTaoId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    CREATE INDEX "IX_HoSoKhaiSinhs_ApThonId_NgayTao" ON "HoSoKhaiSinhs" ("ApThonId", "NgayTao");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    CREATE INDEX "IX_HoSoKhaiSinhs_HoGiaDinhId" ON "HoSoKhaiSinhs" ("HoGiaDinhId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    CREATE UNIQUE INDEX "IX_HoSoKhaiSinhs_MaHoSo" ON "HoSoKhaiSinhs" ("MaHoSo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    CREATE INDEX "IX_HoSoKhaiSinhs_NguoiTaoId_NgayTao" ON "HoSoKhaiSinhs" ("NguoiTaoId", "NgayTao");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005150646_AddBirthDrafts') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261005150646_AddBirthDrafts', '9.0.2');
    END IF;
END $EF$;
COMMIT;
