namespace XtricateSql
{
    public class Criteria : ICriteria
    {
        public Criteria(string name, CriteriaOperator @operator, string value)
        {
            Name = name;
            Operator = @operator;
            Value = value;
        }

        public string Name { get; set; }
        public CriteriaOperator Operator { get; set; }
        public string Value { get; set; }
    }
}