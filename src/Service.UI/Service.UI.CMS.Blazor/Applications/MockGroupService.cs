using System;
using System.Collections.Generic;
using System.Linq;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Model.Commons;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.UI.CMS.Blazor.Applications
{
    public static class MockGroupService
    {
        private static readonly object _lock = new();
        private static List<GroupDto>? _groups;

        public static List<GroupDto> GetGroups()
        {
            lock (_lock)
            {
                if (_groups != null) return _groups;

                var parentBtp = new GroupDto
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Bộ Tư pháp",
                    MaGroup = "BTP",
                    Description = "Bộ Tư pháp",
                    ModerationStatus = ModerationStatus.Approved,
                    LoaiGroup = EnumLoaiGroup.DonVi
                };

                var sampleNames = new (string Name, string Code)[]
                {
                    ("CSDL Quốc gia về văn bản pháp luật", "CSDLQG"),
                    ("UAT_HDSD", "UAT_HDSD"),
                    ("UAT_CSDL_CongChung", "UAT_CSDL_CongChung"),
                    ("CSDL Toà án", "CSDLTA"),
                    ("UAT_CSDL_THADS", "CSDLTHADS"),
                    ("Hệ thống Thông tin giải quyết TTHC", "HT_TTHC"),
                    ("CSDL Hộ tịch điện tử", "CSDL_HT"),
                    ("Hệ thống Quản lý văn bản và điều hành", "QLVB_DH"),
                    ("Hệ thống Thống kê ngành Tư pháp", "TK_TP"),
                    ("Cổng dịch vụ công Quốc gia", "DVC_QG"),
                    ("Cục Đăng ký giao dịch bảo đảm", "DKGDBD"),
                    ("Cục Trợ giúp pháp lý", "TGPL"),
                    ("Cục Bổ trợ tư pháp", "BTTP"),
                    ("Cục Phổ biến, giáo dục pháp luật", "PBGDPL"),
                    ("Cục Hộ tịch, quốc tịch, chứng thực", "HTQTCT"),
                    ("Trung tâm Lý lịch tư pháp quốc gia", "LLTPQG"),
                    ("Văn phòng Bộ Tư pháp", "VP_BTP"),
                    ("Vụ Hợp tác quốc tế", "HTQT"),
                    ("Vụ Pháp luật dân sự - kinh tế", "PLDS_KT"),
                    ("Vụ Pháp luật hình sự - hành chính", "PLHS_HC"),
                    ("Vụ Pháp luật quốc tế", "PLQT"),
                    ("Vụ Tổ chức cán bộ", "TCCB"),
                    ("Thanh tra Bộ Tư pháp", "TT_BTP"),
                    ("Báo Pháp luật Việt Nam", "PLVN"),
                    ("Tạp chí Dân chủ và Pháp luật", "DC_PL"),
                    ("Học viện Tư pháp", "HVTP"),
                    ("Trường Đại học Luật Hà Nội", "DHLHN"),
                    ("Sở Tư pháp Hà Nội", "STP_HN"),
                    ("Sở Tư pháp TP Hồ Chí Minh", "STP_HCM"),
                    ("Sở Tư pháp Đà Nẵng", "STP_DN"),
                    ("Sở Tư pháp Hải Phòng", "STP_HP"),
                    ("Sở Tư pháp Cần Thơ", "STP_CT"),
                    ("Sở Tư pháp An Giang", "STP_AG"),
                    ("Sở Tư pháp Bà Rịa - Vũng Tàu", "STP_BRVT"),
                    ("Sở Tư pháp Bắc Giang", "STP_BG"),
                    ("Sở Tư pháp Bắc Kạn", "STP_BK"),
                    ("Sở Tư pháp Bạc Liêu", "STP_BL"),
                    ("Sở Tư pháp Bắc Ninh", "STP_BN"),
                    ("Sở Tư pháp Bến Tre", "STP_BT"),
                    ("Sở Tư pháp Bình Định", "STP_BD"),
                    ("Sở Tư pháp Bình Dương", "STP_BDG"),
                    ("Sở Tư pháp Bình Phước", "STP_BP"),
                    ("Sở Tư pháp Bình Thuận", "STP_BTU"),
                    ("Sở Tư pháp Cà Mau", "STP_CM"),
                    ("Sở Tư pháp Cao Bằng", "STP_CB"),
                    ("Sở Tư pháp Đắk Lắk", "STP_DL"),
                    ("Sở Tư pháp Đắk Nông", "STP_DNONG"),
                    ("Sở Tư pháp Điện Biên", "STP_DB"),
                    ("Sở Tư pháp Đồng Nai", "STP_DNAI"),
                    ("Sở Tư pháp Đồng Tháp", "STP_DT"),
                    ("Sở Tư pháp Gia Lai", "STP_GL"),
                    ("Sở Tư pháp Hà Giang", "STP_HG"),
                    ("Sở Tư pháp Hà Nam", "STP_HNA"),
                    ("Sở Tư pháp Hà Tĩnh", "STP_HT")
                };

                _groups = new List<GroupDto>();
                for (int i = 0; i < sampleNames.Length; i++)
                {
                    var (name, code) = sampleNames[i];
                    _groups.Add(new GroupDto
                    {
                        Id = Guid.NewGuid(),
                        Index = i + 1,
                        Name = name,
                        MaGroup = code,
                        Parent = parentBtp,
                        ParentId = parentBtp.Id,
                        Description = null,
                        ModerationStatus = ModerationStatus.Approved,
                        LoaiGroup = EnumLoaiGroup.DonVi,
                        Quota = 100,
                        Created = DateTime.UtcNow.AddDays(-i),
                        LastModified = DateTime.UtcNow.AddDays(-i)
                    });
                }

                return _groups;
            }
        }

        public static DataTableJson<GroupDto> GetPaged(GroupQuery? query)
        {
            var all = GetGroups();
            var q = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query?.Keyword))
            {
                var kw = query.Keyword.Trim().ToLower();
                q = q.Where(x => x.Name.ToLower().Contains(kw) || x.MaGroup.ToLower().Contains(kw));
            }

            var total = q.Count();
            int page = query?.gridRequest?.page ?? 1;
            int pageSize = query?.gridRequest?.pageSize ?? 5;
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var items = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Index = (page - 1) * pageSize + i + 1;
            }

            return new DataTableJson<GroupDto>
            {
                Data = items,
                Total = total
            };
        }

        public static List<GroupTreeDto> GetTree(string? searchTerm)
        {
            var all = GetGroups();
            var root = new GroupTreeDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Bộ Tư pháp",
                MaGroup = "BTP",
                LoaiGroup = EnumLoaiGroup.DonVi,
                ModerationStatus = ModerationStatus.Approved,
                Children = new List<GroupTreeDto>()
            };

            foreach (var g in all)
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    if (!g.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) &&
                        !g.MaGroup.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                root.Children.Add(new GroupTreeDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    MaGroup = g.MaGroup,
                    LoaiGroup = g.LoaiGroup,
                    ModerationStatus = g.ModerationStatus,
                    Children = new List<GroupTreeDto>()
                });
            }

            return new List<GroupTreeDto> { root };
        }

        public static GroupDto? GetById(Guid id)
        {
            var all = GetGroups();
            return all.FirstOrDefault(x => x.Id == id) ?? all.FirstOrDefault();
        }

        public static void SetModerationStatus(Guid id, ModerationStatus status)
        {
            var item = GetGroups().FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.ModerationStatus = status == ModerationStatus.Approved ? ModerationStatus.Approved : ModerationStatus.Pending;
            }
        }

        public static void Delete(Guid id)
        {
            var all = GetGroups();
            var item = all.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                all.Remove(item);
            }
        }
    }
}
