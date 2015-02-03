namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Client;

  public interface ISalesforceConfiguration : ISalesforceConfigurationEntry
  {
    string Name { get; }

    ISalesforceClient Client { get; }

    ISalesforceApiConfiguration Api { get; }

    ISalesforceProfileConfiguration Profile { get; }

    ISalesforceCacheConfiguration Cache { get; }

    ISalesforceFieldMapping FieldMapping { get; }
  }
}