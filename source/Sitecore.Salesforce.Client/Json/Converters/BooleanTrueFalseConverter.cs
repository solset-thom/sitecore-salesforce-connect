namespace Sitecore.Salesforce.Client.Json.Converters
{
  public class BooleanTrueFalseConverter : BooleanConverter
  {
    public BooleanTrueFalseConverter() : base("true", "false")
    {
    }
  }
}