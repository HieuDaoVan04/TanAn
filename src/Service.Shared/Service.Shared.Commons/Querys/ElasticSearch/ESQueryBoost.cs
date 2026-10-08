// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESQueryBoost : AbstractESQuery
    {
        private readonly IESQuery query;
        private readonly double factor;

        public ESQueryBoost(IESQuery query, double factor)
        {
            this.query = query;
            this.factor = factor;
        }

        public double Factor => factor;

        public IESQuery Query => query;
    }
}
