// "Một sản phẩm của HieuDV"

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
#pragma warning disable

namespace GenarateCodeByEntitys
{
    internal class Program
    {
        public static string filePathLog = "data/runninglog.txt";
        public static string NguoiDangThaoTac = "SIMAX AutoMapping";
        static void Main(string[] args)
        {
            Assembly.Load("Service.TanAn.Domain");
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            GhiLogVaoFile(TypeError.Info, "Phần mềm đang khởi động");
            ClearLogFile(filePathLog);

            Type classType = null;
            while (classType == null)
            {
                GhiLogVaoFile(TypeError.Info, $"Nhập vào tên class muốn mapping với Sharepoint");
                string className = System.Console.ReadLine();

                try
                {
                    classType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a =>
                        {
                            try
                            {
                                return a.GetTypes();
                            }
                            catch (ReflectionTypeLoadException ex)
                            {
                                return new Type[0];
                            }
                        })
                        .FirstOrDefault(t => t.FullName.Equals($"Service.TanAn.Domain.Entities.{className}"));
                }
                catch (Exception ex)
                {
                    GhiLogVaoFile(TypeError.Error, $"Lỗi khi tìm kiếm class {className}: {ex.Message}");
                    continue;
                }
                if (classType == null)
                {
                    GhiLogVaoFile(TypeError.Error, $"Không tìm thấy class có tên '{className}'");
                    EnterToContinute();
                }
                else break;
            }

            GhiLogVaoFile(TypeError.Info, $"Tạo file template");
            GenarateTemplateMVC(classType);
            GhiLogVaoFile(TypeError.Info, $"Tạo thành công");

        }

