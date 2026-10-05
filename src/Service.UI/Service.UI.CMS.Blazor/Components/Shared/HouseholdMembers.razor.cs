using Microsoft.AspNetCore.Components;
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
            form=new(){MaHoGiaDinh=HouseholdId,HoTen=n.HoTen,CCCD=n.CCCD,NgaySinh=n.NgaySinh,GioiTinh=n.GioiTinh,QuanHeVoiChuHo=n.QuanHeVoiChuHo,DanToc=n.DanToc,TonGiao=n.TonGiao,NgheNghiep=n.NgheNghiep,ThuongTru=n.ThuongTru,TamTru=n.TamTru,QueQuan=n.QueQuan,TrinhDoHocVan=n.TrinhDoHocVan,GhiChu=n.GhiChu};
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
