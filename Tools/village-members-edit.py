from pathlib import Path
b=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Shared')
(b/'HouseholdMembers.razor').write_text('''@using Service.Shared.Contracts.DTOs
@using Service.TanAn.Domain.Enums
<FluentButton OnClick="New" Disabled="busy">Thêm nhân khẩu vào hộ</FluentButton>
@if(!string.IsNullOrEmpty(error)) { <p role="alert">@error</p> }
<FluentDataGrid Items="Members.AsQueryable()" TGridItem="NhanKhauDto" RowSize="DataGridRowSize.Large">
    <PropertyColumn Property="@(x=>x.HoTen)" Title="Họ tên" />
    <PropertyColumn Property="@(x=>x.CCCD)" Title="CCCD" />
    <PropertyColumn Property="@(x=>x.QuanHeVoiChuHo)" Title="Quan hệ" />
    <TemplateColumn Title="Thao tác"><FluentButton Disabled="busy" OnClick="@(() => Edit(context))">Sửa</FluentButton></TemplateColumn>
</FluentDataGrid>
@if(form != null)
{
    <EditForm Model="form" OnSubmit="Save">
        <fieldset disabled="busy" style="display:grid;gap:10px;grid-template-columns:1fr 1fr">
            <legend>@(editingId==null ? "Thêm nhân khẩu" : "Sửa nhân khẩu")</legend>
            <label>Họ tên *<InputText @bind-Value="form.HoTen" required /></label>
            <label>CCCD<InputText @bind-Value="form.CCCD" maxlength="12" /></label>
            <label>Ngày sinh *<InputDate @bind-Value="form.NgaySinh" required /></label>
            <label>Giới tính<InputSelect @bind-Value="form.GioiTinh">@foreach(var gender in Enum.GetValues<GioiTinhEnum>()) { <option value="@gender">@gender</option> }</InputSelect></label>
            <label>Quan hệ với chủ hộ<InputText @bind-Value="form.QuanHeVoiChuHo" required disabled="@(form.QuanHeVoiChuHo == "Chủ hộ")" /></label>
            <label>Nghề nghiệp<InputText @bind-Value="form.NgheNghiep" /></label>
            <FluentButton Type="ButtonType.Submit" Appearance="Appearance.Accent">Lưu nhân khẩu</FluentButton>
            <FluentButton Type="ButtonType.Button" OnClick="@(() => form=null)">Hủy</FluentButton>
        </fieldset>
    </EditForm>
}
''',encoding='utf-8')
(b/'HouseholdMembers.razor.cs').write_text('''using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.UI.CMS.Blazor.Applications;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class HouseholdMembers
{
    [Parameter] public Guid HouseholdId {get;set;}
    [Parameter] public List<NhanKhauDto> Members {get;set;}=new();
    [Parameter] public EventCallback Changed {get;set;}
    [Inject] public IServiceScopeFactory Scopes {get;set;}=default!;
    [Inject] public IUserService Users {get;set;}=default!;
    private CreateNhanKhauForm? form;
    private Guid? editingId;
    private bool busy;
    private string error="";
    private void New() {editingId=null;form=new(){MaHoGiaDinh=HouseholdId,NgaySinh=DateTime.Today};error="";}
    private async Task Edit(NhanKhauDto row)
    {
        busy=true;error="";
        try {
            using var scope=Scopes.CreateScope();
            var result=await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetNhanKhauByIdAsync(row.Id);
            var n=result.Data ?? throw new Exception("Không tải được nhân khẩu.");
            editingId=n.Id;
            form=new(){MaHoGiaDinh=HouseholdId,HoTen=n.HoTen,CCCD=n.CCCD,NgaySinh=n.NgaySinh,GioiTinh=n.GioiTinh,QuanHeVoiChuHo=n.QuanHeVoiChuHo,DanToc=n.DanToc,TonGiao=n.TonGiao,NgheNghiep=n.NgheNghiep,ThuongTru=n.ThuongTru,TamTru=n.TamTru,QueQuan=n.QueQuan,TrinhDoHocVan=n.TrinhDoHocVan};
        }catch(Exception ex) {error=ex.Message;}
        finally {busy=false;}
    }
    private async Task Save()
    {
        if(form==null || busy) return;
        busy=true;error="";
        try {
            if(string.IsNullOrWhiteSpace(form.HoTen) || form.NgaySinh>DateTime.Today || form.NgaySinh<new DateTime(1900,1,1)) throw new ArgumentException("Kiểm tra họ tên và ngày sinh.");
            var user=await Users.GetCurrentUserAsync();if(!user.IsAuthenticated) throw new UnauthorizedAccessException("Hãy đăng nhập lại.");
            using var scope=Scopes.CreateScope();var service=scope.ServiceProvider.GetRequiredService<IPopulationService>();
            var result=editingId.HasValue ? await service.UpdateNhanKhauAsync(editingId.Value,form,user.UserName) : await service.CreateNhanKhauAsync(form,user.UserName);
            if(!result.Success) throw new Exception(result.Message);
            form=null;await Changed.InvokeAsync();
        }catch(Exception ex) {error=ex.Message;}
        finally {busy=false;}
    }
}
''',encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/HoKhau/Index.razor');s=p.read_text(encoding='utf-8');start=s.index('        <FluentDataGrid Items="@((detail.ThanhVien');end=s.index('\n',start);s=s[:start]+'        <HouseholdMembers HouseholdId="detail.Id" Members="@(detail.ThanhVien ?? new())" Changed="@(() => View(detail))" />'+s[end:];p.write_text(s,encoding='utf-8')
# Preserve full resident fields when editing through the new member form.
p=Path('src/Service.TanAn/Service.TanAn.Application/Services/PopulationService.cs');s=p.read_text(encoding='utf-8');s=s.replace('                NgheNghiep = n.NgheNghiep,\n                QuanHeVoiChuHo', '                NgheNghiep = n.NgheNghiep,\n                ThuongTru = n.ThuongTru, QueQuan = n.QueQuan, TrinhDoHocVan = n.TrinhDoHocVan,\n                QuanHeVoiChuHo');s=s.replace('                TamTru = n.TamTru,\n                TrangThai', '                ThuongTru = n.ThuongTru, QueQuan = n.QueQuan, TrinhDoHocVan = n.TrinhDoHocVan,\n                TamTru = n.TamTru,\n                TrangThai');p.write_text(s,encoding='utf-8')
