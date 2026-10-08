using Service.Shared.Commons.Model.SQL;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Application.Services;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;
using Service.UI.CMS.Blazor.Applications;
using BirthEdit = Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh.Edit;
using Service.UI.CMS.Blazor.Components.Shared;

try
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    await using var db = new TanAnDbContext(new DbContextOptionsBuilder<TanAnDbContext>().UseSqlite(connection).Options);
    await db.Database.EnsureCreatedAsync();
    var villageA = new ApThon { Ma = "FILTER-A", Ten = "Thôn lọc A" };
    var villageB = new ApThon { Ma = "FILTER-B", Ten = "Thôn lọc B" };
    var inactiveVillage = new ApThon { Ma = "FILTER-OFF", Ten = "Thôn không hoạt động", DangHoatDong = false };
    db.ApThons.AddRange(villageA, villageB, inactiveVillage);
    await db.SaveChangesAsync();
    db.Users.Add(new() { UserName = "ui-test", Role = RoleEnum.Admin });
    await db.SaveChangesAsync();
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddHttpContextAccessor();
    services.AddFluentUIComponents();
    services.AddSingleton<IJSRuntime, NoJavaScript>();
    services.AddSingleton<NavigationManager, TestNavigation>();
    services.AddSingleton<ITanAnDbContext>(db);
    services.AddSingleton<IPopulationService>(new PopulationService(db, new AuditLogService(db)));
    services.AddSingleton<IUserService, TestUser>();
    services.AddSingleton(DispatchProxy.Create<ICallServiceRegistry, UnusedRegistry>());
    await using var provider = services.BuildServiceProvider();
    await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
    async Task<string> Render<T>(Dictionary<string, object?>? parameters = null) where T : IComponent
        => await renderer.Dispatcher.InvokeAsync(async () => WebUtility.HtmlDecode(
            (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters ?? new()))).ToHtmlString()));
    async Task<string> RenderType(Type componentType, Dictionary<string, object?>? parameters = null)
        => await renderer.Dispatcher.InvokeAsync(async () => WebUtility.HtmlDecode(
            (await renderer.RenderComponentAsync(componentType, ParameterView.FromDictionary(parameters ?? new()))).ToHtmlString()));
    var aggregate = await Render<Service.UI.CMS.Blazor.Components.Pages.BienDong.BienDongDanCu.Index>();
    if (aggregate.Contains("Thêm mới") || !aggregate.Contains("Danh sách biến động dân cư"))
        throw new Exception("Aggregate must list all changes without creation controls.");
    Console.WriteLine("PASS: aggregate renders without add button");
    if (!aggregate.Contains("Tất cả loại biến động") || !aggregate.Contains("id=\"change-from-date\"")
        || !aggregate.Contains("id=\"change-to-date\"") || !aggregate.Contains("row-number-heading"))
        throw new Exception("Aggregate is missing type/date filters or the visible row-number heading");
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>())
        if (!aggregate.Contains($"value=\"{(int)type}\"")) throw new Exception($"Missing filter option for {type}");
    Console.WriteLine("PASS: aggregate renders all type options, both date filters and explicit # heading");
    var icon = new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.PersonArrowRight();
    var group = new Service.UI.CMS.Blazor.Components.Layout.NavGroup(icon, "Biến động dân cư", true, "gap-sm",
        [new Service.UI.CMS.Blazor.Components.Layout.NavLink("/bien-dong/khai-sinh", icon, "Khai sinh", [])]) { Href = "/bien-dong" };
    var menuHtml = await Render<FluentNavMenu>(new() { ["ChildContent"] = (RenderFragment)(builder =>
    {
        builder.OpenComponent<NavMenuItem>(0);
        builder.AddAttribute(1, "Value", group);
        builder.CloseComponent();
    }) });
    if (!menuHtml.Contains("href=\"/bien-dong\"") || !menuHtml.Contains("href=\"/bien-dong/khai-sinh\""))
        throw new Exception("Parent menu must link to the aggregate alongside its type children.");
    Console.WriteLine("PASS: parent menu links to aggregate and child menu links to its category");
    var pageFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>())
    {
        var pageType = typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{type}.Index")!;
        ComponentBase? page = null;
        var root = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["PageType"] = pageType,
                ["Capture"] = (Action<ComponentBase>)(component => page = component)
            })));
        async Task Act(string method, params object[] args) => await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = pageType.GetMethod(method, pageFlags)!.Invoke(page, args);
            if (result is Task task) await task;
            typeof(ComponentBase).GetMethod("StateHasChanged", pageFlags)!.Invoke(page, null);
        });
        async Task<string> Html() => await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(root.ToHtmlString()));
        var filter = pageType.GetField("searchKeyword", pageFlags)!;
        filter.SetValue(page, "retained-filter");
        await Act("OpenEdit");
        var editHtml = await Html();
        if (!editHtml.Contains($"Đăng ký {BusinessCreateDialog.Label(type.ToString()).ToLowerInvariant()}"))
            throw new Exception($"Page {type} did not open its own editor");
        if ((string)filter.GetValue(page)! != "retained-filter") throw new Exception("Opening a dialog cleared the list filter");
        await Act("CloseEdit");
        if ((await Html()).Contains("<fluent-dialog")) throw new Exception($"Page {type} did not close its editor");
        await Act("OpenView", new BienDongDto { LoaiBienDong = type, HoTenNhanKhau = "Kiểm tra chi tiết", NgayPhatSinh = new(2026, 10, 5) });
        var viewHtml = await Html();
        if (!viewHtml.Contains("Kiểm tra chi tiết") || !viewHtml.Contains($"Chi tiết {BusinessCreateDialog.Label(type.ToString()).ToLowerInvariant()}"))
            throw new Exception($"Page {type} did not open its own details");
        await Act("CloseView");
        if ((await Html()).Contains("<fluent-dialog")) throw new Exception($"Page {type} did not close its details");
        await Act("OpenEdit");
        await Act("OnSaved");
        if ((bool)pageType.GetField("showEdit", pageFlags)!.GetValue(page)!)
            throw new Exception($"Page {type} did not close editor after save");
    }
    Console.WriteLine("PASS: all six screen folders open their Edit/View, preserve filters and close/reload after save");
    var birthDetails = await Render<Service.UI.CMS.Blazor.Components.Pages.BienDong.BienDongDanCu.View>(new()
    {
        ["Record"] = new BienDongDto
        {
            LoaiBienDong = LoaiBienDongEnum.KhaiSinh,
            HoSoKhaiSinh = new KhaiSinhForm { HoTen = "Trẻ trong hồ sơ", HoTenNguoiYeuCau = "Người khai hồ sơ" }
        }
    });
    if (!birthDetails.Contains("Trẻ trong hồ sơ") || !birthDetails.Contains("Người khai hồ sơ") || !birthDetails.Contains("Thông tin khai sinh"))
        throw new Exception("Aggregate did not render the complete birth declaration");
    Console.WriteLine("PASS: aggregate View renders the complete birth declaration");
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>())
    {
        var html = await RenderType(typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{type}.Index")!);
        if (html.Contains("change-type-filter") || html.Contains("Tất cả loại biến động"))
            throw new Exception($"Category page must not display the type filter: {type}");
        if (!html.Contains("id=\"change-from-date\"") || !html.Contains("id=\"change-to-date\""))
            throw new Exception($"Category page is missing its date filters: {type}");
        if (!html.Contains("id=\"change-village-filter\"") || !html.Contains("Tất cả thôn")
            || !html.Contains(villageA.Ten) || !html.Contains(villageB.Ten) || html.Contains(inactiveVillage.Ten))
            throw new Exception($"Category page did not render its active village filter: {type}");
        if (!html.Contains("Thêm mới") || !html.Contains($"Danh sách {BusinessCreateDialog.Label(type.ToString()).ToLowerInvariant()}"))
            throw new Exception($"Missing category heading or add action for {type}.");
        if (type == LoaiBienDongEnum.KhaiSinh)
        {
            var birthForm = await Render<BirthEdit>();
            foreach (var label in new[] { "Đăng ký khai sinh", "Tra cứu hộ", "Thông tin trẻ", "Người yêu cầu đăng ký", "Thông tin mẹ", "Thông tin cha", "Lưu nháp", "Để trống nếu chưa có", "Tự nhập thông tin", "Loại nơi sinh", "Loại cư trú" })
                if (!birthForm.Contains(label)) throw new Exception($"Birth form is missing {label}");
            if (birthForm.Contains("Tìm nhân khẩu theo họ tên")) throw new Exception("Birth form must accept a new child instead of requiring an existing resident");
            if (System.Text.RegularExpressions.Regex.IsMatch(birthForm, @"<fieldset\b[^>]*\bdisabled\b"))
                throw new Exception("Birth inputs are disabled while the form is idle");
            Console.WriteLine("PASS: birth dialog renders household lookup, new child, applicant, optional parents and save action");
            continue;
        }
        var form = await RenderType(typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{type}.Edit")!);
        if (!form.Contains($"Đăng ký {BusinessCreateDialog.Label(type.ToString()).ToLowerInvariant()}")
            || !System.Text.RegularExpressions.Regex.IsMatch(form, @"<select[^>]*disabled")
            || !System.Text.RegularExpressions.Regex.IsMatch(form, $"<option(?=[^>]*value=\"{type}\")(?=[^>]*selected)[^>]*>"))
            throw new Exception($"Create form must keep its category fixed: {type}.");
        Console.WriteLine($"PASS: {type} page has add action and fixed-type form");
    }
    var houseA = new HoGiaDinh { MaSoHo = "FILL-A", TenChuHo = "Chủ hộ A", DiaChi = "Địa chỉ A", DiaBan = villageA, ApThon = villageA.Ten };
    var personA = new NhanKhau { HoGiaDinh = houseA, HoTen = "Người trong hộ A", CCCD = "111111111111", NgaySinh = new(1990, 1, 1), ThuongTru = "Địa chỉ riêng A", DanToc = "Dân tộc A" };
    var houseB = new HoGiaDinh { MaSoHo = "FILL-B", TenChuHo = "Chủ hộ B", DiaChi = "Địa chỉ B", DiaBan = villageB, ApThon = villageB.Ten };
    db.NhanKhaus.Add(personA); db.HoGiaDinhs.Add(houseB); await db.SaveChangesAsync();
    BirthEdit? captured = null;
    var birthRoot = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<BirthFormHarness>(
        ParameterView.FromDictionary(new Dictionary<string, object?> { ["Capture"] = (Action<BirthEdit>)(component => captured = component) })));
    var flags = BindingFlags.Instance | BindingFlags.NonPublic;
    async Task Invoke(string name) => await renderer.Dispatcher.InvokeAsync(async () =>
    {
        var result = typeof(BirthEdit).GetMethod(name, flags)!.Invoke(captured, null);
        if (result is Task task) await task;
    });
    void Field(string name, object value) => typeof(BirthEdit).GetField(name, flags)!.SetValue(captured, value);
    async Task CheckBirthInputState(bool processing)
    {
        Field("busy", processing);
        var html = await renderer.Dispatcher.InvokeAsync(() =>
        {
            typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(captured, null);
            return birthRoot.ToHtmlString();
        });
        var disabled = System.Text.RegularExpressions.Regex.IsMatch(html, @"<fieldset\b[^>]*\bdisabled\b");
        if (disabled != processing) throw new Exception($"Birth input disabled state is wrong when busy={processing}");
    }
    await CheckBirthInputState(false);
    await CheckBirthInputState(true);
    await CheckBirthInputState(false);
    Console.WriteLine("PASS: birth inputs are editable while idle, locked during processing and re-enabled afterward");
    var draft = (KhaiSinhForm)typeof(BirthEdit).GetField("form", flags)!.GetValue(captured)!;
    Field("houseKeyword", " FILL-A "); await Invoke("FindHousehold");
    if (draft.HoGiaDinhId != houseA.Id || draft.ThuongTru != houseA.DiaChi) throw new Exception("Code lookup did not fill the household and child address");
    draft.NguoiYeuCauId = personA.Id; await Invoke("FillApplicant");
    if (draft.HoTenNguoiYeuCau != personA.HoTen || draft.SoGiayTo != personA.CCCD || draft.NoiCuTruNguoiYeuCau != personA.ThuongTru)
        throw new Exception("Selecting an applicant did not fill stored identity and address");
    Field("houseKeyword", "FILL-B"); await Invoke("FindHousehold");
    if (draft.HoGiaDinhId != houseB.Id || draft.ThuongTru != houseB.DiaChi || draft.NguoiYeuCauId.HasValue || draft.HoTenNguoiYeuCau != "")
        throw new Exception("Changing households leaked the previous applicant or address");
    Field("houseKeyword", "NO-SUCH-HOUSE"); await Invoke("FindHousehold");
    if (draft.HoGiaDinhId != Guid.Empty) throw new Exception("Failed household lookup retained a stale selection");
    if (VietnameseDateText.Format(new(2026, 10, 5)) != "Ngày năm tháng mười năm hai nghìn không trăm hai mươi sáu")
        throw new Exception("Birth date text was not spelled in Vietnamese");
    Console.WriteLine("PASS: actual birth dialog callbacks fill household/applicant data and clear stale data on changed or failed lookup");
    var draftBeforePeople = await db.NhanKhaus.CountAsync();
    var draftBeforeChanges = await db.BienDongDanCus.CountAsync();
    var draftSaves = 0;
    captured!.Saved = EventCallback.Factory.Create(new object(), () => draftSaves++);
    draft.HoTen = "Tên trẻ giữ khi chuyển cách nhập"; draft.HoTenNguoiYeuCau = "Người nhập thủ công";
    Field("houseKeyword", "NO-SUCH-HOUSE"); await Invoke("FindHousehold");
    if (draft.HoTenNguoiYeuCau != "Người nhập thủ công") throw new Exception("Failed lookup without linked household discarded manual information");
    draft.KieuNhap = KieuNhapKhaiSinh.TuNhap; draft.HoTen = "Trẻ nhập thủ công";
    await Invoke("ChangeEntryMode");
    await renderer.Dispatcher.InvokeAsync(async () => await (Task)typeof(BirthEdit).GetMethod("Save", flags)!.Invoke(captured, [new Microsoft.AspNetCore.Components.Forms.EditContext(draft)])!);
    if (draftSaves != 1 || await db.HoSoKhaiSinhs.CountAsync(x => x.Id == draft.RequestId) != 1 || await db.NhanKhaus.CountAsync() != draftBeforePeople || await db.BienDongDanCus.CountAsync() != draftBeforeChanges)
        throw new Exception("Manual draft form must save without household and without creating population records: " + typeof(BirthEdit).GetField("error", flags)!.GetValue(captured));
    var reopenedHtml = await Render<BirthEdit>(new() { ["DraftId"] = draft.RequestId });
    if (!reopenedHtml.Contains("Trẻ nhập thủ công") || !reopenedHtml.Contains("Lưu nháp")) throw new Exception("Draft form did not reopen saved fields");
    BirthEdit? reopenedEditor = null;
    await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<BirthFormHarness>(ParameterView.FromDictionary(new Dictionary<string, object?>
        { ["DraftId"] = draft.RequestId, ["Capture"] = (Action<BirthEdit>)(component => reopenedEditor = component) })));
    var reopenedForm = (KhaiSinhForm)typeof(BirthEdit).GetField("form", flags)!.GetValue(reopenedEditor)!;
    reopenedForm.HoTen = "Trẻ nhập thủ công đã sửa";
    await renderer.Dispatcher.InvokeAsync(async () => await (Task)typeof(BirthEdit).GetMethod("Save", flags)!.Invoke(reopenedEditor, [new Microsoft.AspNetCore.Components.Forms.EditContext(reopenedForm)])!);
    var editedDraft = await db.HoSoKhaiSinhs.SingleAsync(x => x.Id == draft.RequestId);
    if (editedDraft.HoTenTre != reopenedForm.HoTen || editedDraft.PhienBan != 2 || await db.NhanKhaus.CountAsync() != draftBeforePeople)
        throw new Exception("Reopened form did not update the same draft without creating a resident");
    var draftPageHtml = await Render<Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh.Index>();
    if (!draftPageHtml.Contains("Trẻ nhập thủ công") || !draftPageHtml.Contains("Chưa duyệt") || !draftPageHtml.Contains("Sửa")) throw new Exception("Draft index did not display saved draft and actions");
    Console.WriteLine("PASS: actual manual form saves incomplete draft without population writes, reopens fields and displays edit actions");
    // Exercise the Fluent grid's real page-change callback with more than one page of drafts.
    var draftService = provider.GetRequiredService<IPopulationService>();
    for (var i = 1; i <= 12; i++)
    {
        var result = await draftService.SaveKhaiSinhDraftAsync(new()
        {
            HoSo = new() { HoTen = $"Phân trang hồ sơ {i:00}", KieuNhap = KieuNhapKhaiSinh.TuNhap },
            ApThonId = i <= 6 ? villageA.Id : villageB.Id
        }, "ui-test");
        if (!result.Success) throw new Exception("Could not seed paging fixture: " + result.Message);
    }
    var birthPageType = typeof(Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh.Index);
    ComponentBase? draftList = null;
    var draftListRoot = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(
        ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            ["PageType"] = birthPageType,
            ["Capture"] = (Action<ComponentBase>)(component => draftList = component)
        })));
    async Task DraftListAction(string method, params object[] args) => await renderer.Dispatcher.InvokeAsync(async () =>
    {
        await (Task)birthPageType.GetMethod(method, flags)!.Invoke(draftList, args)!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(draftList, null);
    });
    List<KhaiSinhDraftDto> DraftRows() => (List<KhaiSinhDraftDto>)birthPageType.GetField("drafts", flags)!.GetValue(draftList)!;
    var draftPagination = (PaginationState)birthPageType.GetField("draftPagination", flags)!.GetValue(draftList)!;
    birthPageType.GetField("searchKeyword", flags)!.SetValue(draftList, "Phân trang hồ sơ");
    await DraftListAction("LoadData");
    if (DraftRows().Count != 10 || draftPagination.TotalItemCount != 12) throw new Exception("Draft grid must load only the requested page with the full total");
    var firstPageIds = DraftRows().Select(x => x.Id).ToHashSet();
    await renderer.Dispatcher.InvokeAsync(() => draftPagination.SetCurrentPageIndexAsync(1));
    if (DraftRows().Count != 2 || DraftRows().Any(x => firstPageIds.Contains(x.Id))
        || (int)birthPageType.GetField("draftStartIndex", flags)!.GetValue(draftList)! != 10)
        throw new Exception("Fluent draft pagination lost, duplicated or misnumbered the second page");
    draftPagination.ItemsPerPage = 5;
    await DraftListAction("RefreshData", 5);
    if (draftPagination.CurrentPageIndex != 0 || DraftRows().Count != 5 || draftPagination.TotalItemCount != 12)
        throw new Exception("Changing the draft page size did not reset and reload the Fluent grid");
    draftPagination.ItemsPerPage = 10;
    await DraftListAction("RefreshData", 10);
    birthPageType.GetField("selectedVillageId", flags)!.SetValue(draftList, villageA.Id);
    await DraftListAction("LoadData");
    if (draftPagination.CurrentPageIndex != 0 || draftPagination.TotalItemCount != 6 || DraftRows().Count != 6)
        throw new Exception("Changing draft village filter did not reset pagination and filter the results");
    birthPageType.GetField("searchKeyword", flags)!.SetValue(draftList, "NO-SUCH-DRAFT");
    await DraftListAction("LoadData");
    if (DraftRows().Count != 0 || draftPagination.TotalItemCount != 0) throw new Exception("Draft keyword filter did not empty the Fluent grid");
    await DraftListAction("ClearSearch");
    birthPageType.GetField("activeTabId", flags)!.SetValue(draftList, "recorded");
    await DraftListAction("ChangeTab");
    if ((bool)birthPageType.GetField("showDrafts", flags)!.GetValue(draftList)!) throw new Exception("Fluent tab callback did not select recorded births");
    birthPageType.GetField("activeTabId", flags)!.SetValue(draftList, "drafts");
    await DraftListAction("ChangeTab");
    var fluentListHtml = await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(draftListRoot.ToHtmlString()));
    if (DraftRows().Count != 10 || draftPagination.TotalItemCount != 13 || !fluentListHtml.Contains("<fluent-tabs")
        || !fluentListHtml.Contains("fluent-data-grid") || !fluentListHtml.Contains("<fluent-button") || !fluentListHtml.Contains("paginator-nav")
        || !fluentListHtml.Contains("Hiển thị 1 đến 10 của 13 bản ghi"))
        throw new Exception("Switching tabs did not restore the Fluent draft grid and shared paginator");
    Console.WriteLine("PASS: Fluent draft grid pages without duplicate rows, resets on filters and reloads after tab changes");
    var testUser = (TestUser)provider.GetRequiredService<IUserService>();
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>().Where(x => x != LoaiBienDongEnum.KhaiSinh))
    {
        var editType = typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{type}.Edit")!;
        ComponentBase? editor = null;
        await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["PageType"] = editType,
                ["Capture"] = (Action<ComponentBase>)(component => editor = component)
            })));
        async Task EditAction(string method) => await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await (Task)editType.GetMethod(method, flags)!.Invoke(editor, null)!;
        });
        editType.GetField("keyword", flags)!.SetValue(editor, personA.CCCD);
        await EditAction("FindPeople");
        if (!((List<NhanKhauDto>)editType.GetField("people", flags)!.GetValue(editor)!).Any(x => x.Id == personA.Id))
            throw new Exception($"Local {type} editor did not find the selected resident");
        var beforeCount = await db.BienDongDanCus.CountAsync();
        await EditAction("Save");
        if (await db.BienDongDanCus.CountAsync() != beforeCount || !((string)editType.GetField("error", flags)!.GetValue(editor)!).Contains("chọn nhân khẩu"))
            throw new Exception($"Local {type} editor accepted an empty resident");
        editType.GetField("personId", flags)!.SetValue(editor, personA.Id);
        await EditAction("Save");
        if (await db.BienDongDanCus.CountAsync() != beforeCount || !((string)editType.GetField("error", flags)!.GetValue(editor)!).Contains("lý do"))
            throw new Exception($"Local {type} editor accepted an empty reason");
        var change = (CreateBienDongForm)editType.GetField("change", flags)!.GetValue(editor)!;
        change.LyDo = $"Local form {type}";
        change.NgayPhatSinh = new(2026, 10, 5);
        change.LoaiBienDong = LoaiBienDongEnum.KhaiSinh;
        var admin = testUser.Current;
        testUser.Current = new() { IsAuthenticated = true, Role = "CanBoThon", UserName = "no-menu" };
        await EditAction("Save");
        testUser.Current = admin;
        if (await db.BienDongDanCus.CountAsync() != beforeCount || !((string)editType.GetField("error", flags)!.GetValue(editor)!).Contains("quyền"))
            throw new Exception($"Local {type} editor allowed saving without its menu permission");
        var saves = 0;
        editType.GetProperty("Saved")!.SetValue(editor, EventCallback.Factory.Create(new object(), () => saves++));
        await EditAction("Save");
        var stored = await db.BienDongDanCus.SingleOrDefaultAsync(x => x.LyDo == change.LyDo);
        if (stored?.LoaiBienDong != type || stored.NhanKhauId != personA.Id || stored.CanBoGhiNhan != admin.UserName || saves != 1)
            throw new Exception($"Local {type} editor did not save the screen's fixed type and notify the list");
        if ((bool)editType.GetField("busy", flags)!.GetValue(editor)!) throw new Exception($"Local {type} editor remained locked after save");
    }
    Console.WriteLine("PASS: all five local change editors find residents, reject missing fields/menu permission, save their fixed type and unlock");
    var personB = new NhanKhau { HoGiaDinh = houseB, HoTen = "Người trong hộ B", CCCD = "222222222222" };
    db.NhanKhaus.Add(personB);
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>())
        db.BienDongDanCus.Add(new() { NhanKhau = personB, LoaiBienDong = type, NgayPhatSinh = new(2026, 10, 5), LyDo = "Village B fixture" });
    db.BienDongDanCus.Add(new() { NhanKhau = personA, LoaiBienDong = LoaiBienDongEnum.KhaiSinh, NgayPhatSinh = new(2026, 10, 5), LyDo = "Village A birth fixture" });
    await db.SaveChangesAsync();
    foreach (var folder in Enum.GetValues<LoaiBienDongEnum>().Select(x => x.ToString()).Append("BienDongDanCu"))
    {
        var pageType = typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{folder}.Index")!;
        ComponentBase? page = null;
        await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["PageType"] = pageType,
                ["Capture"] = (Action<ComponentBase>)(component => page = component)
            })));
        async Task Load() => await renderer.Dispatcher.InvokeAsync(async () =>
            await (Task)pageType.GetMethod("LoadData", flags)!.Invoke(page, null)!);
        IQueryable<BienDongDto> Rows() => (IQueryable<BienDongDto>)pageType.GetField("bienDongQuery", flags)!.GetValue(page)!;
        if (folder == "KhaiSinh") await renderer.Dispatcher.InvokeAsync(async () => await (Task)pageType.GetMethod("ShowRecorded", flags)!.Invoke(page, null)!);
        var count = folder == "BienDongDanCu" ? 12 : 2;
        if (Rows().Count() != count) throw new Exception($"Local {folder} list did not load its own category");
        pageType.GetField("fromDate", flags)!.SetValue(page, new DateTime(2026, 10, 5));
        pageType.GetField("toDate", flags)!.SetValue(page, new DateTime(2026, 10, 5));
        await Load();
        if (Rows().Count() != count) throw new Exception($"Local {folder} list lost records on the inclusive date boundaries");
        if (folder != "BienDongDanCu")
        {
            pageType.GetField("selectedVillageId", flags)!.SetValue(page, villageA.Id);
            await Load();
            if (Rows().Count() != 1 || Rows().Single().NhanKhauId != personA.Id)
                throw new Exception($"Local {folder} list did not combine village A with type/date filters");
            pageType.GetField("selectedVillageId", flags)!.SetValue(page, villageB.Id);
            await Load();
            if (Rows().Count() != 1 || Rows().Single().NhanKhauId != personB.Id)
                throw new Exception($"Local {folder} list did not filter village B");
        }
        pageType.GetField("searchKeyword", flags)!.SetValue(page, "NO-SUCH-PERSON");
        await Load();
        if (Rows().Any()) throw new Exception($"Local {folder} list ignored the keyword");
        await renderer.Dispatcher.InvokeAsync(async () => await (Task)pageType.GetMethod("ClearSearch", flags)!.Invoke(page, null)!);
        if (Rows().Count() != count) throw new Exception($"Local {folder} list did not reset its filters");
        if (folder == "BienDongDanCu")
        {
            pageType.GetField("selectedType", flags)!.SetValue(page, (int)LoaiBienDongEnum.ChuyenDen);
            await Load();
            if (Rows().Count() != 2 || Rows().Any(x => x.LoaiBienDong != LoaiBienDongEnum.ChuyenDen))
                throw new Exception("Local aggregate list ignored the selected change type");
        }
    }
    var currentAdmin = testUser.Current;
    testUser.Current = new() { IsAuthenticated = true, Role = "CanBoThon", VillageIds = [villageA.Id] };
    foreach (var type in Enum.GetValues<LoaiBienDongEnum>())
    {
        var html = await RenderType(typeof(BirthEdit).Assembly.GetType($"Service.UI.CMS.Blazor.Components.Pages.BienDong.{type}.Index")!);
        if (!html.Contains(villageA.Ten) || html.Contains(villageB.Ten) || html.Contains(inactiveVillage.Ten))
            throw new Exception($"Local {type} village dropdown exposed an unassigned or inactive village");
    }
    testUser.Current = currentAdmin;
    Console.WriteLine("PASS: six village filters combine with type/date/search, reset to all and only offer assigned active villages");
    bool HasApprovalButton(string html, bool disabled) => System.Text.RegularExpressions.Regex.Matches(html, @"<fluent-button\b[^>]*>[\s\S]*?</fluent-button>")
        .Cast<System.Text.RegularExpressions.Match>().Any(match => match.Value.Contains("Duyệt hồ sơ")
            && System.Text.RegularExpressions.Regex.IsMatch(match.Value.Split('>')[0], @"\bdisabled\b") == disabled);
    foreach (var role in new[] { "Admin", "CanBoXa", "CanBoThon", "NguoiDan", "ChuTichXa", "ctx" })
    {
        testUser.Current = new() { IsAuthenticated = true, Role = role, UserName = "ui-test", VillageIds = [villageA.Id] };
        var html = await Render<Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh.Index>();
        if (!HasApprovalButton(html, true) || HasApprovalButton(html, false)) throw new Exception($"Approval must be disabled for {role}");
    }
    testUser.Current = new() { IsAuthenticated = true, Role = "CanBoXa", RoleCodes = ["ctx"] };
    var noMenuHtml = await Render<Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh.Index>();
    if (!HasApprovalButton(noMenuHtml, true) || HasApprovalButton(noMenuHtml, false)) throw new Exception("ctx without birth menu must not enable approval");
    var chairman = new User { UserName = "ui-chairman", Role = RoleEnum.CanBoXa };
    var ctxRole = new Role { RoleCode = "ctx", RoleName = "Chủ tịch xã" };
    var birthMenu = new Service.TanAn.Domain.Entities.Module { TenModule = "Khai sinh", LienKet = "/bien-dong/khai-sinh" };
    db.Users.Add(chairman); db.Roles.Add(ctxRole); db.Modules.Add(birthMenu);
    db.UserRoles.Add(new() { UserId = chairman.Id, RoleId = ctxRole.Id });
    db.RoleModules.Add(new() { RoleId = ctxRole.Id, ModuleId = birthMenu.Id });
    await db.SaveChangesAsync();
    testUser.Current = new()
    {
        IsAuthenticated = true, UserName = chairman.UserName, Role = "CanBoXa", RoleCodes = ["CTX"],
        MenusActive = [new() { Path = "/bien-dong/khai-sinh" }]
    };
    var readyDraft = await draftService.SaveKhaiSinhDraftAsync(new()
    {
        HoSo = new()
        {
            HoGiaDinhId = houseA.Id, HoTen = "Trẻ duyệt tự tạo", NgaySinh = new(2024, 1, 1), NgayDangKy = null,
            NoiSinh = "Bệnh viện kiểm tra", QueQuan = "Tân An", ThuongTru = houseA.DiaChi, QuanHeVoiChuHo = "Con",
            HoTenNguoiYeuCau = "Người yêu cầu kiểm tra", SoGiayTo = "123456789012", NoiCuTruNguoiYeuCau = houseA.DiaChi, QuanHeVoiTre = "Cha"
        }
    }, chairman.UserName);
    if (!readyDraft.Success) throw new Exception("Could not seed complete declaration: " + readyDraft.Message);
    var pendingTotal = await db.HoSoKhaiSinhs.CountAsync(x => x.ModerationStatus == ModerationStatus.Pending);
    ComponentBase? notificationComponent = null;
    var notificationRoot = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        ["PageType"] = typeof(ApprovalNotifications),
        ["Capture"] = (Action<ComponentBase>)(component => notificationComponent = component)
    })));
    async Task<string> NotificationHtml() => await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(notificationRoot.ToHtmlString()));
    async Task ReloadNotifications() => await renderer.Dispatcher.InvokeAsync(async () =>
    {
        await (Task)typeof(ApprovalNotifications).GetMethod("Load", flags)!.Invoke(notificationComponent, null)!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(notificationComponent, null);
    });
    var notificationHtml = await NotificationHtml();
    if (!notificationHtml.Contains("Việc cần duyệt") || !notificationHtml.Contains($"Xem tất cả hồ sơ chờ duyệt ({pendingTotal})")
        || System.Text.RegularExpressions.Regex.Matches(notificationHtml, "class=\"approval-notice\"").Count != Math.Min(10, pendingTotal))
        throw new Exception("Approval notifications must show total count and bounded pending preview for ctx at login");
    var workspaceNotices = await Render<Service.UI.CMS.Blazor.Components.Pages.BanLamViec.ViewThongBaoHT>();
    if (!workspaceNotices.Contains("Việc cần duyệt") || workspaceNotices.Contains("Thông báo theo thôn"))
        throw new Exception("Workspace must show pending approvals and hide village notifications for ctx");
    ComponentBase? bell = null;
    await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        ["PageType"] = typeof(Service.UI.CMS.Blazor.Components.Layout.Component.NotificationCenter),
        ["Capture"] = (Action<ComponentBase>)(component => bell = component)
    })));
    var bellType = bell!.GetType();
    if ((int)bellType.GetField("pendingApprovals", flags)!.GetValue(bell)! != pendingTotal)
        throw new Exception("Header notification badge must include total pending approvals at login");
    ComponentBase? approvalPage = null;
    var approvalRoot = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        ["PageType"] = birthPageType,
        ["Capture"] = (Action<ComponentBase>)(component => approvalPage = component)
    })));
    var chairmanHtml = await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(approvalRoot.ToHtmlString()));
    if (!HasApprovalButton(chairmanHtml, false) || !chairmanHtml.Contains("#107c10") || !chairmanHtml.Contains(villageA.Ten) || !chairmanHtml.Contains(villageB.Ten))
        throw new Exception("Chairman must have green enabled approval buttons and all commune villages");
    var approvalRow = readyDraft.Data!;
    await renderer.Dispatcher.InvokeAsync(async () =>
    {
        await (Task)typeof(ApprovalNotifications).GetMethod("Open", flags)!.Invoke(notificationComponent, [(Guid?)approvalRow.Id])!;
        birthPageType.GetProperty("RequestedDraftId")!.SetValue(approvalPage, approvalRow.Id);
        await (Task)birthPageType.GetMethod("OnParametersSetAsync", flags)!.Invoke(approvalPage, null)!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(approvalPage, null);
    });
    if (!provider.GetRequiredService<NavigationManager>().Uri.EndsWith($"/bien-dong/khai-sinh?hoSoId={approvalRow.Id:D}"))
        throw new Exception("Notification must link to the exact pending draft");
    async Task<string> DetailHtml() => await renderer.Dispatcher.InvokeAsync(() =>
        System.Text.RegularExpressions.Regex.Match(WebUtility.HtmlDecode(approvalRoot.ToHtmlString()), @"<fluent-dialog\b[\s\S]*?</fluent-dialog>").Value);
    var cleanDetailHtml = await DetailHtml();
    if (string.IsNullOrEmpty(cleanDetailHtml) || cleanDetailHtml.Contains("filterError") || cleanDetailHtml.Contains("role=\"alert\""))
        throw new Exception("Opening birth detail without error must not display a literal variable name or alert");
    var chairmanUser = testUser.Current;
    testUser.Current = currentAdmin;
    await renderer.Dispatcher.InvokeAsync(async () =>
    {
        await (Task)birthPageType.GetMethod("ApproveDraft", flags)!.Invoke(approvalPage, [approvalRow])!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(approvalPage, null);
    });
    var deniedDetailHtml = await DetailHtml();
    if (!deniedDetailHtml.Contains(BirthApprovalAuthorization.DeniedMessage) || deniedDetailHtml.Contains("filterError"))
        throw new Exception("Actual approval failure must appear inside the open detail dialog");
    testUser.Current = chairmanUser;
    await renderer.Dispatcher.InvokeAsync(async () =>
    {
        await (Task)birthPageType.GetMethod("ApproveDraft", flags)!.Invoke(approvalPage, [approvalRow])!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(approvalPage, null);
    });
    var approvedHtml = await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(approvalRoot.ToHtmlString()));
    var approvedRow = (await draftService.GetKhaiSinhDraftAsync(approvalRow.Id)).Data!;
    if (approvedRow.HoSo!.NgayDangKy?.Date != approvedRow.NgayDuyet!.Value.ToLocalTime().Date)
        throw new Exception("Draft without registration date must automatically use approval date");
    if (!(approvedRow.ModerationStatus == ModerationStatus.Approved) || !approvedHtml.Contains("Đã duyệt") || !approvedHtml.Contains(chairman.UserName))
        throw new Exception("Actual approval callback did not update status and open detail");
    if ((await DetailHtml()).Contains("role=\"alert\"")) throw new Exception("Successful approval must clear the old detail error");
    if ((await DetailHtml()).Contains("Duyệt hồ sơ") || approvedHtml.Contains("Ghi nhận nhân khẩu"))
        throw new Exception("Approved detail must have no approval or separate population registration button");
    if (!approvedRow.DaGhiNhan || (bool)birthPageType.GetField("showDrafts", flags)!.GetValue(approvalPage)!
        || !approvedHtml.Contains("Trẻ duyệt tự tạo") || approvedHtml.Contains("Chỉ hiển thị hồ sơ chưa duyệt")
        || !await db.BienDongDanCus.AnyAsync(x => x.Id == approvedRow.Id))
        throw new Exception("Approval must create population data and switch to recorded tab without pending-only checkbox");
    await ReloadNotifications();
    if (!(await NotificationHtml()).Contains($"Xem tất cả hồ sơ chờ duyệt ({pendingTotal - 1})")
        || (await NotificationHtml()).Contains(approvedRow.MaHoSo))
        throw new Exception("Refreshing notification must remove approved record and reduce count");
    await renderer.Dispatcher.InvokeAsync(async () => await (Task)bellType.GetMethod("RefreshUnread", flags)!.Invoke(bell, null)!);
    if ((int)bellType.GetField("pendingApprovals", flags)!.GetValue(bell)! != pendingTotal - 1)
        throw new Exception("Header notification badge must decrease after approval refresh");
    ctxRole.ModerationStatus = ModerationStatus.Pending; await db.SaveChangesAsync();
    await ReloadNotifications();
    if ((await NotificationHtml()).Contains("Việc cần duyệt")) throw new Exception("Revoked ctx role must hide notifications even with stale UI role codes");
    ctxRole.ModerationStatus = ModerationStatus.Approved; await db.SaveChangesAsync();
    var lockedEditor = await Render<BirthEdit>(new() { ["DraftId"] = approvedRow.Id });
    if (!lockedEditor.Contains("Hồ sơ đã duyệt, không được chỉnh sửa") || !System.Text.RegularExpressions.Regex.IsMatch(lockedEditor, @"<fieldset\b[^>]*\bdisabled\b"))
        throw new Exception("Approved editor must refuse changes");
    var legacyForm = System.Text.Json.JsonSerializer.Deserialize<KhaiSinhForm>(System.Text.Json.JsonSerializer.Serialize(readyDraft.Data!.HoSo))!;
    legacyForm.RequestId = Guid.NewGuid(); legacyForm.HoTen = "Trẻ hồ sơ cũ giao diện";
    var legacySaved = await draftService.SaveKhaiSinhDraftAsync(new() { HoSo = legacyForm }, chairman.UserName);
    var legacyRecord = await db.HoSoKhaiSinhs.SingleAsync(x => x.Id == legacySaved.Data!.Id);
    legacyRecord.ModerationStatus = ModerationStatus.Approved; legacyRecord.NguoiDuyet = chairman.UserName;
    legacyRecord.NguoiDuyetId = chairman.Id; legacyRecord.NgayDuyet = DateTime.UtcNow.AddDays(-1);
    await db.SaveChangesAsync();
    provider.GetRequiredService<NavigationManager>().NavigateTo("/bien-dong/khai-sinh");
    ComponentBase? legacyPage = null;
    var legacyRoot = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<ScreenHarness>(ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        ["PageType"] = birthPageType,
        ["Capture"] = (Action<ComponentBase>)(component => legacyPage = component)
    })));
    var pendingRows = (List<KhaiSinhDraftDto>)birthPageType.GetField("drafts", flags)!.GetValue(legacyPage)!;
    if (pendingRows.Any(x => x.ModerationStatus != ModerationStatus.Pending)
        || (await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(legacyRoot.ToHtmlString()))).Contains("Ghi nhận nhân khẩu"))
        throw new Exception("Draft tab must show only pending rows and never a separate population completion action");
    var expectedPending = await db.HoSoKhaiSinhs.CountAsync(x => x.ModerationStatus == ModerationStatus.Pending);
    var pendingPagination = (PaginationState)birthPageType.GetField("draftPagination", flags)!.GetValue(legacyPage)!;
    if (pendingPagination.TotalItemCount != expectedPending)
        throw new Exception("Draft pagination total must exclude approved records before paging");
    var legacyRow = (await draftService.GetKhaiSinhDraftAsync(legacyRecord.Id)).Data!;
    await renderer.Dispatcher.InvokeAsync(async () => await (Task)birthPageType.GetMethod("ApproveDraft", flags)!.Invoke(legacyPage, [legacyRow])!);
    if (await db.BienDongDanCus.AnyAsync(x => x.Id == legacyRecord.Id))
        throw new Exception("UI must not register approved legacy data through a hidden completion action");
    var repaired = await draftService.ApproveKhaiSinhAsync(legacyRow.Id, legacyRow.PhienBan, chairman.UserName);
    if (!repaired.Success || !repaired.Data!.DaGhiNhan)
        throw new Exception("One-time legacy repair must still support already approved records");
    await renderer.Dispatcher.InvokeAsync(async () =>
    {
        birthPageType.GetProperty("RequestedDraftId")!.SetValue(legacyPage, legacyRecord.Id);
        await (Task)birthPageType.GetMethod("OnParametersSetAsync", flags)!.Invoke(legacyPage, null)!;
        typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(legacyPage, null);
    });
    var recordedHtml = await renderer.Dispatcher.InvokeAsync(() => WebUtility.HtmlDecode(legacyRoot.ToHtmlString()));
    if ((bool)birthPageType.GetField("showDrafts", flags)!.GetValue(legacyPage)!
        || !recordedHtml.Contains("Trẻ hồ sơ cũ giao diện") || recordedHtml.Contains("Duyệt hồ sơ")
        || recordedHtml.Contains("Ghi nhận nhân khẩu") || recordedHtml.Contains(">Sửa<"))
        throw new Exception("Approved deep link must open recorded tab with view-only details and no edit or approval actions");
    testUser.Current = currentAdmin;
    if ((await Render<ApprovalNotifications>()).Contains("Việc cần duyệt")) throw new Exception("Account without ctx must not see pending notifications");
    if (!(await Render<VillageNotifications>()).Contains("Thông báo theo thôn")) throw new Exception("Account without ctx must retain village notifications");
    Console.WriteLine("PASS: approval requires assigned ctx code and birth menu, green button approves without chairman enum and locks the form");
    Console.WriteLine("PASS: detail hides empty error, displays actual approval failure and clears it after success");
    Console.WriteLine("PASS: workspace pending notifications count/preview, open exact draft, disappear after approval and hide after permission revocation");
    Console.WriteLine("PASS: header badge includes pending approvals and decreases after approval");
    Console.WriteLine("PASS: pending-only draft rows and pagination; approval automatically creates child and recorded entry; approved details and deep links have no edit/approval/completion actions");
}
catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }

