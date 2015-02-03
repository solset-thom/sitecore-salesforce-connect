namespace Sitecore.Salesforce.Configuration
{
  using System.Collections.Generic;

  using Sitecore.Integration.Common.Convert;
  using Sitecore.Salesforce.Data.Profile;

  public class SalesforceProfileConfiguration : SalesforceConfigurationEntry, ISalesforceProfileConfiguration
  {
    public IConvertProvider InboundConverter { get; set; }

    public IConvertProvider OutboundConverter { get; set; }

    public IReadOnlyCollection<ISalesforceProfileProperty> Properties { get; set; }
  }
}