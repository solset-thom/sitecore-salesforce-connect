// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceManager.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce
{
  using System.Collections.Generic;

  using Sitecore.Configuration;
  using Sitecore.Integration.Common.Providers;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;

  public static class SalesforceManager
  {
    private static readonly ProviderHelper<SalesforceProvider, ProviderCollection<SalesforceProvider>> ProviderHelper;

    static SalesforceManager()        
    {
      ProviderHelper = new ProviderHelper<SalesforceProvider, ProviderCollection<SalesforceProvider>>("salesforce/salesforceManager");
    }

    public static SalesforceProvider Provider
    {
      get
      {
        return ProviderHelper.Provider;
      }
    }

    public static ProviderCollection<SalesforceProvider> Providers
    {
      get
      {
        return ProviderHelper.Providers;
      }
    }

    public static ISalesforceConfiguration GetConfiguration(string providerName)
    {
      return Provider.GetConfiguration(providerName);
    }

    public static List<ISalesforceConfiguration> GetConfigurations()
    {
      return Provider.GetConfigurations();
    }

    public static SLimits GetSalesforceLimits(ISalesforceClient client)
    {
      return Provider.GetSalesforceLimits(client);
    }
  }
}