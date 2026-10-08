// "Một sản phẩm của HieuDV"

using System;
using System.ComponentModel;
using System.Reflection;

namespace Service.Shared.Commons.Extensions
{
    public static class SIConvert
    {
        public static string GetEnumDescription(object? value)
        {
            if (value == null) return string.Empty;
            var type = value.GetType();
            var name = value.ToString();
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var field = type.GetField(name);
            if (field != null)
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null) return attr.Description;
            }
            return name;
        }

        public static string? ToAttributeValue(this Enum value)
        {
            if (value == null) return null;
            return Convert.ToInt32(value).ToString();
        }
    }
}
