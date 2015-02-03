
namespace Sitecore.Salesforce.Tests.Profile
{
  using System.Collections.Specialized;

  using NUnit.Framework;
  using Salesforce.Profile;
  using System;
  using System.Collections.Generic;

  using Sitecore.Salesforce.Api;
  using Sitecore.Salesforce.Client;

  [TestFixture]
  public class SalesforceProfileProviderTests
  {
    private SalesforceProfileProvider profileProvider;

    private ISalesforceClient Client { get; set; }

    private IContactsApi ContactsApi { get; set; }

    [TestFixtureSetUp]
    public void SetUpTest()
    {
      this.profileProvider = new SalesforceProfileProvider();
      this.profileProvider.Initialize("salesforce", new NameValueCollection() { { "readOnly", "false" }, { "applicationName", "sitecore" } });

      var configuration = SalesforceManager.GetConfiguration(this.profileProvider.Name);

      this.Client = configuration.Client;
      this.ContactsApi = configuration.Api.ContactsApi;
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {
    }

    [Test]
    [Category("Integration")]
    public bool UpdateProperties()
    {
      bool resultValue = false;
      string loginName = Environment.UserName + "_UpdatePropertiesTest@Site.com";

      var contact = this.ContactsApi.Get(loginName);

      if (contact == null)
      {
        var dictionary = new Dictionary<string, object>
          {
            { "LastName", loginName.Split('@')[0] },
            { "Email", loginName },
            { "Description", "Contact to update properties." }
          };

        var result = this.ContactsApi.Create(dictionary);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
      }

      string[] titles = { "Dev", "PO", "QA", "Admin" };
      var rnd = new Random();
      int rndValue = rnd.Next(titles.Length);

      resultValue = this.profileProvider.UpdateProperties(loginName, new Dictionary<string, object> { { "Title", titles[rndValue] } });

      this.ContactsApi.Delete(loginName);
      return resultValue;
    }
  }
}
