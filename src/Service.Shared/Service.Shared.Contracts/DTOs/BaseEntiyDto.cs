// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

using System;
using Service.Shared.Commons.Extensions;
using Service.Shared.Commons.Model.SQL;

namespace Service.Shared.Contracts.DTOs
{
    public abstract class BaseEntiyDto
    {
        /// <summary>
        /// ID cho bản ghi
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Thời gian tạo cho bản ghi (lưu dưới dạng UTC)
        /// </summary>
        private DateTime _created;
        public DateTime Created
        {
            get
            {
                // Chuyển đổi từ UTC sang GMT+7
                return _created;
            }
            set
            {
                // Lưu vào dưới dạng UTC
                _created = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Thời gian chỉnh sửa cuối cho bản ghi (lưu dưới dạng UTC)
        /// </summary>
        private DateTime _lastModified;
        public DateTime LastModified
        {
            get
            {
                // Chuyển đổi từ UTC sang GMT+7
                return _lastModified;
            }
            set
            {
                // Lưu vào dưới dạng UTC
                _lastModified = value;
            }
        }

        /// <summary>
        /// Người tạo đầu tiên
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Người sửa cuối cùng
        /// </summary>
        public Guid LastModifiedBy { get; set; }

        /// <summary>
        /// Trạng thái duyệt của bản ghi
        /// </summary>
        public ModerationStatus ModerationStatus { get; set; }

        /// <summary>
        /// Phần chú thích của trạng thái duyệt
        /// </summary>
        public string ModerationStatusTxt => SIConvert.GetEnumDescription(ModerationStatus);
    }

    public abstract class BaseEntityDto : BaseEntiyDto
    {
    }
}
