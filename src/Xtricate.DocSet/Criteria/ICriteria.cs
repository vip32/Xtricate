namespace Xtricate.DocSet
{
    public interface ICriteria
    {
        string Name { get; set; }
        CriteriaOperator Operator { get; set; }
        string Value { get; set; }
    }
}