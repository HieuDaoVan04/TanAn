-- Run explicitly against the configured project database. No startup seeding.
-- The caller owns the transaction; preserve existing page IDs and grants.
DO $population_change_menus$
DECLARE
    parent_menu "Modules"%ROWTYPE;
    root_menu "Modules"%ROWTYPE;
    population_menu "Modules"%ROWTYPE;
BEGIN
    IF (SELECT COUNT(*) FROM "Modules" WHERE "LienKet" = '/bien-dong') <> 1 THEN
        RAISE EXCEPTION 'Expected exactly one existing /bien-dong menu';
    END IF;
    SELECT * INTO parent_menu FROM "Modules" WHERE "LienKet" = '/bien-dong' FOR UPDATE;
    IF (SELECT COUNT(*) FROM "Modules" p JOIN "Modules" h ON h."ModuleChaId" = p."Id"
        WHERE h."LienKet" = '/ho-khau' AND p."TenModule" = 'Quản lý dân cư') <> 1 THEN
        RAISE EXCEPTION 'Expected one Quản lý dân cư parent for the household menu';
    END IF;
    SELECT p.* INTO population_menu FROM "Modules" p JOIN "Modules" h ON h."ModuleChaId" = p."Id"
    WHERE h."LienKet" = '/ho-khau' AND p."TenModule" = 'Quản lý dân cư' FOR UPDATE OF p;

    INSERT INTO "Modules" ("Id", "TenModule", "LienKet", "ModuleChaId", "PhanHeId", "ViTri", "Icon", "Expands", "PhanLoai", "ModerationStatus")
    SELECT '6abb3cd4-67b9-5ab2-972f-d335717dcd43'::uuid, 'Biến động', NULL, population_menu."Id", parent_menu."PhanHeId",
        (SELECT "ViTri" + 1 FROM "Modules" WHERE "LienKet" = '/ho-khau'),
        parent_menu."Icon", true, parent_menu."PhanLoai", parent_menu."ModerationStatus"
    WHERE NOT EXISTS (SELECT 1 FROM "Modules" WHERE "TenModule" = 'Biến động' AND ("ModuleChaId" IS NULL OR "ModuleChaId" = population_menu."Id"));
    IF (SELECT COUNT(*) FROM "Modules" WHERE "TenModule" = 'Biến động' AND ("ModuleChaId" IS NULL OR "ModuleChaId" = population_menu."Id") AND "LienKet" IS NULL) <> 1 THEN
        RAISE EXCEPTION 'Expected one Biến động group without a route';
    END IF;
    SELECT * INTO root_menu FROM "Modules" WHERE "TenModule" = 'Biến động' AND ("ModuleChaId" IS NULL OR "ModuleChaId" = population_menu."Id") AND "LienKet" IS NULL FOR UPDATE;
    UPDATE "Modules" SET "ModuleChaId" = population_menu."Id",
        "ViTri" = (SELECT "ViTri" + 1 FROM "Modules" WHERE "LienKet" = '/ho-khau')
    WHERE "Id" = root_menu."Id";


    IF EXISTS (SELECT 1 FROM "Modules" WHERE "LienKet" IN ('/bien-dong/khai-sinh', '/bien-dong/khai-tu', '/bien-dong/tam-tru', '/bien-dong/tam-vang', '/bien-dong/chuyen-den', '/bien-dong/chuyen-di') AND "ModuleChaId" IS DISTINCT FROM parent_menu."Id" AND "ModuleChaId" IS DISTINCT FROM root_menu."Id") THEN
        RAISE EXCEPTION 'A change type route belongs to another menu parent';
    END IF;
    INSERT INTO "Modules" ("Id", "TenModule", "LienKet", "ModuleChaId", "PhanHeId", "ViTri", "Icon", "Expands", "PhanLoai", "ModerationStatus")
    SELECT c.id, c.name, c.route, root_menu."Id", parent_menu."PhanHeId", c.position + 1, parent_menu."Icon", false, parent_menu."PhanLoai", parent_menu."ModerationStatus"
    FROM (VALUES
                ('38b973de-bb56-54c1-9133-a5ff1a95db55'::uuid, 'Khai sinh', '/bien-dong/khai-sinh', 1),
                ('0b65b4a7-de49-5380-aa30-ef2180e0d030'::uuid, 'Khai tử', '/bien-dong/khai-tu', 2),
                ('8122ad73-37ab-5312-be11-8ff8b4838465'::uuid, 'Tạm trú', '/bien-dong/tam-tru', 3),
                ('45fca68c-4af6-52a0-8379-7c1f2192c327'::uuid, 'Tạm vắng', '/bien-dong/tam-vang', 4),
                ('ef3c5bf2-02cf-5085-ab02-c6dec6b68475'::uuid, 'Chuyển đến', '/bien-dong/chuyen-den', 5),
                ('d269dab8-33e0-5913-92fa-19e87126351e'::uuid, 'Chuyển đi', '/bien-dong/chuyen-di', 6)
    ) AS c(id, name, route, position)
    WHERE NOT EXISTS (SELECT 1 FROM "Modules" m WHERE m."LienKet" = c.route);
    UPDATE "Modules" SET "ModuleChaId" = root_menu."Id", "ViTri" = 1, "Expands" = false
    WHERE "Id" = parent_menu."Id" AND ("ModuleChaId" IS DISTINCT FROM root_menu."Id" OR "ViTri" <> 1 OR "Expands");
    UPDATE "Modules" SET "ModuleChaId" = root_menu."Id", "ViTri" = CASE "LienKet"
        WHEN '/bien-dong/khai-sinh' THEN 2 WHEN '/bien-dong/khai-tu' THEN 3
        WHEN '/bien-dong/tam-tru' THEN 4 WHEN '/bien-dong/tam-vang' THEN 5
        WHEN '/bien-dong/chuyen-den' THEN 6 WHEN '/bien-dong/chuyen-di' THEN 7 END
    WHERE "LienKet" IN ('/bien-dong/khai-sinh','/bien-dong/khai-tu','/bien-dong/tam-tru','/bien-dong/tam-vang','/bien-dong/chuyen-den','/bien-dong/chuyen-di');
    INSERT INTO "RoleModules" ("Id", "RoleId", "ModuleId")
    SELECT md5(rm."RoleId"::text || root_menu."Id"::text)::uuid, rm."RoleId", root_menu."Id"
    FROM "RoleModules" rm WHERE rm."ModuleId" = parent_menu."Id"
      AND NOT EXISTS (SELECT 1 FROM "RoleModules" existing WHERE existing."RoleId" = rm."RoleId" AND existing."ModuleId" = root_menu."Id");
    INSERT INTO "RoleModules" ("Id", "RoleId", "ModuleId")
    SELECT md5(rm."RoleId"::text || child."Id"::text)::uuid, rm."RoleId", child."Id"
    FROM "RoleModules" rm
    JOIN "Modules" child ON child."ModuleChaId" = root_menu."Id"
    WHERE rm."ModuleId" = parent_menu."Id"
      AND child."LienKet" IN ('/bien-dong/khai-sinh', '/bien-dong/khai-tu', '/bien-dong/tam-tru', '/bien-dong/tam-vang', '/bien-dong/chuyen-den', '/bien-dong/chuyen-di')
      AND NOT EXISTS (SELECT 1 FROM "RoleModules" existing WHERE existing."RoleId" = rm."RoleId" AND existing."ModuleId" = child."Id");
END
$population_change_menus$;
