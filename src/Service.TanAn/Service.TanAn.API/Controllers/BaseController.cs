// "Một sản phẩm của HieuDV"

using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;

namespace Service.TanAn.API.Controllers
{
    /// <summary>
    /// Base Controller cho toàn bộ API trong Nền tảng số Xã Tân An
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// HieuDV
        /// </summary>
        protected CurrentUserDto CurrentUser { get; set; }

        /// <summary>
        /// Khởi tạo BaseController với RequestContext
        /// </summary>
        /// <param name="requestContext"></param>
        public BaseController(IRequestContext requestContext)
        {
            CurrentUser = requestContext.CurrentUser;
        }
    }
}


