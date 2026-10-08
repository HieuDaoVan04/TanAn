// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.Grid
{
    public class GridRequest
    {
#pragma warning disable IDE1006 

        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public Filter filter { get; set; }

        public List<Sort> sort { get; set; }
#pragma warning restore IDE1006

        public GridRequest()
        {
            sort = new List<Sort>() {  };
            filter = new Filter();

        }

    }
}
