# Kế hoạch tự điền form bằng template JSON qua API

## 1. Mục tiêu và phạm vi

API quản lý nhiều template, tra dữ liệu hộ/nhân khẩu khi được yêu cầu và trả các giá trị theo tên trường của form. Giao diện dùng một bộ xử lý chung để điền dữ liệu. Các màn hình vẫn có giao diện trực tiếp trong `Index.razor`, `Edit.razor`, `View.razor` và logic riêng trong `.razor.cs` theo yêu cầu hiện tại.

Đợt đầu áp dụng cho 6 màn hình Biến động, bắt đầu bằng Khai sinh. Template trong kế hoạch này là cấu hình tự điền dữ liệu, không phải mẫu Word, dữ liệu dân cư giả hoặc cơ chế tự sinh toàn bộ giao diện. Sinh giao diện từ metadata và xuất tờ khai là các khả năng mở rộng sau.

Kết quả mong muốn:

- Sửa quy tắc lấy địa chỉ, họ tên, giấy tờ trong một template thay vì sửa các phép gán trong từng màn hình.
- API đọc dữ liệu mới nhất trong phạm vi người dùng được phép xem.
- Cán bộ có thể bổ sung, sửa và xác nhận dữ liệu sau khi tự điền.
- Thêm template dùng cùng model và nguồn đã hỗ trợ chủ yếu là bổ sung JSON.
- Trường mới chưa có trong model hoặc nguồn dữ liệu mới vẫn cần sửa DTO, Razor hoặc provider tương ứng. JSON không thay thế việc phát triển nghiệp vụ.

## 2. Những điểm cần thay đổi trong dự án hiện tại

| Vị trí hiện tại | Công việc |
| --- | --- |
| `KhaiSinh/Edit.razor.cs`: `ApplyHousehold`, `FillApplicant`, `ResetHousehold` | Chuyển quy tắc điền/xóa trường sang template và bộ xử lý trạng thái |
| `KhaiSinh/ParentFields.razor.cs`: `FillPerson` | Phát sự kiện chọn cha/mẹ về form; API resolve và bộ xử lý chung điền các trường |
| `KhaiSinhForm`, `KhaiSinhParentForm` | Là model đích có kiểu dữ liệu; không đổi thành dictionary dùng để lưu nghiệp vụ |
| 5 màn hình còn lại: `Edit.razor.cs` | Chọn nhân khẩu và nhận `NhanKhauId`, dữ liệu hiển thị từ template tương ứng |
| `ApiServiceTransport.cs` | Hiện chỉ cho phép một số đường dẫn quản trị; thêm các đường dẫn template cụ thể |
| `ModuleRegistry.cs` | Hiện có nhánh gọi service cục bộ và nhánh HTTP; định tuyến template rõ ràng vào HTTP, tránh trả `Success` với `Data` rỗng |
| `PopulationService.KhaiSinh.cs` | Tiếp tục chịu trách nhiệm kiểm tra, tạo nhân khẩu, snapshot, transaction và chống lưu trùng |

Hiện form khai sinh gọi `IPopulationService` trực tiếp trong scope. Vì mục tiêu mới là pull qua API, cần bổ sung luồng HTTP cho tự điền, không chỉ chuyển các phép gán sang một class frontend khác.

## 3. Thiết kế template

Mỗi template có mã duy nhất, phiên bản bất biến, model đích và các sự kiện được hỗ trợ. Ví dụ:

```json
{
  "code": "khai-sinh",
  "version": 1,
  "model": "KhaiSinhForm",
  "menuPath": "/bien-dong/khai-sinh",
  "events": {
    "householdSelected": {
      "bindings": [
        { "target": "HoGiaDinhId", "source": "household.Id", "policy": "context" },
        { "target": "MaSoHo", "source": "household.MaSoHo", "policy": "context" },
        { "target": "TenChuHo", "source": "household.TenChuHo", "policy": "context" },
        { "target": "ThuongTru", "source": "household.DiaChi", "policy": "suggestion" },
        { "target": "Me.NoiCuTru", "source": "household.DiaChi", "policy": "suggestion" }
      ]
    }
  }
}
```

