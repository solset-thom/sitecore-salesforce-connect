namespace Sitecore.Salesforce.Configuration
{
  using System.Reflection;

  using Sitecore.Salesforce.Diagnostics;

  public class SalesforceConfigurationEntry : ISalesforceConfigurationEntry
  {
    public virtual bool IsValid()
    {
      //TODO: compile expression
      var type = this.GetType();

      foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!propertyInfo.CanRead)
        {
          continue;
        }

        object propertyObj = propertyInfo.GetGetMethod().Invoke(this, new object[0]);
        if (propertyObj == null)
        {
          LogHelper.Error(propertyInfo.Name + " is null",this);

          return false;
        }

        var entry = propertyObj as ISalesforceConfigurationEntry;
        if (entry != null && !entry.IsValid())
        {
          return false;
        }
      }

      return true;
    }
  }
}