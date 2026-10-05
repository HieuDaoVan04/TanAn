// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITanAnDbContext _db;

        public DashboardService(ITanAnDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResult<ThongKeTongQuanDto>> GetThongKeTongQuanAsync()
        {
            var totalHo = await _db.HoGiaDinhs.CountAsync();
            var totalNhanKhau = await _db.NhanKhaus.CountAsync();
            var nam = await _db.NhanKhaus.CountAsync(n => n.GioiTinh == GioiTinhEnum.Nam);
            var nu = await _db.NhanKhaus.CountAsync(n => n.GioiTinh == GioiTinhEnum.Nu);
            var anSinh = await _db.DoiTuongAnSinhs.CountAsync(d => d.TrangThaiHoatDong);
            var hoNgheoCanNgheo = await _db.DoiTuongAnSinhs.CountAsync(d => d.TrangThaiHoatDong && (d.LoaiDoiTuong == DoiTuongAnSinhEnum.HoNgheo || d.LoaiDoiTuong == DoiTuongAnSinhEnum.HoCanNgheo));
            var chuaXuLy = await _db.YeuCauNguoiDans.CountAsync(y => y.TrangThai == TrangThaiHoSoEnum.MoiTiepNhan || y.TrangThai == TrangThaiHoSoEnum.DangXuLy);
            var daDuyet = await _db.YeuCauNguoiDans.CountAsync(y => y.TrangThai == TrangThaiHoSoEnum.DaPheDuyet);

            var danhSachAp = TanAnLocalities.Villages;
            var hoList = await _db.HoGiaDinhs.Select(h => new { h.Id, h.ApThon }).ToListAsync();
            var nhanKhauCountByHo = await _db.NhanKhaus
                .GroupBy(n => n.MaHoGiaDinh)
                .Select(g => new { HoId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.HoId, g => g.Count);

            var thongKeAp = new List<ThongKeApThonDto>();
            foreach (var ap in danhSachAp)
            {
                var hoThuocAp = hoList.Where(h => string.Equals(h.ApThon, ap, StringComparison.OrdinalIgnoreCase)).ToList();
                int soHo = hoThuocAp.Count;
                int soNhanKhau = hoThuocAp.Sum(h => nhanKhauCountByHo.TryGetValue(h.Id, out var c) ? c : 0);
                var householdIds = hoThuocAp.Select(h => h.Id).ToList();
                int soAnSinh = await _db.DoiTuongAnSinhs.CountAsync(d => d.TrangThaiHoatDong
                    && d.NhanKhau != null && householdIds.Contains(d.NhanKhau.MaHoGiaDinh));

                thongKeAp.Add(new ThongKeApThonDto
                {
                    TenApThon = ap,
                    SoHoGiaDinh = soHo,
                    SoNhanKhau = soNhanKhau,
                    SoDoiTuongAnSinh = soAnSinh
                });
            }

            // Thống kê biến động
            var bienDongList = await _db.BienDongDanCus
                .GroupBy(b => b.LoaiBienDong)
                .Select(g => new { Loai = g.Key, Count = g.Count() })
                .ToListAsync();

            var thongKeBienDong = new List<ThongKeBienDongDto>();
            if (bienDongList.Any())
            {
                foreach (var b in bienDongList)
                {
                    string label = b.Loai switch
                    {
                        LoaiBienDongEnum.KhaiSinh => "Khai sinh",
                        LoaiBienDongEnum.KhaiTu => "Khai tử",
                        LoaiBienDongEnum.TamTru => "Đăng ký tạm trú",
                        LoaiBienDongEnum.TamVang => "Tạm vắng",
                        LoaiBienDongEnum.ChuyenDi => "Chuyển đi",
                        LoaiBienDongEnum.ChuyenDen => "Chuyển đến",
                        _ => b.Loai.ToString()
                    };
                    thongKeBienDong.Add(new ThongKeBienDongDto { LoaiBienDong = label, SoLuong = b.Count });
                }
            }

            var stats = new ThongKeTongQuanDto
            {
                TongSoHoGiaDinh = totalHo,
                TongSoNhanKhau = totalNhanKhau,
                SoNhanKhauNam = nam,
                SoNhanKhauNu = nu,
                TongDoiTuongAnSinh = anSinh,
                TongSoHoNgheoCanNgheo = hoNgheoCanNgheo,
                TongYeuCauChuaXuLy = chuaXuLy,
                TongYeuCauDaPheDuyet = daDuyet,
                ThongKeTheoAp = thongKeAp,
                ThongKeBienDongThang = thongKeBienDong
            };

            return ApiResult<ThongKeTongQuanDto>.Ok(stats);
        }
    }
}