Đây là ví dụ rút gọn; template thực tế phải khai báo đầy đủ các trường, sự kiện và nhóm cần xóa khi đổi ngữ cảnh.

Quy ước:

- `target` trùng tên thuộc tính C#; trường lồng nhau dùng đường dẫn như `Me.HoTen`, `Cha.NgaySinh`.
- `source` tham chiếu các nguồn đã đăng ký, như `household`, `applicant`, `mother`, `father`, `resident`.
- `policy: context`: cập nhật lựa chọn hoặc dữ liệu xác định theo ngữ cảnh đang chọn.
- `policy: suggestion`: chỉ điền khi trống hoặc còn là giá trị tự điền trước đó; giữ giá trị cán bộ đã sửa trong cùng ngữ cảnh.
- Giá trị mặc định khai báo bằng `constant` hoặc hàm có tên đã hỗ trợ, ví dụ `today`. Không thực thi mã tùy ý từ JSON.
- Có thể bổ sung fallback có thứ tự, ví dụ thường trú của người yêu cầu rồi đến địa chỉ hộ.
- Phân biệt trường không có trong kết quả, trường được điền `null`, và trường cần xóa rõ ràng.
- Các giá trị chỉ mang tính mặc định như quốc tịch Việt Nam phải được nhận diện là mặc định, không ghi nhận là dữ liệu đã tra trong nhân khẩu.

Mapping không biến mất: nó được khai báo tập trung trong JSON và đọc bằng một bộ xử lý chung, thay cho phép gán thủ công ở nhiều màn hình.

## 4. API đề xuất

| API | Chức năng |
| --- | --- |
| `GET /api/v1/form-templates` | Danh sách template người dùng có quyền sử dụng, mã và phiên bản |
| `GET /api/v1/form-templates/{code}?version=1` | Metadata và quy tắc áp dụng được phép công khai cho frontend |
| `POST /api/v1/form-templates/{code}/resolve` | Kiểm tra ngữ cảnh, đọc dữ liệu và trả dữ liệu tự điền |

Các API yêu cầu phiên `TanAnSession` và kiểm tra quyền của menu gắn với template. API không nhận SQL, tên bảng hoặc tên kiểu .NET tùy ý từ caller.

Ví dụ request chọn người yêu cầu:

```json
{
  "version": 1,
  "event": "applicantSelected",
  "context": {
    "householdId": "11111111-1111-1111-1111-111111111111",
    "applicantId": "22222222-2222-2222-2222-222222222222"
  },
  "clientSequence": 8
}
```

Ví dụ phần `data` bên trong response của API:

```json
{
  "templateCode": "khai-sinh",
  "version": 1,
  "event": "applicantSelected",
  "clientSequence": 8,
  "data": {
    "NguoiYeuCauId": "22222222-2222-2222-2222-222222222222",
    "HoTenNguoiYeuCau": "Nguyễn Văn A",
    "SoGiayTo": "012345678901",
    "LoaiGiayTo": "CCCD",
    "NoiCuTruNguoiYeuCau": "Thôn A, xã Tân An"
  },
  "clearPaths": ["NgayCapGiayTo", "NoiCapGiayTo", "QuanHeVoiTre"]
}
```

`data` là tập giá trị theo đường dẫn thuộc tính model, có thể chứa khóa `Me.HoTen`; bộ xử lý không deserialize toàn bộ response thành form mới. Metadata cung cấp kiểu, chính sách và thông tin nguồn cho mỗi đường dẫn. Response dùng envelope `ApiResult<T>` hiện có; lớp HTTP chuyển đúng lỗi/status sang kết quả frontend.

Ngữ cảnh giao diện như thông tin hộ được chọn và danh sách thành viên trả riêng dưới `contextData`, dùng DTO cụ thể. Không nhồi dữ liệu hiển thị không thuộc form vào dictionary lưu nghiệp vụ.

