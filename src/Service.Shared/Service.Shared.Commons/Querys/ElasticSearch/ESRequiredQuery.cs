// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESRequiredQuery : AbstractESQuery
    {
        private readonly IESQuery query;

        public ESRequiredQuery(IESQuery q) => query = q;

        public IESQuery Query => query;
    }
}
