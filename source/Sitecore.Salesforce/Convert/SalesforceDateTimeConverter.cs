namespace Sitecore.Salesforce.Convert
{
  using System;

  using Sitecore.Integration.Common.Convert;

  public class SalesforceDateTimeConverter : 
    IConverter<string, DateTime>, IConverter<DateTime,string>,
    IConverter<string, DateTimeOffset>, IConverter<DateTimeOffset,string>
  {
    private static readonly DateTime SalesforceMinDate = new DateTime(1700, 1, 1);

    private readonly string dateTimeFormat;

    public SalesforceDateTimeConverter()
      : this("yyyy-MM-ddTHH:mm:ssZ")
    {
    }

    public SalesforceDateTimeConverter(string dateTimeFormat)
    {
      this.dateTimeFormat = dateTimeFormat;
    }

    DateTime IConverter<string, DateTime>.Convert(string source)
    {
      DateTime value;
      DateTime.TryParse(source, out value);

      return value >= SalesforceMinDate ? value : SalesforceMinDate;
    }

    string IConverter<DateTime,string>.Convert(DateTime source)
    {
      if (source < SalesforceMinDate)
      {
        source = SalesforceMinDate;
      }

      return source.ToString(this.dateTimeFormat);
    }

    DateTimeOffset IConverter<string, DateTimeOffset>.Convert(string source)
    {
      DateTimeOffset value;
      DateTimeOffset.TryParse(source, out value);

      return value >= SalesforceMinDate ? value : SalesforceMinDate;
    }

    string IConverter<DateTimeOffset,string>.Convert(DateTimeOffset source)
    {
      if (source < SalesforceMinDate)
      {
        source = SalesforceMinDate;
      }

      return source.ToString(this.dateTimeFormat);
    }
  }
}
