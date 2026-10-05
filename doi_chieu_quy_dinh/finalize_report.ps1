$ErrorActionPreference = 'Stop'
$reportDir = 'D:\Đồ Án\Bao_cao_tot_nghiep_Dao_Van_Hieu'
$reportStem = 'Bao_cao_DATN_Dao_Van_Hieu_2200454'
$taskWord = New-Object -ComObject Word.Application
$taskWord.Visible = $false
$taskWord.DisplayAlerts = 0
$taskWord.AutomationSecurity = 3
$taskDoc = $taskWord.Documents.Open((Join-Path $reportDir ($reportStem + '.docx')), $false, $false)
$taskDoc.Fields.Update() | Out-Null
foreach ($tocStyleId in @(-20,-21,-22)) {
    $tocStyle = $taskDoc.Styles.Item($tocStyleId)
    $tocStyle.Font.Name = 'Times New Roman'
    $tocStyle.Font.Size = 13
    $tocStyle.Font.Color = 0
    $tocStyle.ParagraphFormat.SpaceAfter = 0
    $tocStyle.ParagraphFormat.SpaceBefore = 0
    $tocStyle.ParagraphFormat.LineSpacingRule = 5
    $tocStyle.ParagraphFormat.LineSpacing = 15.6
}
$taskDoc.Repaginate()
foreach ($toc in $taskDoc.TablesOfContents) {$toc.Update(); $toc.Range.ParagraphFormat.SpaceBefore=0; $toc.Range.ParagraphFormat.SpaceAfter=0; $toc.Range.ParagraphFormat.LineSpacingRule=5; $toc.Range.ParagraphFormat.LineSpacing=15.6}
$taskDoc.Fields.Update() | Out-Null
$taskDoc.Repaginate()
$taskDoc.Save()
$taskDoc.ExportAsFixedFormat('D:\Đồ Án\doi_chieu_quy_dinh\report_v3.pdf',17)
Write-Output ('DOCX pages: ' + $taskDoc.ComputeStatistics(2))
$taskDoc.SaveAs2((Join-Path $reportDir ($reportStem + '.doc')),0)
$taskDoc.Fields.Update() | Out-Null
$taskDoc.Repaginate()
foreach ($toc in $taskDoc.TablesOfContents) {$toc.Update(); $toc.Range.ParagraphFormat.SpaceBefore=0; $toc.Range.ParagraphFormat.SpaceAfter=0; $toc.Range.ParagraphFormat.LineSpacingRule=5; $toc.Range.ParagraphFormat.LineSpacing=15.6}
$taskDoc.Fields.Update() | Out-Null
$taskDoc.Save()
$taskDoc.ExportAsFixedFormat('D:\Đồ Án\doi_chieu_quy_dinh\report_final.pdf',17)
Write-Output ('DOC pages: ' + $taskDoc.ComputeStatistics(2))
$taskDoc.Close(0)

