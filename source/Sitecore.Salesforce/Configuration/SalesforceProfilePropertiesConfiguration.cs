namespace Sitecore.Salesforce.Configuration
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Web.Configuration;

  using Sitecore.Salesforce.Data.Profile;

  public class SalesforceProfilePropertiesConfiguration : IReadOnlyCollection<ISalesforceProfileProperty>
  {
    protected readonly List<ISalesforceProfileProperty> Properties;

    public SalesforceProfilePropertiesConfiguration(string providerName)
    {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
      this.Properties = this.GetProperties(providerName);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
    }

    public IEnumerator<ISalesforceProfileProperty> GetEnumerator()
    {
      return this.Properties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.Properties.GetEnumerator();
    }

    public int Count
    {
      get
      {
        return this.Properties.Count;
      }
    }

    protected virtual List<ISalesforceProfileProperty> GetProperties(string providerName)
    {
      var properties = new List<ISalesforceProfileProperty>(); 

      var profileSection = (ProfileSection)WebConfigurationManager.OpenWebConfiguration("/aspnet").GetSection("system.web/profile");
      foreach (ProfilePropertySettings property in profileSection.PropertySettings)
      {
        var salesforceProperty = this.GetProviderProperty(property, providerName);
        if (salesforceProperty != null)
        {
          properties.Add(salesforceProperty);
        }
      }

      return properties;
    }

    protected virtual SalesforceProfileProperty GetProviderProperty(ProfilePropertySettings property, string providerName)
    {
      if (string.IsNullOrEmpty(property.CustomProviderData))
      {
        return null;
      }

      string[] data = property.CustomProviderData.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string providerStr in data)
      {
        string[] provideData = providerStr.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

        if (provideData.Length == 2 && provideData[0] == providerName && !string.IsNullOrEmpty(provideData[1]))
        {
          return new SalesforceProfileProperty
            {
              Name = property.Name, 
              SalesforceName = provideData[1]
            };
        }
      }

      return null;
    }
  }
}