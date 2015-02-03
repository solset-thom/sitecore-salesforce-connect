
namespace Sitecore.Salesforce.Soql
{
  using System.Web;
  using Sitecore.Diagnostics;
  using Utils;

  public static class ComparisonOperatorParser
  {

    public static string Parse(ComparisonOperator comparisonOperator, string param)
    {
      switch (comparisonOperator)
      {
        case ComparisonOperator.Contains:
          return string.Format("Like '%{0}%'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.Equals:
          return string.Format("='{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.GreaterOrEqual:
          return string.Format(">='{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.GreaterThan:
          return string.Format(">'{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.In:
          return string.Format(" IN ({0})", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.LessOrEqual:
          return string.Format("<='{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.LessThan:
          return string.Format("<'{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.NotEquals:
          return string.Format("!='{0}'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.NotIn:
          return string.Format("NOT IN ({0})", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        case ComparisonOperator.StartsWith:
          return string.Format("Like '{0}%'", HttpUtility.UrlEncode(SoqlUtil.Escape(param)));
        default:
          return "";
      }
    }

    public static string Parse(ComparisonOperator comparisonOperator, string param1, string param2)
    {
      Assert.ArgumentNotNull(param1, "param1");
      Assert.ArgumentNotNull(param2, "param2");
      switch (comparisonOperator)
      {
        case ComparisonOperator.DoesNotContain:
          return string.Format("Not {0} Like '%{1}%'", HttpUtility.UrlEncode(SoqlUtil.Escape(param1)), HttpUtility.UrlEncode(SoqlUtil.Escape(param2)));
        default:
          return "";
      }
    }
  }
}
