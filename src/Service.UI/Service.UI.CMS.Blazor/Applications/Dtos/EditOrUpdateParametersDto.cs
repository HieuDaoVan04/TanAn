// "Một sản phẩm của HieuDV"

using System;
using Microsoft.AspNetCore.Components;

namespace Service.UI.CMS.Blazor.Applications.Dtos
{
    public class EditOrUpdateParametersDto
    {
        public Guid Id { get; set; }
        public string? Parameter { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsChangePasswordMode { get; set; }
        public EventCallback OnRefresh { get; set; }
    }
}
