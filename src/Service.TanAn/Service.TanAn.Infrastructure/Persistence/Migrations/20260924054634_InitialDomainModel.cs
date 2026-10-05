using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApThons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "boolean", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApThons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupCode = table.Column<string>(type: "text", nullable: false),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UnitType = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogThaoTacNguoiDungs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ModuleName = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BeforeChange = table.Column<string>(type: "text", nullable: true),
                    AfterChange = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogThaoTacNguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenModule = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    LienKet = table.Column<string>(type: "text", nullable: true),
                    Expands = table.Column<bool>(type: "boolean", nullable: false),
                    ViTri = table.Column<int>(type: "integer", nullable: false),
                    PhanLoai = table.Column<int>(type: "integer", nullable: false),
                    ModuleChaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Modules_ModuleChaId",
                        column: x => x.ModuleChaId,
                        principalTable: "Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionName = table.Column<string>(type: "text", nullable: false),
                    PermissionCode = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSync = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false),
                    PermissionParentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Permissions_PermissionParentId",
                        column: x => x.PermissionParentId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Mota = table.Column<string>(type: "text", nullable: true),
                    IsSync = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSync = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhongBanId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChuyenTrangId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChuyenTrangName = table.Column<string>(type: "text", nullable: false),
                    TenNguoiThucHien = table.Column<string>(type: "text", nullable: true),
                    ListRoleBeforeEdit = table.Column<List<string>>(type: "text[]", nullable: true),
                    ListRoleAfterEdit = table.Column<List<string>>(type: "text[]", nullable: true),
                    LstChuyenMucBeforeEdit = table.Column<List<string>>(type: "text[]", nullable: true),
                    LstChuyenMucAfterEdit = table.Column<List<string>>(type: "text[]", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ApThon = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    PhanLoai = table.Column<int>(type: "integer", nullable: false),
                    KeyPublic = table.Column<string>(type: "text", nullable: true),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "text", nullable: true),
                    TotalLogin = table.Column<int>(type: "integer", nullable: false),
                    TotalLoginFaild = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoGiaDinhs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaSoHo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenChuHo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CCCDChuHo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    DiaChi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApThon = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    ApThonId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoGiaDinhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoGiaDinhs_ApThons_ApThonId",
                        column: x => x.ApThonId,
                        principalTable: "ApThons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhongBanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Groups_PhongBanId",
                        column: x => x.PhongBanId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauNguoiDans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaYeuCau = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HoTenNguoiYeuCau = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CCCDNguoiYeuCau = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    SoDienThoai = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LoaiYeuCau = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NoiDung = table.Column<string>(type: "text", nullable: false),
                    TrangThai = table.Column<int>(type: "integer", nullable: false),
                    CanBoXuLy = table.Column<string>(type: "text", nullable: true),
                    GhiChuCanBo = table.Column<string>(type: "text", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiNopId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanBoXuLyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauNguoiDans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauNguoiDans_Users_CanBoXuLyId",
                        column: x => x.CanBoXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauNguoiDans_Users_NguoiNopId",
                        column: x => x.NguoiNopId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanKhaus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoTen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CCCD = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GioiTinh = table.Column<int>(type: "integer", nullable: false),
                    DanToc = table.Column<string>(type: "text", nullable: false),
                    TonGiao = table.Column<string>(type: "text", nullable: false),
                    QueQuan = table.Column<string>(type: "text", nullable: false),
                    ThuongTru = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TamTru = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NgheNghiep = table.Column<string>(type: "text", nullable: true),
                    TrinhDoHocVan = table.Column<string>(type: "text", nullable: true),
                    QuanHeVoiChuHo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaHoGiaDinh = table.Column<Guid>(type: "uuid", nullable: false),
                    TrangThai = table.Column<int>(type: "integer", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GhiChu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanKhaus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhanKhaus_HoGiaDinhs_MaHoGiaDinh",
                        column: x => x.MaHoGiaDinh,
                        principalTable: "HoGiaDinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanLoaiHos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoGiaDinhId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoaiHo = table.Column<int>(type: "integer", nullable: false),
                    TuNgay = table.Column<DateOnly>(type: "date", nullable: false),
                    DenNgay = table.Column<DateOnly>(type: "date", nullable: true),
                    SoQuyetDinh = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanLoaiHos", x => x.Id);
                    table.CheckConstraint("CK_PhanLoaiHo_ThoiGian", "\"DenNgay\" IS NULL OR \"DenNgay\" >= \"TuNgay\"");
                    table.ForeignKey(
                        name: "FK_PhanLoaiHos_HoGiaDinhs_HoGiaDinhId",
                        column: x => x.HoGiaDinhId,
                        principalTable: "HoGiaDinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuXuLyHoSos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YeuCauId = table.Column<Guid>(type: "uuid", nullable: false),
                    NguoiXuLyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrangThaiCu = table.Column<int>(type: "integer", nullable: true),
                    TrangThaiMoi = table.Column<int>(type: "integer", nullable: false),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuXuLyHoSos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuXuLyHoSos_Users_NguoiXuLyId",
                        column: x => x.NguoiXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuXuLyHoSos_YeuCauNguoiDans_YeuCauId",
                        column: x => x.YeuCauId,
                        principalTable: "YeuCauNguoiDans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YeuCauId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenTep = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DuongDanLuu = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LoaiTep = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DungLuong = table.Column<long>(type: "bigint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKems", x => x.Id);
                    table.CheckConstraint("CK_TepDinhKem_DungLuong", "\"DungLuong\" >= 0");
                    table.ForeignKey(
                        name: "FK_TepDinhKems_YeuCauNguoiDans_YeuCauId",
                        column: x => x.YeuCauId,
                        principalTable: "YeuCauNguoiDans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BienDongDanCus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoaiBienDong = table.Column<int>(type: "integer", nullable: false),
                    NhanKhauId = table.Column<Guid>(type: "uuid", nullable: false),
                    NgayPhatSinh = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoiDenOrDi = table.Column<string>(type: "text", nullable: true),
                    NoiDi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NoiDen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CanBoId = table.Column<Guid>(type: "uuid", nullable: true),
                    LyDo = table.Column<string>(type: "text", nullable: false),
                    CanBoGhiNhan = table.Column<string>(type: "text", nullable: false),
                    FileDinhKemUrl = table.Column<string>(type: "text", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienDongDanCus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienDongDanCus_NhanKhaus_NhanKhauId",
                        column: x => x.NhanKhauId,
                        principalTable: "NhanKhaus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienDongDanCus_Users_CanBoId",
                        column: x => x.CanBoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoiTuongAnSinhs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NhanKhauId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoaiDoiTuong = table.Column<int>(type: "integer", nullable: false),
                    MucTroCapHangThang = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayBatDauHuong = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "boolean", nullable: false),
                    NgayKetThucHuong = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoQuyetDinh = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTuongAnSinhs", x => x.Id);
                    table.CheckConstraint("CK_AnSinh_MucTroCap", "\"MucTroCapHangThang\" >= 0");
                    table.CheckConstraint("CK_AnSinh_ThoiGian", "\"NgayKetThucHuong\" IS NULL OR \"NgayKetThucHuong\" >= \"NgayBatDauHuong\"");
                    table.ForeignKey(
                        name: "FK_DoiTuongAnSinhs_NhanKhaus_NhanKhauId",
                        column: x => x.NhanKhauId,
                        principalTable: "NhanKhaus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienHos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoGiaDinhId = table.Column<Guid>(type: "uuid", nullable: false),
                    NhanKhauId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuanHeVoiChuHo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LaChuHo = table.Column<bool>(type: "boolean", nullable: false),
                    TuNgay = table.Column<DateOnly>(type: "date", nullable: false),
                    DenNgay = table.Column<DateOnly>(type: "date", nullable: true),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienHos", x => x.Id);
                    table.CheckConstraint("CK_ThanhVienHo_ThoiGian", "\"DenNgay\" IS NULL OR \"DenNgay\" >= \"TuNgay\"");
                    table.ForeignKey(
                        name: "FK_ThanhVienHos_HoGiaDinhs_HoGiaDinhId",
                        column: x => x.HoGiaDinhId,
                        principalTable: "HoGiaDinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhVienHos_NhanKhaus_NhanKhauId",
                        column: x => x.NhanKhauId,
                        principalTable: "NhanKhaus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuTroCaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoiTuongAnSinhId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThangNam = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SoTien = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayChiTra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiChiTra = table.Column<string>(type: "text", nullable: false),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    NguoiChiTraId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTroCaps", x => x.Id);
                    table.CheckConstraint("CK_TroCap_SoTien", "\"SoTien\" > 0");
                    table.ForeignKey(
                        name: "FK_LichSuTroCaps_DoiTuongAnSinhs_DoiTuongAnSinhId",
                        column: x => x.DoiTuongAnSinhId,
                        principalTable: "DoiTuongAnSinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuTroCaps_Users_NguoiChiTraId",
                        column: x => x.NguoiChiTraId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApThons_Ma",
                table: "ApThons",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BienDongDanCus_CanBoId",
                table: "BienDongDanCus",
                column: "CanBoId");

            migrationBuilder.CreateIndex(
                name: "IX_BienDongDanCus_NhanKhauId_NgayPhatSinh",
                table: "BienDongDanCus",
                columns: new[] { "NhanKhauId", "NgayPhatSinh" });

            migrationBuilder.CreateIndex(
                name: "IX_DoiTuongAnSinhs_NhanKhauId_LoaiDoiTuong",
                table: "DoiTuongAnSinhs",
                columns: new[] { "NhanKhauId", "LoaiDoiTuong" });

            migrationBuilder.CreateIndex(
                name: "IX_HoGiaDinhs_ApThonId",
                table: "HoGiaDinhs",
                column: "ApThonId");

            migrationBuilder.CreateIndex(
                name: "IX_HoGiaDinhs_MaSoHo",
                table: "HoGiaDinhs",
                column: "MaSoHo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTroCaps_DoiTuongAnSinhId_ThangNam",
                table: "LichSuTroCaps",
                columns: new[] { "DoiTuongAnSinhId", "ThangNam" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTroCaps_NguoiChiTraId",
                table: "LichSuTroCaps",
                column: "NguoiChiTraId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyHoSos_NguoiXuLyId",
                table: "LichSuXuLyHoSos",
                column: "NguoiXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyHoSos_YeuCauId_NgayTao",
                table: "LichSuXuLyHoSos",
                columns: new[] { "YeuCauId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ModuleChaId",
                table: "Modules",
                column: "ModuleChaId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanKhaus_CCCD",
                table: "NhanKhaus",
                column: "CCCD",
                unique: true,
                filter: "\"CCCD\" IS NOT NULL AND \"CCCD\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_NhanKhaus_MaHoGiaDinh_TrangThai",
                table: "NhanKhaus",
                columns: new[] { "MaHoGiaDinh", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionParentId",
                table: "Permissions",
                column: "PermissionParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanLoaiHos_HoGiaDinhId",
                table: "PhanLoaiHos",
                column: "HoGiaDinhId",
                unique: true,
                filter: "\"DenNgay\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKems_YeuCauId",
                table: "TepDinhKems",
                column: "YeuCauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienHos_HoGiaDinhId",
                table: "ThanhVienHos",
                column: "HoGiaDinhId",
                unique: true,
                filter: "\"DenNgay\" IS NULL AND \"LaChuHo\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienHos_HoGiaDinhId_TuNgay",
                table: "ThanhVienHos",
                columns: new[] { "HoGiaDinhId", "TuNgay" });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienHos_NhanKhauId",
                table: "ThanhVienHos",
                column: "NhanKhauId",
                unique: true,
                filter: "\"DenNgay\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PhongBanId",
                table: "UserPermissions",
                column: "PhongBanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauNguoiDans_CanBoXuLyId",
                table: "YeuCauNguoiDans",
                column: "CanBoXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauNguoiDans_MaYeuCau",
                table: "YeuCauNguoiDans",
                column: "MaYeuCau",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauNguoiDans_NguoiNopId",
                table: "YeuCauNguoiDans",
                column: "NguoiNopId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauNguoiDans_TrangThai_NgayGui",
                table: "YeuCauNguoiDans",
                columns: new[] { "TrangThai", "NgayGui" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BienDongDanCus");

            migrationBuilder.DropTable(
                name: "LichSuTroCaps");

            migrationBuilder.DropTable(
                name: "LichSuXuLyHoSos");

            migrationBuilder.DropTable(
                name: "LogThaoTacNguoiDungs");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "PhanLoaiHos");

            migrationBuilder.DropTable(
                name: "RoleModules");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropTable(
                name: "TepDinhKems");

            migrationBuilder.DropTable(
                name: "ThanhVienHos");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserRoleHistories");

            migrationBuilder.DropTable(
                name: "DoiTuongAnSinhs");

            migrationBuilder.DropTable(
                name: "YeuCauNguoiDans");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "NhanKhaus");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "HoGiaDinhs");

            migrationBuilder.DropTable(
                name: "ApThons");
        }
    }
}
