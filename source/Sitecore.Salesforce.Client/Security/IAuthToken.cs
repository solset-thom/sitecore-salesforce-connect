namespace Sitecore.Salesforce.Client.Security
{
  public interface IAuthToken
  {
    string Id { get; set; }

    string IssuedAt { get; set; }

    string InstanceUrl { get; set; }

    string Signature { get; set; }

    string AccessToken { get; set; }

    string TokenType { get; set; }

    string RefreshToken { get; set; }
  }
}