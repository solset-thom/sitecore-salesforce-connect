namespace Sitecore.Salesforce.Configuration
{
  using System.Collections.Generic;

  public interface ISalesforceFieldMapping : ISalesforceConfigurationEntry
  {
    string Id { get; }

    string Login { get; }

    string Email { get; }

    string Description { get; }

    string Password { get; }
    
    string PasswordAnswer { get; }
    
    string PasswordQuestion { get; }

    string IsApproved { get; }

    string IsLockedOut { get; }

    string CreatedDate { get; }

    string LastLoginDate { get;}

    string LastActivityDate { get;}

    string LastPasswordChangedDate { get; }

    List<string> GetAllFields();
  }
}