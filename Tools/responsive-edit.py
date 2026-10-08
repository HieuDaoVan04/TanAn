from pathlib import Path
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/MainLayout.razor');s=p.read_text(encoding='utf-8-sig').replace('<FluentLayout>','<FluentLayout Class="app-shell">').replace('Style="height: calc(100dvh - 90px);"','Class="workspace-body"');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/NavMenu.razor');s=p.read_text(encoding='utf-8-sig').replace('title="Menu expand/collapse toggle"','title="Mở hoặc đóng menu" @bind="mobileExpanded"').replace('    <nav class="sitenav"','    <label for="navmenu-toggle" class="nav-backdrop" aria-label="Đóng menu"></label>\n    <nav class="sitenav"');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/NavMenu.razor.cs');s=p.read_text(encoding='utf-8-sig').replace('        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => _ = InvokeAsync(Refresh);','        private bool mobileExpanded;\n        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => _ = InvokeAsync(async () => { mobileExpanded = false; await Refresh(); });');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/Header.razor');s=p.read_text(encoding='utf-8-sig').replace('    <span class="home-link__text">','    <span class="home-link__mobile">UBND xã Tân An</span>\n    <span class="home-link__text">');p.write_text(s,encoding='utf-8')
import re
for p in Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages').rglob('*.razor'):
 s=p.read_text(encoding='utf-8-sig')
 s2=re.sub(r'<FluentStack\b[^>]*Orientation="Orientation.Horizontal"[^>]*>',lambda m:m[0] if 'Class=' in m[0] else m[0][:-1]+' Class="page-toolbar">',s)
 if s2!=s:p.write_text(s2,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Shared/HouseholdMembers.razor');s=p.read_text(encoding='utf-8').replace('style="display:grid;gap:10px;grid-template-columns:1fr 1fr"','class="data-form"');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/AIChatbot/Index.razor');s=p.read_text(encoding='utf-8-sig').replace('height: 600px','height: clamp(360px, 65dvh, 760px)');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/App.razor');s=p.read_text(encoding='utf-8-sig').replace('    <HeadOutlet />','    <link rel="stylesheet" href="css/responsive.css" />\n    <HeadOutlet />');p.write_text(s,encoding='utf-8')
