// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceProviderTest.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceProviderTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tests
{
  using NUnit.Framework;

  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.FakeDb;
  using Sitecore.Integration.Common.Providers;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;

  [TestFixture]
  public class SalesforceProviderTest
  {
    private Db database;

    private ISalesforceClient Client { get; set; }

    [TestFixtureSetUp]
    public void SetUpTest()
    {
      ProviderHelper<SalesforceProvider, ProviderCollection<SalesforceProvider>>.DefaultProvider = new SalesforceProvider();

      var configuration = SalesforceManager.GetConfiguration("salesforce");

      this.Client = configuration.Client;
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {
    }

    [Test]
    public void GetSalesforceLimitsTest()
    {
      SLimits limits = SalesforceManager.GetSalesforceLimits(this.Client);

      Assert.IsNotNull(limits);
      Assert.IsNotNull(limits.DailyApiRequests);
      Assert.IsNotNull(limits.DataStorageMb);
      Assert.Pass(string.Format("LIMITS -- Daily Api Requests: {0}/{1} | Data Storage: {2}/{3} Mb", limits.DailyApiRequests.Remaining, limits.DailyApiRequests.Max, limits.DataStorageMb.Remaining, limits.DataStorageMb.Max));
    }
  }
}