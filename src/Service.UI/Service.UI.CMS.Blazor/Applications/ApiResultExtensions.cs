using Service.Shared.Commons.Interfaces;

namespace Service.UI.CMS.Blazor.Applications;

public static class ApiResultExtensions
{
    public static void EnsureSuccess<T>(this ResultAPI<T> result)
    {
        if (!result.Success || (int)result.Status is < 200 or >= 300)
            throw new InvalidOperationException(result.Message ?? "Thao tác API không thành công.");
    }

    public static T RequireData<T>(this ResultAPI<T> result)
    {
        result.EnsureSuccess();
        return result.Data ?? throw new InvalidOperationException("API không trả về dữ liệu.");
    }
}
