
namespace Sitecore.Salesforce.Client.Security
{
  using System.Collections.Generic;
  using System.Net.Http;

  public class PasswordAuthenticator : AuthenticatorBase
  {
    public string ConsumerKey { get; set; }

    public string ConsumerSecret { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string SecurityToken { get; set; }

    public PasswordAuthenticator() : this(new HttpClient())
    {
    }

    public PasswordAuthenticator(HttpClient httpClient)
      :base(httpClient)
    {
    }

    protected override Dictionary<string, string> GetAuthParameters()
    {
      return new Dictionary<string, string>(5)
        {
          { "grant_type", "password" },
          { "client_id", this.ConsumerKey },
          { "client_secret", this.ConsumerSecret },
          { "username", this.UserName },
          { "password", this.Password + this.SecurityToken }
        };
    }
  }
}