// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

using System;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Layout.Component.Attachments
{
    public class ViewOrEditParametersDto : EditOrUpdateParametersDto
    {
        public string? FileName { get; set; }
        public bool IsToTrinh { get; set; }
        public bool IsPrimary { get; set; }
        public int? ParameterInt { get; set; }
        public object? DataObject { get; set; }
    }
}

namespace Service.UI.Blazor.Components.Layout.Component.Attachments
{
    public class ViewOrEditParametersDto : Service.UI.CMS.Blazor.Components.Layout.Component.Attachments.ViewOrEditParametersDto
    {
    }
}
