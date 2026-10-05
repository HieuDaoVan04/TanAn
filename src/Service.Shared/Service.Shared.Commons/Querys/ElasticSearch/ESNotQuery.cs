// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESNotQuery : AbstractESQuery
    {
        private readonly IESQuery query;

        public ESNotQuery(IESQuery q) => query = q;

        public IESQuery Query => query;
    }
}
