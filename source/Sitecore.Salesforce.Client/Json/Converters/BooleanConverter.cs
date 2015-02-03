namespace Sitecore.Salesforce.Client.Json.Converters
{
  using System;

  using Newtonsoft.Json;

  public class BooleanConverter : JsonConverter
  {
    protected readonly string TrueValue;
    protected readonly string FalseValue;

    public BooleanConverter(string trueValue, string falseValue)
    {
      this.TrueValue = trueValue;
      this.FalseValue = falseValue;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteValue(((bool)value) ? this.TrueValue : this.FalseValue);
      }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.Value != null)
      {
        return reader.Value.ToString().ToUpperInvariant() == this.TrueValue.ToUpperInvariant();
      }

      return false;
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(bool);
    }
  }
}
