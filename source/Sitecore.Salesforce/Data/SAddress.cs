
namespace Sitecore.Salesforce.Data
{
  using Newtonsoft.Json;

  public class SAddress
  {
    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("countryCode")]
    public string CountryCode { get; set; }

    [JsonProperty("latitude")]
    public string Latitude { get; set; }

    [JsonProperty("longitude")]
    public string Longitude { get; set; }

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("stateCode")]
    public string StateCode { get; set; }

    [JsonProperty("street")]
    public string Street { get; set; }
  }
}
