// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESQuery : AbstractESQuery, ISelfSerializingQuery, IESQuery
    {
        private readonly string query;
        public static readonly AbstractESQuery All = new ESQuery("*:*");

        public ESQuery(string query) => this.query = query;

        public string Query => query;
    }
}
