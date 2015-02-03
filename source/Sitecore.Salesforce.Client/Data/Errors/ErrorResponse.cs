namespace Sitecore.Salesforce.Client.Data.Errors
{
  using Newtonsoft.Json;

  using Sitecore.Salesforce.Client.Json.Converters;

  public class ErrorResponse : IErrorResponse
  {
    [JsonProperty("errorCode")]
    [JsonConverter(typeof(ErrorCodeEnumConverter))]
    public ErrorCode ErrorCode { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    public ErrorResponse()
    {
      this.Message = string.Empty;
    }
  }
}