        private static void GenarateTemplateMVC(Type listItemType)
        {
            string entityName = listItemType.Name;
            string outputDirectoryPath = $"Output/{entityName}";

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            GenarateTemplateContractDtoClass(entityName, listItemType);
            GenarateTemplateContractFormClass(entityName, listItemType);
            GenarateTemplateContractQuery(entityName, listItemType);

            GenarateTemplateIRepository(entityName, listItemType);
            GenarateTemplateRepository(entityName, listItemType);

            GenarateTemplateIService(entityName, listItemType);
            GenarateTemplateService(entityName, listItemType);

            GenarateTemplateController(entityName, listItemType);

            GenarateTemplateBlazorIndex(entityName, listItemType);
            GenarateTemplateBlazorIndexCs(entityName, listItemType);
            GenarateTemplateBlazorView(entityName, listItemType);
            GenarateTemplateBlazorEdit(entityName, listItemType);

            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputDirectoryPath),
                UseShellExecute = true
            });
        }

        private static void GenarateTemplateView(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/View.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, "View.cshtml");

            string fileContent = File.ReadAllText(inputFilePath);

            string columnReplacement = @"";

            var listCoreField = new string[8] { "Id", "CreatedBy", "CreatedByText", "LastModifiedBy", "LastModified", "LastModifiedByText", "ModerationStatusTxt", "Created" };
            foreach (PropertyInfo propertyInfo in listPropertyInfo)
            {
                if (!listCoreField.Contains(propertyInfo.Name) && propertyInfo.Name != "ModerationStatus")
                {
                    columnReplacement += $@"
                <tr>
                    <td>
                        <label>{propertyInfo.Name}</label>
                    </td>
                    <td>
                        <p class=""itemtext"">@Model.{propertyInfo.Name}</p>
                    </td>

                </tr>";
                }
            }
            fileContent = fileContent.Replace("@columnsTemplate@", columnReplacement);
            fileContent = fileContent.Replace("@Entity@", entityName);

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateIndex(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Index.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, "Index.cshtml");

            string fileContent = File.ReadAllText(inputFilePath);
            string columnReplacement = string.Empty;

            var listCoreField = new string[8] { "Id", "CreatedBy", "CreatedByText", "LastModifiedBy", "LastModified", "LastModifiedByText", "ModerationStatusTxt", "Created" };

            for (int i = 0; i < listPropertyInfo.Length; i++)
            {
                var propertyInfo = listPropertyInfo[i];
                if (i == 0)
                {
                    columnReplacement = $@"
                {{
                    'data': '{propertyInfo.Name}',
                    'title': '{propertyInfo.Name}',
                    'orderable': false,
                    'visible': true,
                    'render': function (data, type, row) {{
                        return `<a href='#' onclick='xem@Entity@(${{row.Id}})'>${{row.{propertyInfo.Name}}}</a>`;
                    }},
                }},";
                }
                else
                {
                    if (!listCoreField.Contains(propertyInfo.Name) && propertyInfo.Name != "ModerationStatus")
                    {
                        columnReplacement += $@"
                {{
                    'data': '{propertyInfo.Name}'," + $@"
                    'title': '{propertyInfo.Name}',
                    'orderable': false,
                    'visible': true
                }},";
                    }
                }

                if (i == 5) break;
            }

            columnReplacement += @"{
                    ""data"": 'ModerationStatusTxt',
                    ""title"": ""Trạng thái kiểm duyệt"",
                    ""render"": function (data, type, row) {
                            return `<td> <span class=""badge rounded-pill ${getModerationStatusClass(row.ModerationStatus)}"">${data}</span></td`;
                    },
                    ""orderable"": false,
                    ""width"": ""83px""
                },{
                    ""data"": '',
                    ""title"": ""Chức năng"",
                    ""render"": function (data, type, row) {
                        var html = `<div class='button-container-in-grid button-container-${row.Id}'>`;
                        html += `<a href='#' title=""Sửa "" data-permission="""" onclick='sua@Entity@(${row.Id})'><i class=""fa fa-pencil"" aria-hidden=""true""></i></a>`
                        if (row.ModerationStatus != 0)
                            html += `<a href='#' title=""Duyệt "" data-permission="""" onclick='duyet@Entity@(${row.Id})'><i class=""fa fa-check"" aria-hidden=""true""></i></a>`
                        else
                            html += `<a href='#' title=""Hủy duyệt"" data-permission="""" onclick='huyDuyet@Entity@(${row.Id})'><i class=""fa fa-times"" aria-hidden=""true""></i></a>`

                        html += ` <a href='#' title=""Xóa"" data-permission="""" onclick='xoa@Entity@(${row.Id})'><i class=""fa fa-trash-o"" aria-hidden=""true""></i></a>`;
                        return html
                    },
                    ""orderable"": false,
                    ""width"": ""83px""
                }";

            fileContent = fileContent.Replace("@columnsTemplate@", columnReplacement);
            fileContent = fileContent.Replace("@Entity@", entityName);

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateEdit(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";

            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Edit.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, "Edit.cshtml");

            string fileContent = File.ReadAllText(inputFilePath);

            string columnReplacement = @"";
            string moreScript = @"";

            var listCoreField = new string[8] { "Id", "CreatedBy", "CreatedByText", "LastModifiedBy", "LastModified", "LastModifiedByText", "ModerationStatusTxt", "Created" };
            foreach (PropertyInfo propertyInfo in listPropertyInfo)
            {
                if (!listCoreField.Contains(propertyInfo.Name) && propertyInfo.Name != "_ModerationStatus")
                {
                    string template = "";
                    string templateScript = "";

                    bool Require = false;
                    int MaxLength = 0;
                    RequiredAttribute requiredAttr = propertyInfo.GetCustomAttribute<RequiredAttribute>();
                    if (requiredAttr != null)
                    {
                        Require = true;
                    }

                    MaxLengthAttribute maxLengthAttr = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                    if (maxLengthAttr != null)
                    {
                        MaxLength = maxLengthAttr.Length;
                    }
                    string requiredtxt = Require ? "required" : string.Empty;
                    string maxLengthtxt = MaxLength > 0 ? $"maxlength=\"{MaxLength}\"" : "";

                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        template = $@" <div class=""col-md-6"">
                            <div class=""mb-3"">

                                <label asp-for=""{propertyInfo.Name}"" class=""form-label"">{propertyInfo.Name}</label>
                                <input asp-for=""{propertyInfo.Name}"" {maxLengthtxt} class=""form-control "" placeholder=""{propertyInfo.Name}"" {requiredtxt} />
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""invalid-feedback"">Trường này là bắt buộc</span>
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""valid-feedback"">Dữ liệu hợp lệ</span>
                            </div>
                        </div>";
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        template = $@" <div class=""col-md-6"">
                            <div class=""mb-3"">

                                <label asp-for=""{propertyInfo.Name}"" class=""form-label"">{propertyInfo.Name}</label>
                                <input asp-for=""{propertyInfo.Name}"" {maxLengthtxt} class=""form-control "" placeholder=""{propertyInfo.Name}"" {requiredtxt} />
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""invalid-feedback"">Trường này bắt buộc và kiểu dữ liệu là số</span>
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""valid-feedback"">Dữ liệu hợp lệ</span>
                            </div>
                        </div>";
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        template = $@" <div class=""col-md-12"">
                             <div class=""mb-3 mt-4 form-check"">
                                <input asp-for=""{propertyInfo.Name}"" class=""form-check-input"" placeholder=""{propertyInfo.Name}"" {requiredtxt} />
                                <label asp-for=""{propertyInfo.Name}"" class=""form-check-label"">{propertyInfo.Name}</label>
                                <span asp--for=""{propertyInfo.Name}"" class=""invalid-feedback"">Trường này bắt buộc và kiểu dữ liệu là số</span>
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""valid-feedback"">Dữ liệu hợp lệ</span>
                            </div>
                         </div>";
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        template = $@" <div class=""col-md-6"">
                            <div class=""mb-3"">

                                <label asp-for=""{propertyInfo.Name}"" class=""form-label"">{propertyInfo.Name}</label>
                                <input asp-for=""{propertyInfo.Name}"" type=""text"" value="""" {maxLengthtxt} class=""form-control "" placeholder=""{propertyInfo.Name}"" {requiredtxt} />
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""invalid-feedback"">Trường này bắt buộc và chọn từ bộ chọn ngày</span>
                                <span asp-validation-for=""{propertyInfo.Name}"" class=""valid-feedback"">Dữ liệu hợp lệ</span>
                            </div>
                        </div>";

                        templateScript = $@"
                            $('#{propertyInfo.Name}')
                                .datepicker()
                                .formatInputFriendly({{ type: 'date' }})
                                .on('change', function () {{
                                    $('#{propertyInfo.Name}').data('datepicker').updateViewByData();
                                }});";
                    }

                    moreScript += templateScript;
                    columnReplacement += template;
                }
            }
            fileContent = fileContent.Replace("@addingMoreScript@", moreScript);
            fileContent = fileContent.Replace("@columnsTemplate@", columnReplacement);
            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateService(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Service.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Service.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());
            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateContractDtoClass(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/ContractDto.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Dto.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateContractFormClass(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/ContractForm.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Form.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateController(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Controller.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Controller.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateBlazorIndex(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}/{entityName}";
            if (!Directory.Exists(outputDirectoryPath))
                Directory.CreateDirectory(outputDirectoryPath);

            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/BlazorIndex.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"Index.razor");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateBlazorIndexCs(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/BlazorIndex.cs.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"Index.razor.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateBlazorView(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/BlazorView.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"View.razor");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateBlazorEdit(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/BlazorEdit.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"Edit.razor");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateIService(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/IService.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"I{entityName}Service.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());
            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateIRepository(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/IRepository.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"I{entityName}Repository.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());

            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateRepository(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Repository.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Repository.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());
            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateContractQuery(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/ContractQuery.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Query.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());
            File.WriteAllText(outputFilePath, fileContent);
        }

        private static void GenarateTemplateDto(string entityName, Type listItemType)
        {
            var listPropertyInfo = listItemType.GetProperties();
            string outputDirectoryPath = $"Output/{entityName}";
            var entityNameRaw = entityName;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string inputFilePath = Path.Combine(projectDirectory, "Templates/Dto.txt");
            string outputFilePath = Path.Combine(outputDirectoryPath, $"{entityName}Dto.cs");

            string fileContent = File.ReadAllText(inputFilePath);

            fileContent = fileContent.Replace("@Entity@", entityName);
            fileContent = fileContent.Replace("@EntityRaw@", entityNameRaw);
            fileContent = fileContent.Replace("@cmt1@", GetComment());
            fileContent = fileContent.Replace("@cmt2@", GetComment());
            File.WriteAllText(outputFilePath, fileContent);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            GhiLogVaoFile(TypeError.Info, "Chương trình đã đóng");
            OpenLogFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePathLog));
        }

        private static void OpenLogFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Lỗi khi mở file log: {ex.Message}");
            }
        }

        private static void GhiLogVaoFile(TypeError type, string text)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            string logging = "";
            switch (type)
            {
                case TypeError.Info:
                default:
                    logging = $"[Info]   {text}";
                    break;
                case TypeError.Error:
                    logging = $"[Error]   {text}";
                    break;
                case TypeError.Warning:
                    logging = $"[Warning]   {text}";
                    break;

            }
            System.Console.WriteLine(logging);
            AppendLogToFile(filePathLog, logging);
        }

        private static void EnterToContinute()
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            System.Console.WriteLine("Bạn có muốn tiếp tục không? Nhấn Enter để tiếp tục, nhập bất kỳ phím nào khác để dừng.");
            ConsoleKeyInfo keyInfo = System.Console.ReadKey(true);
            if (keyInfo.Key != ConsoleKey.Enter)
            {
                GhiLogVaoFile(TypeError.Info, "Người dùng không enter. Chương trình đóng");
                Environment.Exit(0);
            }
            else GhiLogVaoFile(TypeError.Info, "Đang tiếp tục công việc...");
        }

        private static void AppendLogToFile(string filePath, string logMessage)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {logMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Lỗi khi ghi log: {ex.Message}");
            }
        }

        private static void ClearLogFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine(string.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Lỗi khi xóa log: {ex.Message}");
            }
        }

        private static string GetComment()
        {
            string author = "HieuDV";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string mixedAuthor = MixCharacters(author);
            string comment = CreateAuthorComment(mixedAuthor, timestamp);
            return comment;
        }

        private static string MixCharacters(string input)
        {
            char[] characters = input.ToCharArray();
            StringBuilder mixed = new StringBuilder();
            foreach (char c in characters)
            {
                mixed.Append(c);
                if (c == 32) continue;
                mixed.Append(GenerateRandomCharacter());
            }
            return mixed.ToString();
        }

        private static char GenerateRandomCharacter()
        {
            const string chars = "!@#$%^&*()-_+=<>?/.,;:'\"[]{}|\\ `~";
            Random random = new Random();
            return chars[random.Next(chars.Length)];
        }

        private static string CreateAuthorComment(string author, string timestamp)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine($" * Tác giả: {author}");
            sb.AppendLine($" * Ngày tạo: {timestamp}");
            sb.AppendLine(" * Lưu ý: Vui lòng không chỉnh sửa hoặc xóa bình luận này.");
            sb.AppendLine(" * Bình luận này được tạo tự động và chứa dữ liệu duy nhất.");
            sb.AppendLine(" */");
            return sb.ToString();
        }
    }

    public enum TypeError
    {
        Info = 1,
        Error = 2,
        Warning = 3,
    }
    public enum TypeRuning
    {
        Continute = 1,
        Stop = 2,
    }
}
