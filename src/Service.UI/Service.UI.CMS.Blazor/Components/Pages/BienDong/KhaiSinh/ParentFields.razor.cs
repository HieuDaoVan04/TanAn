using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh;

public partial class ParentFields
{
    [Parameter, EditorRequired] public KhaiSinhParentForm Form { get; set; } = default!;
    [Parameter] public IReadOnlyList<NhanKhauDto> Members { get; set; } = [];
    [Parameter] public string HouseholdAddress { get; set; } = "";
    [Parameter] public string ParentLabel { get; set; } = "";
    private void FillPerson()
    {
        var person = Members.SingleOrDefault(x => x.Id == Form.NhanKhauId);
        Form.HoTen = person?.HoTen ?? "";
        Form.NgaySinh = person != null && person.NgaySinh.Year >= 1900 ? person.NgaySinh : null;
        Form.DanToc = person?.DanToc ?? "Kinh";
        Form.QuocTich = "Việt Nam";
        Form.NoiCuTru = !string.IsNullOrWhiteSpace(person?.ThuongTru) ? person.ThuongTru : HouseholdAddress;
    }
}
