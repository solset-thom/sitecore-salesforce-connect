namespace Sitecore.Salesforce.Tests.Security
{
  using NUnit.Framework;

  using Sitecore.Configuration;
  using Sitecore.Salesforce.Client.Exceptions;
  using Sitecore.Salesforce.Client.Security;

  [TestFixture]
  [Category("Integration")]
  public class PasswordAuthenticatorTest
  {
    [Test]
    public void ShouldReturnsAuthTokenByCorrectAuthParameters()
    {
      var node = Factory.GetConfigNode("salesforce/clients/client/param[@desc='authenticator'][1]", true);
      var authenticator = Factory.CreateObject(node, false) as IAuthenticator;

      IAuthToken authToken = authenticator.Authenticate();

      Assert.NotNull(authToken);
      Assert.IsNotNullOrEmpty(authToken.TokenType);
      Assert.IsNotNullOrEmpty(authToken.AccessToken);
    }

    [Test]
    [ExpectedException(typeof(SalesforceAuthException))]
    public void ShouldThrowAuthExceptionForEmptyAuthParameters()
    {
      new PasswordAuthenticator().Authenticate();
    }
  }
}