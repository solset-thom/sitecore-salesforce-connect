namespace Sitecore.Salesforce.Profile
{
  using System.Configuration;

  public class SalesforceProfileProperty : SettingsProperty
  {
    public string SalesforceName { get; private set; }

    public SalesforceProfileProperty(SettingsProperty property, string salesforceName)
      : base(property)
    {
      this.SalesforceName = salesforceName;
    }
  }
}