public class UnusedRegistry : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => throw new NotSupportedException("No exports or network calls during rendering.");
}
public sealed class BirthFormHarness : ComponentBase
{
    [Parameter] public Action<BirthEdit> Capture { get; set; } = default!;
    [Parameter] public Guid? DraftId { get; set; }
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<BirthEdit>(0);
        builder.AddAttribute(1, "DraftId", DraftId);
        builder.AddComponentReferenceCapture(2, component => Capture((BirthEdit)component));
        builder.CloseComponent();
    }
}
public sealed class ScreenHarness : ComponentBase
{
    [Parameter] public Type PageType { get; set; } = default!;
    [Parameter] public Action<ComponentBase> Capture { get; set; } = default!;
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, PageType);
        builder.AddComponentReferenceCapture(1, component => Capture((ComponentBase)component));
        builder.CloseComponent();
    }
}
sealed class NoJavaScript : IJSRuntime
{
    public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => throw new NotSupportedException("SSR must not call JavaScript.");
    public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
}
sealed class TestNavigation : NavigationManager
{
    public TestNavigation() => Initialize("http://localhost/", "http://localhost/bien-dong");
    protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
}
sealed class TestUser : IUserService
{
    public CurrentUserDto Current { get; set; } = new() { IsAuthenticated = true, UserName = "ui-test", Role = "Admin" };
    public Task<CurrentUserDto> GetCurrentUserAsync() => Task.FromResult(Current);
}