Mã lỗi: 401 khi phiên không hợp lệ; 403 khi thiếu quyền menu; 404 khi template/đối tượng không có trong phạm vi nhìn thấy; 400 khi sự kiện hoặc ngữ cảnh sai; 409 khi phiên bản không khớp hoặc không còn được hỗ trợ.

## 5. Backend và nguồn dữ liệu

Các thành phần dự kiến:

| Thành phần | Trách nhiệm |
| --- | --- |
| `IFormTemplateStore` / `JsonFormTemplateStore` | Đọc cấu hình JSON đã triển khai, kiểm tra cấu trúc, trả theo mã + phiên bản |
| `IFormTemplateService` / `FormTemplateService` | Điều phối resolve, kiểm quyền, phiên bản, sự kiện và provider |
| `HouseholdTemplateSource`, `ResidentTemplateSource` | Tra hộ/nhân khẩu qua các truy vấn có phạm vi thôn |
| Bộ xử lý binding | Đọc source, áp dụng constant/fallback/biến đổi đã đăng ký, tạo kết quả |
| `FormTemplatesController` | Cung cấp các API, envelope và HTTP status |

Kiểm tra cấu hình khi khởi động: mã trùng, phiên bản trùng, target không tồn tại, target chỉ đọc, kiểu không phù hợp, nguồn/hàm chưa đăng ký và quyền menu không hợp lệ. Template lỗi không được đưa vào sử dụng; log chỉ rõ file và đường dẫn lỗi để sửa.

Khi resolve cha/mẹ/người yêu cầu, API kiểm tra người được chọn còn thuộc hộ và còn trong phạm vi truy cập. Không dựa vào danh sách thành viên cũ trên frontend. Mỗi yêu cầu đọc dữ liệu mới; chỉ cache metadata, không cache dữ liệu cá nhân dùng tự điền giữa các người dùng.

API tự điền chỉ đọc. API lưu tiếp tục kiểm tra đầy đủ và giữ giao dịch khai sinh hiện tại. `RequestId`, ngày sinh bằng chữ và các thuộc tính tính toán không được template ghi đè. Không thay đổi format snapshot cũ chỉ để triển khai tự điền.

## 6. Frontend và trạng thái form

Tạo `FormTemplateClient` để gọi API và `FormAutofillSession<T>` để quản lý một form đang mở. Session giữ:

- Template và phiên bản đang sử dụng.
- Các thuộc tính được phép cập nhật và kiểu đích.
- Giá trị tự điền lần trước, các trường đã được cán bộ sửa, ngữ cảnh nguồn.
- Số thứ tự request hiện tại; bỏ response cũ nếu cán bộ đã chọn hộ/người khác.

Luồng áp dụng: kiểm tra toàn bộ kết quả và chuyển kiểu vào vùng tạm trước, rồi cập nhật những trường được phép trên model hiện tại. Không thay cả form và không tạo `RequestId` mới sau mỗi lần pull. Thông báo thay đổi cho `EditContext`; phân biệt thay đổi do chương trình và thay đổi do cán bộ nhập.

Hỗ trợ string, GUID, nullable GUID, số, enum theo quy ước rõ ràng, ngày ISO và nullable date. Ngày nghiệp vụ không bị đổi sang hôm trước/hôm sau do chuyển múi giờ. Đường dẫn lồng nhau chỉ được tạo object khi nhóm đang bật và schema cho phép.

Quy tắc đổi ngữ cảnh:

| Tình huống | Xử lý |
| --- | --- |
| Mở form | Áp dụng default một lần; tạo `RequestId` một lần |
| Tra và chọn hộ | Resolve theo hộ; điền mã hộ, chủ hộ, gợi ý địa chỉ và dữ liệu hiển thị |
| Đổi hộ | Xóa lựa chọn người yêu cầu/cha/mẹ và dữ liệu thuộc lựa chọn cũ; giữ thông tin độc lập của trẻ như họ tên, ngày sinh |
| Địa chỉ trẻ đã sửa thủ công | Giữ theo chính sách suggestion; cán bộ kiểm tra lại khi đổi hộ |
| Tra hộ thất bại | Xóa ngữ cảnh hộ cũ, không cho lưu bằng ID cũ; giữ thông tin độc lập đã nhập |
| Đổi người yêu cầu/cha/mẹ | Xóa thông tin gắn với người cũ rồi điền người mới; không giữ giấy tờ của người cũ |
| Bỏ chọn người trong hộ | Xóa ID và dữ liệu phụ thuộc, chuyển sang nhập thủ công |
| Bỏ nhóm cha/mẹ | Không tự tạo lại nhóm vì response; chỉ lưu nhóm đang bật |
| API lỗi hoặc trả response cũ | Không áp dụng một phần kết quả; thông báo để tra lại |

