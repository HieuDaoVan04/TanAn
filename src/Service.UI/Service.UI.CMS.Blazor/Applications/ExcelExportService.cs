using System.IO;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Service.UI.CMS.Blazor.Applications;

public class ExcelColumn<T>
{
    public string Header { get; set; }
    public Func<T, object?> Selector { get; set; }

    public ExcelColumn(string header, Func<T, object?> selector)
    {
        Header = header;
        Selector = selector;
    }

    public static implicit operator ExcelColumn<T>((string header, Func<T, object?> selector) tuple)
        => new ExcelColumn<T>(tuple.header, tuple.selector);
}

public interface IExcelExportService
{
    byte[] ExportToExcel<T>(string sheetName, IEnumerable<T> items, IEnumerable<ExcelColumn<T>> columns);
    Task DownloadExcelAsync<T>(IJSRuntime js, string fileName, string sheetName, IEnumerable<T> items, IEnumerable<ExcelColumn<T>> columns);
}

public class ExcelExportService : IExcelExportService
{
    static ExcelExportService()
    {
        // EPPlus 8+ non-commercial license
        ExcelPackage.License.SetNonCommercialPersonal("TanAnAdmin");
    }

    public byte[] ExportToExcel<T>(string sheetName, IEnumerable<T> items, IEnumerable<ExcelColumn<T>> columns)
    {
        var columnList = columns.ToList();
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName);
        worksheet.View.ShowGridLines = true;

        // Header
        for (int col = 0; col < columnList.Count; col++)
        {
            var cell = worksheet.Cells[1, col + 1];
            cell.Value = columnList[col].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(27, 110, 194));
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.LightGray);
        }
        worksheet.Row(1).Height = 28;

        // Data Rows
        int rowIdx = 2;
        foreach (var item in items)
        {
            for (int col = 0; col < columnList.Count; col++)
            {
                var cell = worksheet.Cells[rowIdx, col + 1];
                var val = columnList[col].Selector(item);

                if (val is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else if (val is DateOnly d)
                {
                    cell.Value = d.ToDateTime(TimeOnly.MinValue);
                    cell.Style.Numberformat.Format = "dd/MM/yyyy";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else if (val is int or long or short or byte)
                {
                    cell.Value = Convert.ToInt64(val);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (val is decimal or double or float)
                {
                    cell.Value = Convert.ToDouble(val);
                    cell.Style.Numberformat.Format = "#,##0";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    cell.Value = val?.ToString() ?? string.Empty;
                }

                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(226, 232, 240));
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            worksheet.Row(rowIdx).Height = 22;
            rowIdx++;
        }

        // Auto-fit columns
        for (int col = 1; col <= columnList.Count; col++)
        {
            worksheet.Column(col).AutoFit();
            if (worksheet.Column(col).Width < 12)
                worksheet.Column(col).Width = 12;
        }

        return package.GetAsByteArray();
    }

    public async Task DownloadExcelAsync<T>(IJSRuntime js, string fileName, string sheetName, IEnumerable<T> items, IEnumerable<ExcelColumn<T>> columns)
    {
        var bytes = ExportToExcel(sheetName, items, columns);
        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
