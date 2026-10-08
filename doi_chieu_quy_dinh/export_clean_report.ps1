$ErrorActionPreference='Stop'
$root='D:\Đồ Án\Bao_cao_tot_nghiep_Dao_Van_Hieu\Bao_cao_DATN_Dao_Van_Hieu_2200454'
$w=New-Object -ComObject Word.Application
$w.Visible=$false;$w.DisplayAlerts=0;$w.AutomationSecurity=3
$d=$w.Documents.Open(($root+'.docx'),$false,$false)
$d.Repaginate()
foreach($f in $d.Fields){if($f.Type -ne 13){$f.Update() | Out-Null}}
foreach($t in $d.TablesOfContents){$t.UpdatePageNumbers()}
$d.Repaginate();$d.Save()
$d.ExportAsFixedFormat('D:\Đồ Án\doi_chieu_quy_dinh\report_v3.pdf',17)
$d.SaveAs2(($root+'.doc'),0)
$d.Close(0)
$d=$w.Documents.Open(($root+'.doc'),$false,$true)
$d.Repaginate()
$d.ExportAsFixedFormat('D:\Đồ Án\doi_chieu_quy_dinh\report_final.pdf',17)
Write-Output ('Reopened DOC pages: '+$d.ComputeStatistics(2))
$d.Close(0)
