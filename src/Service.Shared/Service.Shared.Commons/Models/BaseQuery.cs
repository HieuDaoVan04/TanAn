// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

using Service.Shared.Commons.Querys.Grid;

namespace Service.Shared.Commons.Models
{

    public class BaseQuery
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public GridRequest gridRequest { get; set; } = new GridRequest();
        public int draw { get; set; }
        public string? Keyword { get; set; }
        public List<string>? SearchIn { get; set; }
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
        public bool isgetBylisID { get; set; }
        public List<Guid> lstIDGet { get; set; } = new List<Guid>();
    }

    public class DataTableJson<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public object data { get => Data; set { if (value is List<T> list) Data = list; } }
        public int Total { get; set; }
        public int recordsTotal { get => Total; set => Total = value; }
        public int RecordsTotal { get => Total; set => Total = value; }
        public int recordsFiltered { get => Total; set => Total = value; }
        public int draw { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? querytext { get; set; }

        public DataTableJson() { }

        public DataTableJson(List<T> data, int total, int pageIndex = 1, int pageSize = 10)
        {
            Data = data;
            Total = total;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public DataTableJson(List<T> data, int drawOption, int total)
        {
            Data = data;
            draw = drawOption;
            Total = total;
            recordsTotal = total;
            recordsFiltered = total;
        }
    }

    public class DataTableJson : DataTableJson<object>
    {
        public DataTableJson() { }

        public DataTableJson(List<object> data, int total, int pageIndex = 1, int pageSize = 10)
            : base(data, total, pageIndex, pageSize)
        {
        }

        public DataTableJson(IEnumerable<object> data, int drawOption, int total)
            : base(new List<object>(data), drawOption, total)
        {
        }
    }
}
