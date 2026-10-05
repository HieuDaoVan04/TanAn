// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public abstract class AbstractESQuery : IESQuery
    {
        public AbstractESQuery Not() => new ESNotQuery(this);

        public AbstractESQuery Required() => new ESRequiredQuery(this);

        public AbstractESQuery Boost(double factor) => new ESQueryBoost(this, factor);


        public static AbstractESQuery operator &(AbstractESQuery a, AbstractESQuery b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            return b != null ? (AbstractESQuery) new ESMultipleCriteriaQuery(new AbstractESQuery[2]
            {
        a,
        b
            }, "AND") : throw new ArgumentNullException(nameof(b));
        }

        public static AbstractESQuery operator |(AbstractESQuery a, AbstractESQuery b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            return b != null ? (AbstractESQuery) new ESMultipleCriteriaQuery(new AbstractESQuery[2]
            {
        a,
        b
            }, "OR") : throw new ArgumentNullException(nameof(b));
        }

        public static AbstractESQuery operator +(AbstractESQuery a, AbstractESQuery b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            return b != null ? (AbstractESQuery) new ESMultipleCriteriaQuery(new AbstractESQuery[2]
            {
        a,
        b
            }) : throw new ArgumentNullException(nameof(b));
        }

        public static AbstractESQuery operator -(AbstractESQuery a, AbstractESQuery b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            return b != null ? (AbstractESQuery) new ESMultipleCriteriaQuery(new AbstractESQuery[2]
            {
        a,
        b.Not()
            }) : throw new ArgumentNullException(nameof(b));
        }

        public static bool operator false(AbstractESQuery a) => false;

        public static bool operator true(AbstractESQuery a) => false;

        public static AbstractESQuery operator !(AbstractESQuery a) => a != null ? a.Not() : throw new ArgumentNullException(nameof(a));
    }
}