Path('src/Service.UI/Service.UI.CMS.Blazor/wwwroot/css/responsive.css').write_text('''/* Shared viewport sizing for all Tân An workspaces. */
:root { --workspace-padding: clamp(12px, 1.6vw, 28px); --sidebar-width: clamp(236px, 19vw, 292px); }
*, *::before, *::after { box-sizing: border-box; }
html, body { width:100%; min-width:320px; height:100%; }
.app-shell { display:flex; flex-direction:column; height:100dvh; min-height:0; width:100%; overflow:hidden; }
.app-shell .siteheader { display:flex; flex:0 0 auto; min-height:56px; height:auto; align-items:center; gap:8px; padding:8px 16px; }
.siteheader .home-link { min-width:0; flex:1 1 auto; }
.siteheader .home-link__text { line-height:1.35; font-size:clamp(.8rem,1vw,1rem); overflow-wrap:anywhere; }
.home-link__mobile { display:none; }
.siteheader .settings, .siteheader .User { flex-shrink:0; margin:0 !important; padding:0; }
.siteheader .User > .fluent-stack { padding-inline-end:0 !important; }
.app-shell .body-stack { display:flex; flex:1 1 auto; min-height:0; min-width:0; overflow:hidden; flex-direction:row !important; align-items:stretch; }
.app-shell .navmenu { flex:0 0 var(--sidebar-width); min-height:0; overflow:hidden; }
.app-shell nav.sitenav { width:100%; height:100%; padding:16px 10px; overflow-y:auto; overscroll-behavior:contain; }
.app-shell #body-content { flex:1 1 0; min-width:0; min-height:0; height:auto; overflow:auto; overscroll-behavior:contain; }
.app-shell .content { display:block; min-width:0; width:100%; padding:0; }
.app-shell #article { width:100%; min-width:0; max-width:none; margin:0; padding:var(--workspace-padding) !important; }
.app-shell footer { flex:0 0 auto; min-height:32px; height:auto; display:flex; flex-wrap:wrap; gap:4px 12px; padding:6px 16px; font-size:.75rem; line-height:1.4; }
#article > *, #article .fluent-stack, #article .fluent-grid-item, #article .fluent-card { min-width:0; max-width:100%; }
#article h1 { font-size:clamp(1.4rem,2vw,1.85rem); line-height:1.3; }
#article h2 { font-size:clamp(1.15rem,1.5vw,1.5rem); line-height:1.35; margin-bottom:16px; overflow-wrap:anywhere; }
.page-toolbar, .toolbar, .toolbar-actions, .toolbar-filters, .data-toolbar, .dialog-actions { flex-wrap:wrap; gap:10px; min-width:0; }
.page-toolbar fluent-search, .page-toolbar fluent-select, .toolbar fluent-search { max-width:100%; min-width:0; }
.page-toolbar > .page-toolbar { flex-wrap:wrap; }
.data-grid-scroll, .table-scroll, .unit-table { width:100%; max-width:100%; min-width:0; overflow:auto; overscroll-behavior-x:contain; }
.data-grid-scroll .fluent-data-grid, .table-scroll .fluent-data-grid { min-width:840px; }
.fluent-data-grid th, .fluent-data-grid td { overflow-wrap:anywhere; }
.fluent-data-grid .col-title-text { white-space:normal; line-height:1.35; }
.fluent-paginator, .paginator, .grid-footer { max-width:100%; flex-wrap:wrap; gap:8px; }
fluent-dialog { max-width:100vw; max-height:100dvh; }
fluent-dialog::part(control) { max-width:calc(100vw - 24px); max-height:calc(100dvh - 24px); overflow:auto; }
.data-dialog, .editor { max-width:100%; max-height:calc(100dvh - 48px); overflow:auto; }
.data-dialog header, .editor-heading { gap:12px; }
.data-dialog input:not([type=checkbox]), .data-dialog select, .data-dialog textarea, .editor input:not([type=checkbox]), .editor select, .editor textarea { min-width:0; max-width:100%; }
.full-width { grid-column:1/-1; }
.data-dialog .fluent-data-grid { min-width:540px; }
.data-dialog:has(.fluent-data-grid) { overflow:auto; }
.nav-backdrop { display:none; }
@media(min-width:900px) {
    .app-shell .navmenu-icon { display:none; }
    .app-shell nav.sitenav { display:block !important; }
}
@media(max-width:1100px) {
    .siteheader .home-link__text { max-width:480px; }
    .page-toolbar { justify-content:flex-start; }
    .page-toolbar > .page-toolbar { flex:1 1 auto; }
    .page-toolbar fluent-search { flex:1 1 220px; }
}
@media(max-width:899px) {
    .app-shell .siteheader { height:56px; min-height:56px; flex-wrap:nowrap; padding:6px 10px 6px 52px; }
    .siteheader .home-link__text { display:none; }
    .home-link__mobile { display:block; margin-left:8px; font-weight:600; line-height:1.25; }
    .siteheader > a:not(.home-link), .siteheader > fluent-divider { display:none !important; }
    .app-shell .navmenu { flex:0 0 0; width:0; overflow:visible; }
    .app-shell .navmenu-icon { display:flex; align-items:center; justify-content:center; position:fixed; left:8px; right:auto; top:10px; width:36px; height:36px; z-index:40; cursor:pointer; }
    .app-shell input.navmenu-icon { opacity:0; pointer-events:none; }
    .app-shell input.navmenu-icon:focus-visible + label { outline:2px solid var(--accent-fill-rest); outline-offset:2px; }
    .app-shell #navmenu-toggle ~ nav.sitenav { display:none; position:fixed; top:56px; bottom:0; left:0; width:min(300px,88vw); height:calc(100dvh - 56px); background:var(--neutral-layer-1); z-index:32; box-shadow:8px 0 24px #0002; }
    .app-shell #navmenu-toggle:checked ~ nav.sitenav { display:block; }
    .app-shell #navmenu-toggle:checked ~ .nav-backdrop { display:block; position:fixed; inset:56px 0 0; background:#0005; z-index:31; }
    .app-shell footer { padding:6px 12px; }
    .app-shell footer > p:first-child, .app-shell footer .fluent-spacer { display:none; }
}
@media(max-width:600px) {
    .siteheader .home-link__icon { display:none; }
    .siteheader .home-link__mobile { margin-left:0; font-size:.85rem; }
    .siteheader .User .fluent-profile-menu { max-width:48px; }
    .page-toolbar, .toolbar { align-items:stretch; }
    .page-toolbar fluent-search, .page-toolbar fluent-select, .toolbar-filters { width:100% !important; flex-basis:100%; }
    .page-toolbar > .page-toolbar { width:100%; }
    .data-dialog, .editor { padding:14px !important; }
    .data-dialog .data-form, .editor .form-grid { grid-template-columns:minmax(0,1fr) !important; }
    .data-dialog dl { grid-template-columns:minmax(0,1fr) !important; gap:4px; }
    .data-dialog dd { margin:0 0 10px; }
    .dialog-actions { justify-content:flex-start; }
    .login-card { padding:24px !important; }
    fluent-dialog::part(control) { max-width:calc(100vw - 16px); }
}
''',encoding='utf-8')
