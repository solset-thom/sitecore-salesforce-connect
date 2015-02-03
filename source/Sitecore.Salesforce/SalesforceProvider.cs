// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceProvider.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Configuration.Provider;
  using System.Linq;
  using System.Xml;

  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Diagnostics;
  using Sitecore.Xml;

  public class SalesforceProvider : ProviderBase
  {
    protected readonly object ConfigNodeLock = new object();

    protected readonly ConcurrentDictionary<string, ISalesforceConfiguration> Configurations = new ConcurrentDictionary<string, ISalesforceConfiguration>();

    public virtual ISalesforceConfiguration GetConfiguration(string providerName)
    {
      Assert.IsNotNullOrEmpty(providerName, "providerName");

      ISalesforceConfiguration result;

      if (this.Configurations.TryGetValue(providerName, out result))
      {
        return result;
      }

      XmlNode node = this.GetConfigurationNode(providerName);
      if (node == null)
      {
        LogHelper.Error("Could not find configuration node. Provider name: " + providerName, this);

        this.Configurations.TryAdd(providerName, null);
      }

      try
      {
        var configuration = Factory.CreateObject(node, false) as ISalesforceConfiguration;
        if (configuration != null && configuration.IsValid())
        {
          this.Configurations.TryAdd(providerName, configuration);
        }
        else
        {
          this.Configurations.TryAdd(providerName, null);

          LogHelper.Error("Invalid configuration. Provider name:" + providerName, this);
        }
      }
      catch (Exception ex)
      {
        this.Configurations.TryAdd(providerName, null);

        LogHelper.Error("Could not create configuration. Provider name:" + providerName, this, ex);
      }

      //ensure concurrency behaviour
      this.Configurations.TryGetValue(providerName, out result);

      return result;
    }

    public List<ISalesforceConfiguration> GetConfigurations()
    {
      return this.Configurations.Values.Where(i => i != null).ToList();
    }

    public virtual SLimits GetSalesforceLimits(ISalesforceClient client)
    {
      return client.HttpGet<SLimits>("limits");
    }

    protected virtual XmlNode GetConfigurationNode(string providerName)
    {
      Assert.ArgumentNotNullOrEmpty(providerName, "providerName");

      lock (ConfigNodeLock)
      {
        XmlNode node = Factory.GetConfigNode(string.Format("salesforce/configurations/*[@provider='{0}']", providerName), false);
        if (node != null)
        {
          return node;
        }

        var defaultConfiguration = Factory.GetConfigNode("salesforce/configurations/*[@default='true']", true);

        var providerNode = defaultConfiguration.Clone();

        XmlUtil.SetAttribute("provider", providerName, providerNode);
        XmlUtil.RemoveAttribute("default", providerNode);

        if (defaultConfiguration.ParentNode != null)
        {
          defaultConfiguration.ParentNode.AppendChild(providerNode);

          return providerNode;
        }

        LogHelper.Error(string.Format("Could not find configuration root node. Provider name: {0}", providerName), this);

        return null; 
      }
    }
  }
}