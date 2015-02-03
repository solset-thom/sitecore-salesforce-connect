namespace Sitecore.Salesforce.Client.Data.Errors
{
  using Newtonsoft.Json;

  using Sitecore.Salesforce.Client.Json.Converters;

  public class AuthErrorResponse : IErrorResponse
  {
    [JsonProperty("error")]
    [JsonConverter(typeof(ErrorCodeEnumConverter))]
    public ErrorCode ErrorCode { get; set; }

    [JsonProperty("error_description")]
    public string Message { get; set; }

    public AuthErrorResponse()
    {
      this.Message = string.Empty;
    }
  }
}