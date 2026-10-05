// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

namespace Service.Shared.Commons.Querys.ElasticSearch
{
    public class ESQueryByField : AbstractESQuery
    {
        private readonly string fieldName;
        private readonly string fieldValue;

        public ESQueryByField(string fieldName, string fieldValue)
        {
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
            Quoted = true;
        }

        public bool Quoted { get; set; }

        public string FieldName => fieldName;

        public string FieldValue => fieldValue;
    }
}
