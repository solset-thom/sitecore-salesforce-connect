namespace Sitecore.Salesforce.Utils
{

  public static class SoqlUtil
  {
    private static string EscapedCharacters = "\\'\"";

    public static string Escape(string value)
    {
      return Escape(value, EscapedCharacters);
    }

    public static string Escape(string value, string escapedCharacters)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }
      string escapedString = string.Empty;
      foreach (var val in value)
      {
        if (escapedCharacters.IndexOf(val) >= 0)
        {
          escapedString = escapedString + "\\" + val;
        }
        else
        {
          escapedString = escapedString + val;
        }
        
      }

      return escapedString;
    }

  }
}
