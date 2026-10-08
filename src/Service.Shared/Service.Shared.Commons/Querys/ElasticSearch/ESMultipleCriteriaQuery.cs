// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESMultipleCriteriaQuery : AbstractESQuery
    {
        private readonly IEnumerable<IESQuery> queries;
        private readonly string oper;

        public IEnumerable<IESQuery> Queries => queries;

        public string Oper => oper;

        public ESMultipleCriteriaQuery(IEnumerable<IESQuery> queries)
          : this(queries, "")
        {
        }

        public ESMultipleCriteriaQuery(IEnumerable<IESQuery> queries, string oper)
        {
            this.queries = queries;
            this.oper = oper;
        }

        public static ESMultipleCriteriaQuery Create(params IESQuery[] queries) => Create<IESQuery>(queries);

        public static ESMultipleCriteriaQuery Create<T>(IEnumerable<T> queries) where T : IESQuery => new ESMultipleCriteriaQuery(queries.Cast<IESQuery>());

        public class Operator
        {
            public const string OR = "OR";
            public const string AND = "AND";
        }
    }
}
