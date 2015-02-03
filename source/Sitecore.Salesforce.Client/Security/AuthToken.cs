namespace Sitecore.Salesforce.Client.Security
{
  using System;

  using Newtonsoft.Json;

  [Serializable]
  public class AuthToken : IAuthToken
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("issued_at")]
    public string IssuedAt { get; set; }

    [JsonProperty("instance_url")]
    public string InstanceUrl { get; set; }

    [JsonProperty("signature")]
    public string Signature { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
  }
}