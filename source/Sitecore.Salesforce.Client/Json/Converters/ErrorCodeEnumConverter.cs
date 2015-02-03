namespace Sitecore.Salesforce.Client.Json.Converters
{
  using System;

  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;

  using Sitecore.Salesforce.Client.Data.Errors;

  public class ErrorCodeEnumConverter : StringEnumConverter
  {
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.Value == null)
      {
        return ErrorCode.Unknown;
      }

      ErrorCode value;
      if (!Enum.TryParse(reader.Value.ToString(), true, out value))
      {
        return ErrorCode.Unknown;
      }
      return value;
    }
  }
}