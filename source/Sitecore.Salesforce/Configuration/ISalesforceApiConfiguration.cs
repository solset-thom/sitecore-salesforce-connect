namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Api;

  public interface ISalesforceApiConfiguration : ISalesforceConfigurationEntry
  {
    IContactsApi ContactsApi { get; } 

    IRolesApi RolesApi { get; } 
  }
}