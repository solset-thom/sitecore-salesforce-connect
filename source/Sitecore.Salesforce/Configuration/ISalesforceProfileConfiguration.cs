namespace Sitecore.Salesforce.Configuration
{
  using System.Collections.Generic;

  using Sitecore.Integration.Common.Convert;
  using Sitecore.Salesforce.Data.Profile;

  public interface ISalesforceProfileConfiguration : ISalesforceConfigurationEntry
  {
    IConvertProvider InboundConverter { get; }

    IConvertProvider OutboundConverter { get; }

    IReadOnlyCollection<ISalesforceProfileProperty> Properties { get; }
  }
}