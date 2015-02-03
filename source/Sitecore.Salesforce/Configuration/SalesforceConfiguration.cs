
namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Client;

  public class SalesforceConfiguration : SalesforceConfigurationEntry, ISalesforceConfiguration
  {
    public SalesforceConfiguration(string name)
    {
      this.Name = name;
    }

    public string Name { get;protected set; }

    public ISalesforceClient Client { get; set; }

    public ISalesforceApiConfiguration Api { get; set; }

    public ISalesforceProfileConfiguration Profile { get; set; }

    public ISalesforceCacheConfiguration Cache { get; set; }

    public ISalesforceFieldMapping FieldMapping { get; set; }
  }
}