Tại khai sinh cần thống nhất model cha/mẹ và bộ theo dõi trạng thái: các phần nhập cha/mẹ phải liên kết với root form hoặc đăng ký đường dẫn rõ ràng, thay vì để bộ xử lý cập nhật `form.Me` trong khi giao diện đang bind một object `mother` khác. Khi bỏ/bật nhóm có thể giữ bản nháp riêng, nhưng payload lưu chỉ chứa nhóm đang bật.

`Edit.razor` vẫn chứa các input. `.razor.cs` giữ xử lý sự kiện chọn nguồn, gọi client, áp dụng kết quả và lưu; không còn các dãy phép gán theo từng trường lấy từ household/resident. `ParentFields` chỉ hiển thị trường và phát sự kiện lựa chọn về form.

## 7. Vị trí file dự kiến

```text
Service.Shared.Contracts/DTOs/FormTemplates/
    FormTemplateDefinition.cs
    FormTemplateResolveRequest.cs
    FormTemplateResolveResult.cs

Service.TanAn.Application/
    Interfaces/IFormTemplateService.cs
    Interfaces/IFormTemplateStore.cs
    Services/FormTemplates/
        FormTemplateService.cs
        HouseholdTemplateSource.cs
        ResidentTemplateSource.cs

Service.TanAn.API/
    Controllers/v1/FormTemplatesController.cs
    FormTemplates/
        khai-sinh.v1.json
        khai-tu.v1.json
        tam-tru.v1.json
        tam-vang.v1.json
        chuyen-den.v1.json
        chuyen-di.v1.json
    Services/JsonFormTemplateStore.cs

Service.UI.CMS.Blazor/Applications/FormTemplates/
    FormTemplateClient.cs
    FormAutofillSession.cs
    FormValueConverter.cs

Service.UI.CMS.Blazor/Components/Pages/BienDong/
    KhaiSinh/      # Giữ giao diện riêng, tích hợp autofill
    KhaiTu/
    TamTru/
    TamVang/
    ChuyenDen/
    ChuyenDi/
```

Tên file có thể điều chỉnh khi triển khai, nhưng giữ ranh giới contracts, resolve phía API và áp dụng giá trị phía UI.

## 8. Các bước triển khai và tiêu chí hoàn thành

| Bước | Công việc | Tiêu chí hoàn thành |
| --- | --- | --- |
| 1. Hợp đồng | Chốt schema, event, kiểu dữ liệu, policy, error và version; lập bảng trường khai sinh | Mọi trường hiện tự điền/xóa đều có quy tắc rõ ràng; không tự suy ra quan hệ cha/mẹ |
| 2. Backend | Store JSON, source provider, binding processor, resolve API, đăng ký DI và cấu hình publish | Có thể gọi resolve khai sinh bằng HTTP; đúng quyền, phạm vi và phiên bản |
| 3. Frontend dùng chung | Client, bộ chuyển kiểu, cập nhật model, dirty state, request sequence và EditContext | Một hàm áp dụng được nhiều trường, không mất dữ liệu thủ công hoặc thay RequestId |
| 4. Khai sinh thử nghiệm | Thay autofill hộ, người yêu cầu và cha/mẹ; thống nhất model nhóm; giữ giao diện | Hành vi hiện có vẫn đúng; không còn mapping household/resident theo từng field ở màn hình |
| 5. Năm màn hình còn lại | 5 JSON và sự kiện chọn nhân khẩu, default phù hợp với loại trang | Mỗi màn hình dùng template đúng loại và vẫn có Razor riêng |
| 6. Kiểm tra và tài liệu | Kiểm tra HTTP, model, tương tác, snapshot, rollback, quyền, publish JSON; cập nhật hướng dẫn | Bộ kiểm tra qua; chạy được khi triển khai ngoài thư mục source |

