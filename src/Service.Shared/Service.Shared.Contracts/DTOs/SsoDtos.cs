// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.Shared.Contracts.DTOs
{
    public class SsoUserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Sub { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
