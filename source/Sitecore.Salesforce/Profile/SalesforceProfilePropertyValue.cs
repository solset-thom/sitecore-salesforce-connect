namespace Sitecore.Salesforce.Profile
{
  using System.Configuration;

  public class SalesforceProfilePropertyValue : SettingsPropertyValue
  {
    public SalesforceProfilePropertyValue(SalesforceProfileProperty property)
      : base(property)
    {
    }

    public SalesforceProfilePropertyValue(SettingsPropertyValue propertyValue, SalesforceProfileProperty property)
      : base(property)
    {
      base.PropertyValue = propertyValue.PropertyValue;
      base.Deserialized = propertyValue.Deserialized;
      base.IsDirty = propertyValue.IsDirty;
      base.SerializedValue = propertyValue.SerializedValue;
    }

    public SalesforceProfileProperty SalesforceProperty
    {
      get
      {
        return (SalesforceProfileProperty)this.Property;
      }
    }
  }
}