Ở 5 màn hình còn lại, `CreateBienDongForm` hiện chủ yếu nhận `NhanKhauId`, loại, ngày, nơi đi/đến và lý do. Không thêm các thông tin khai sinh vào 5 template này. Dữ liệu họ tên/CCCD/hộ phục vụ hiển thị trả ở `contextData`; loại không lấy từ nhân khẩu và vẫn được cố định tại màn hình khi gọi luồng lưu hiện có. Nếu chuyển cả thao tác lưu sang API template chung ở một đợt sau, API phải kiểm tra mã template/menu và xác định loại hợp lệ phía server.

## 9. Kiểm tra bắt buộc

- Template sai target/source/type bị phát hiện trước khi sử dụng.
- HTTP yêu cầu phiên và đúng menu; đổi ID không đọc được hộ ngoài thôn.
- Chọn người không thuộc hộ bị từ chối; trường hợp thôn/quyền bị thu hồi được kiểm tra lại.
- Áp dụng string, GUID, enum, ngày, null và đường dẫn cha/mẹ đúng kiểu.
- Chỉ xóa trường có trong `clearPaths`; trường thiếu trong `data` không bị tự động đặt về default.
- Không ghi đè tên/ngày sinh trẻ hoặc giá trị cán bộ đã chỉnh ngoài quy tắc đổi ngữ cảnh.
- Đổi hộ, chọn người khác, bỏ nhóm và response đến chậm không để lại dữ liệu cũ.
- Model/EditContext còn dùng đúng object sau khi tự điền; validation hiển thị đúng.
- Lưu khai sinh vẫn tạo đúng một nhân khẩu và một biến động; retry dùng cùng RequestId; lỗi ghi/audit rollback.
- 5 loại khác vẫn lưu đúng loại và giữ các bộ lọc, xem chi tiết, xuất Excel hiện có.
- JSON có trong output/publish và loader không phụ thuộc cwd của máy phát triển.

Mở rộng các bộ kiểm tra `PopulationUiChecks`, `AdministrationApiChecks`, `VillageScopeChecks`, `EntityModelChecks`; thêm bộ kiểm tra template riêng khi cần kiểm chứng engine.

## 10. Database, phiên bản và mở rộng

Đợt đầu lưu template JSON trong source của API, được đóng gói khi publish và tải khi khởi động. Không cần migration database; sửa template cần triển khai lại cấu hình/API. Metadata có thể cache theo mã + version; form đang mở dùng phiên bản đã nhận và không tự nhảy sang bản khác.

Phiên bản đã phát hành không sửa nội dung tại chỗ. Thay đổi cấu hình tạo phiên bản mới; API duy trì các phiên bản đang hỗ trợ để form đang mở hoạt động ổn định. DTO và API lưu vẫn là hợp đồng có kiểm soát, không được thay đổi chỉ vì template gửi thêm khóa.

Sau khi khai sinh vận hành ổn định có thể làm màn hình quản trị template: lưu các phiên bản trong DB, chỉnh sửa, xem trước, kiểm tra schema, duyệt và kích hoạt. Giai đoạn đó mới cần migration và quyết định có lưu mã/phiên bản template cùng hồ sơ để truy vết. Sinh giao diện từ JSON, xuất Word/PDF và các nghiệp vụ khác là các đợt mở rộng riêng.

## 11. Thứ tự đề xuất

Triển khai trọn luồng khai sinh trước: JSON -> resolve API -> bộ áp dụng frontend -> kiểm tra và lưu hồ sơ. Khi đạt tiêu chí mới nhân rộng sang 5 màn hình còn lại. Điều này kiểm chứng đủ hộ, người yêu cầu, nhóm cha/mẹ, dữ liệu lồng nhau và quy tắc đổi ngữ cảnh trước khi phổ biến cơ chế template.
