// "Một sản phẩm của HieuDV"

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Services
{
    public class WelfareService : IWelfareService
    {
        private readonly ITanAnDbContext _db;
        private readonly IAuditLogService _auditLog;

        public WelfareService(ITanAnDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<ApiResult<PagedResult<DoiTuongAnSinhDto>>> GetDoiTuongAnSinhsAsync(string? keyword, int? loaiDoiTuong, int pageIndex, int pageSize)
        {
            var query = _db.DoiTuongAnSinhs.Include(d => d.NhanKhau).ThenInclude(n => n!.HoGiaDinh).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => (d.NhanKhau != null && (d.NhanKhau.HoTen.Contains(keyword) || d.NhanKhau.CCCD.Contains(keyword))));
            }

            if (loaiDoiTuong.HasValue)
            {
                query = query.Where(d => (int)d.LoaiDoiTuong == loaiDoiTuong.Value);
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderBy(d => d.NhanKhau != null ? d.NhanKhau.HoTen : string.Empty)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DoiTuongAnSinhDto
                {
                    Id = d.Id,
                    NhanKhauId = d.NhanKhauId,
                    HoTen = d.NhanKhau != null ? d.NhanKhau.HoTen : string.Empty,
                    CCCD = d.NhanKhau != null ? d.NhanKhau.CCCD : string.Empty,
                    ApThon = d.NhanKhau != null && d.NhanKhau.HoGiaDinh != null ? d.NhanKhau.HoGiaDinh.ApThon : string.Empty,
                    LoaiDoiTuong = d.LoaiDoiTuong,
                    MucTroCapHangThang = d.MucTroCapHangThang,
                    NgayBatDauHuong = d.NgayBatDauHuong,
                    TrangThaiHoatDong = d.TrangThaiHoatDong,
                    GhiChu = d.GhiChu
                })
                .ToListAsync();

            return ApiResult<PagedResult<DoiTuongAnSinhDto>>.Ok(new PagedResult<DoiTuongAnSinhDto>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<ApiResult<DoiTuongAnSinhDto>> CreateDoiTuongAnSinhAsync(CreateAnSinhForm form, string username)
        {
            var nk = await _db.NhanKhaus.FindAsync(form.NhanKhauId);
            var dt = new DoiTuongAnSinh
            {
                NhanKhauId = form.NhanKhauId,
                LoaiDoiTuong = form.LoaiDoiTuong,
                MucTroCapHangThang = form.MucTroCapHangThang,
                NgayBatDauHuong = form.NgayBatDauHuong,
                TrangThaiHoatDong = true,
                GhiChu = form.GhiChu
            };

            await _db.DoiTuongAnSinhs.AddAsync(dt);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                username,
                "Tạo đối tượng an sinh",
                "DoiTuongAnSinh",
                dt.Id.ToString(),
                null,
                JsonSerializer.Serialize(new
                {
                    dt.NhanKhauId,
                    HoTen = nk?.HoTen,
                    dt.LoaiDoiTuong,
                    dt.MucTroCapHangThang,
                    dt.NgayBatDauHuong,
                    dt.TrangThaiHoatDong
                }));

            return ApiResult<DoiTuongAnSinhDto>.Ok(new DoiTuongAnSinhDto
            {
                Id = dt.Id,
                NhanKhauId = dt.NhanKhauId,
                HoTen = nk?.HoTen ?? string.Empty,
                CCCD = nk?.CCCD ?? string.Empty,
                LoaiDoiTuong = form.LoaiDoiTuong,
                MucTroCapHangThang = dt.MucTroCapHangThang,
                NgayBatDauHuong = dt.NgayBatDauHuong,
                TrangThaiHoatDong = dt.TrangThaiHoatDong
            });
        }

        public async Task<ApiResult<LichSuTroCapDto>> AddLichSuTroCapAsync(CreateTroCapForm form, string username)
        {
            var ls = new LichSuTroCap
            {
                DoiTuongAnSinhId = form.DoiTuongAnSinhId,
                ThangNam = form.ThangNam,
                SoTien = form.SoTien,
                NgayChiTra = DateTime.Now,
                NguoiChiTra = username,
                GhiChu = form.GhiChu
            };

            await _db.LichSuTroCaps.AddAsync(ls);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                username,
                "Ghi nhận chi trả trợ cấp",
                "LichSuTroCap",
                ls.Id.ToString(),
                null,
                JsonSerializer.Serialize(new
                {
                    ls.DoiTuongAnSinhId,
                    ls.ThangNam,
                    ls.SoTien,
                    ls.NgayChiTra,
                    ls.NguoiChiTra
                }));

            return ApiResult<LichSuTroCapDto>.Ok(new LichSuTroCapDto
            {
                Id = ls.Id,
                DoiTuongAnSinhId = ls.DoiTuongAnSinhId,
                ThangNam = form.ThangNam,
                SoTien = ls.SoTien,
                NgayChiTra = ls.NgayChiTra,
                NguoiChiTra = ls.NguoiChiTra,
                GhiChu = ls.GhiChu
            });
        }
    }
}


