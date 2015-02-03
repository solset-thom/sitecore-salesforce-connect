
namespace Sitecore.Salesforce.Soql
{
  using System.ComponentModel;

  public enum ComparisonOperator
  {
    [Description("contains - Like '%{0}%'")]
    Contains,
    
    [Description("does not contain - Not {1} Like '%{0}%'")]
    DoesNotContain,
    
    [Description("equals - ='{0}'")]
    Equals,
    
    [Description("greater or equal to - >='{0}'")]
    GreaterOrEqual,
    
    [Description("greater than - >'{0}'")]
    GreaterThan,
    
    [Description("in - IN ({0})")]
    In,

    [Description("less or equal to - <='{0}'")]
    LessOrEqual,

    [Description("less than - <'{0}'")]
    LessThan,

    [Description("not equal to - !='{0}'")]
    NotEquals,

    [Description("not in -  NOT IN ({0})")]
    NotIn,

    [Description("starts with - Like '{0}%'")]
    StartsWith

  }
}
