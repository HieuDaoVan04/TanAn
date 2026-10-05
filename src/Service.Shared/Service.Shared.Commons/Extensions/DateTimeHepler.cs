// "Một sản phẩm của HieuDV"

using System.Globalization;

namespace Service.Shared.Commons.Extensions
{
    public static class DateTimeHepler
    {
        public static CultureInfo CreateCustomCultureForDatePicker()
        {
            var culture = new CultureInfo("vi-VN");
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            return culture;
        }
    }
}
