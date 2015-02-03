namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Api;

  public class SalesforceApiConfiguration : SalesforceConfigurationEntry, ISalesforceApiConfiguration
  {
    public IContactsApi ContactsApi { get; set; }

    public IRolesApi RolesApi { get; set; }
  }
}