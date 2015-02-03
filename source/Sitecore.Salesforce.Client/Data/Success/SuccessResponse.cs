
namespace Sitecore.Salesforce.Client.Data.Success
{
  using Newtonsoft.Json;

  public class SuccessResponse
  {
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("Success")]
    public string Success;

    [JsonProperty("errors")]
    public object Errors;
  }
}
