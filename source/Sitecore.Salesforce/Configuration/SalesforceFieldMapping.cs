namespace Sitecore.Salesforce.Configuration
{
  using System.Collections.Generic;
  using System.Reflection;

  public class SalesforceFieldMapping : SalesforceConfigurationEntry, ISalesforceFieldMapping
  {
    public string Id { get; set; }

    public string Login { get; set; }

    public string Email { get; set; }

    public string Description { get; set; }

    public string Password { get; set; }

    public string PasswordAnswer { get; set; }

    public string PasswordQuestion { get; set; }

    public string IsApproved { get; set; }

    public string IsLockedOut { get; set; }

    public string CreatedDate { get; set; }
    
    public string LastLoginDate { get; set; }
    
    public string LastActivityDate { get; set; }
    
    public string LastPasswordChangedDate { get; set; }

    public virtual List<string> GetAllFields()
    {
      //TODO: compile expression
      var type = this.GetType();
      var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

      var fields = new List<string>(properties.Length);

      foreach (var property in properties)
      {
        if (!property.CanRead)
        {
          continue;
        }

        string field = property.GetGetMethod().Invoke(this, new object[0]) as string;
        if (!string.IsNullOrEmpty(field))
        {
          fields.Add(field);
        }
      }

      return fields;
    }
  }
}