
namespace Sitecore.Salesforce.Profile
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Configuration;
  using System.Linq;
  using System.Web.Profile;
  using Api;

  using Data;

  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Linq;

  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Diagnostics;
  using Sitecore.Salesforce.Soql;

  public class SalesforceProfileProvider : ProfileProvider
  {
    protected bool Initialized;
    
    public bool ReadOnly { get; private set; }

    public override string ApplicationName { get; set; }

    protected IContactsApi ContactsApi { get; set; }

    protected ISalesforceFieldMapping FieldMapping { get; set; }

    protected ISalesforceProfileConfiguration ProfileConfiguration { get; set; }

    protected ISalesforceCacheConfiguration Cache { get; set; }

    protected string[] SaleforcePropertyNames { get; set; }

    public override void Initialize(string name, NameValueCollection config)
    {
      base.Initialize(name, config);
      try
      {
        if (SalesforceSettings.Disabled)
        {
          LogHelper.Info("Salesforce connector is disabled.", this);
          return;
        }

        if (MainUtil.GetBool(config["disabled"], false))
        {
          LogHelper.Info(string.Format("Provider is disabled. Provider name: {0}", this.Name), this);
          return;
        }

        var configuration = SalesforceManager.GetConfiguration(name);
        if (configuration == null)
        {
          LogHelper.Error("Initialization failed. Configuration is null.", this);
          return;
        }

        this.ContactsApi = configuration.Api.ContactsApi;
        this.FieldMapping = configuration.FieldMapping;
        this.ProfileConfiguration = configuration.Profile;
        this.Cache = configuration.Cache;

        this.SaleforcePropertyNames = configuration.Profile.Properties.Select(i => i.SalesforceName).ToArray();

        this.ApplicationName = config["applicationName"];

        this.ReadOnly = MainUtil.GetBool(config["readOnly"], false);

        this.Initialized = true;
      }
      catch(Exception ex)
      {
        this.Initialized = false;

        LogHelper.Error(string.Format("Provider initialization error. Provider name: {0}", this.Name), this, ex);
      }
    }

    public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
    {
      var result = new SettingsPropertyValueCollection();

      if (!this.Initialized)
      {
        return result;
      }

      string userName = (string)context["UserName"];

      if (string.IsNullOrEmpty(userName) || userName.Contains(@"\"))
      {
        return result;
      }

      var relevantProperties = this.GetRelevantProperties(collection);
      if (relevantProperties.Count == 0)
      {
        return result;
      }

      var userProperties = this.GetProperties(userName, this.SaleforcePropertyNames);

      foreach (SalesforceProfileProperty property in relevantProperties)
      {
        object value;
        if (userProperties.TryGetValue(property.SalesforceName, out value))
        {
          value = this.ConvertProperty(value, property.PropertyType);
        }

        result.Add(new SettingsPropertyValue(property)
          {
            IsDirty = false,
            PropertyValue = value,
          });

        collection.Remove(property.Name);
      }

      return result;
    }

    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
    {
      if (!this.Initialized)
      {
        return;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      string userName = (string)context["UserName"];
      if (string.IsNullOrEmpty(userName) || userName.Contains(@"\"))
      {
        return;
      }

      var properties = this.PreparePropertiesToUpdate(collection);

      if (properties.Count > 0)
      {
        if (!this.UpdateProperties(userName, properties))
        {
          LogHelper.Warn(string.Format("Couldn't save profile changes for Salesforce user. User name: {0}", userName), this);
        }
        else
        {
          LogHelper.Debug(string.Format("Profile changes have been set for Salesforce user. User name: {0}", userName));
        }
      }
    }

    public override int DeleteProfiles(ProfileInfoCollection profiles)
    {
      if (!this.Initialized || this.ReadOnly)
      {
        return 0;
      }
      string[] usernames = (from profile in profiles.Cast<ProfileInfo>() select profile.UserName).ToArray<string>();

      return this.DeleteProfiles(usernames);
    }

    public override int DeleteProfiles(string[] usernames)
    {
      if (!this.Initialized || this.ReadOnly)
      {
        return 0;
      }

      if (usernames.Length == 0)
      {
        return 0;
      }

      int num = 0;

      Dictionary<string, object> properties = new Dictionary<string, object>();
      foreach (string saleforcePropertyName in this.SaleforcePropertyNames)
      {
        properties[saleforcePropertyName] = null;
      }

      foreach (var username in usernames)
      {
        if (this.UpdateProperties(username, properties))
        {
          LogHelper.Debug(string.Format("Profile has been deleted for Salesforce user. User name: {0}", username));
          num++;
        }
      }

      return num;
    }

    public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
    {
      if (!this.Initialized || this.ReadOnly)
      {
        return 0;
      }

      List<ISalesforceContact> contacts = this.ContactsApi.GetAll(0, int.MaxValue);

      List<string> userNames = new List<string>();
      foreach (var contact in contacts)
      {
        if (contact.LastActivityDate <= userInactiveSinceDate)
        {
          userNames.Add(contact.Login);
        }
      }

      return this.DeleteProfiles(userNames.ToArray());
    }

    public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
    {
      if (!this.Initialized)
      {
        return  0;
      }

      return this.ContactsApi.GetTotalCountByFieldValue(this.FieldMapping.LastActivityDate, userInactiveSinceDate, ComparisonOperator.LessOrEqual);
    }

    public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize,
      out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new ProfileInfoCollection();
      }

      totalRecords = this.ContactsApi.GetTotalCount();
      var contacts = this.ContactsApi.GetAll(pageIndex, pageSize);

      return this.GetProfileInfoCollection(contacts);
    }

    public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
      DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new ProfileInfoCollection();
      }

      totalRecords = this.ContactsApi.GetTotalCountByFieldValue(this.FieldMapping.LastActivityDate, userInactiveSinceDate, ComparisonOperator.LessOrEqual);
      var contacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.LastActivityDate, userInactiveSinceDate, ComparisonOperator.LessOrEqual, pageIndex, pageSize);

      return this.GetProfileInfoCollection(contacts);
    }

    public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch,
      int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new ProfileInfoCollection();
      }

      totalRecords = this.ContactsApi.GetTotalCountByFieldValue(this.FieldMapping.Login, usernameToMatch, ComparisonOperator.Contains);
      var contacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.Login, usernameToMatch, ComparisonOperator.Contains, pageIndex, pageSize);

      return this.GetProfileInfoCollection(contacts);
    }

    public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption,
      string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new ProfileInfoCollection();
      }

      totalRecords = this.ContactsApi.GetTotalInactiveByLoginName(usernameToMatch, ComparisonOperator.Contains, userInactiveSinceDate);
      var contacts = this.ContactsApi.FindInactiveByLoginName(usernameToMatch, ComparisonOperator.Contains, userInactiveSinceDate, pageIndex, pageSize);

      return this.GetProfileInfoCollection(contacts);
    }

    protected virtual Dictionary<string, object> PreparePropertiesToUpdate(SettingsPropertyValueCollection collection)
    {
      var properties = new Dictionary<string, object>();

      var relevantPropertyValues = this.GetRelevantPropertyValues(collection);
      if (relevantPropertyValues.Count == 0)
      {
        return properties;
      }

      foreach (SalesforceProfilePropertyValue property in relevantPropertyValues)
      {
        if (property.IsDirty && property.PropertyValue != null)
        {
          object value;
          if (this.ProfileConfiguration.OutboundConverter.TryConvert(property.PropertyValue, typeof(string), out value))
          {
            property.PropertyValue = value;
          }

          properties[property.SalesforceProperty.SalesforceName] = property.PropertyValue;
        }
      }

      return properties;
    }

    public virtual bool UpdateProperties(string loginName, Dictionary<string, object> properties)
    {
      Assert.ArgumentNotNullOrEmpty(loginName, "loginName");

      var user = this.ContactsApi.Get(loginName);
      if (user != null)
      {
        foreach (var pair in properties)
        {
          user.SetProperty(pair.Key, pair.Value);
        }

        if (this.ContactsApi.Update(user))
        {
          this.Cache.Users.RemoveByName(loginName);
          return true;
        }
      }

      return false;
    } 

    protected virtual SettingsPropertyCollection GetRelevantProperties(SettingsPropertyCollection properties)
    {
      var result = new SettingsPropertyCollection();

      if (this.ProfileConfiguration.Properties.Count == 0)
      {
        return result;
      }

      foreach (SettingsProperty property in properties)
      {
        var entry = this.ProfileConfiguration.Properties.FirstOrDefault(i => i.Name == property.Name);
        if (entry != null)
        {
          result.Add(new SalesforceProfileProperty(property,entry.SalesforceName));
        }
      }

      return result;
    }

    protected virtual SettingsPropertyValueCollection GetRelevantPropertyValues(SettingsPropertyValueCollection properties)
    {
      var result = new SettingsPropertyValueCollection();

      if (this.ProfileConfiguration.Properties.Count == 0)
      {
        return result;
      }

      foreach (SettingsPropertyValue property in properties)
      {
        var entry = this.ProfileConfiguration.Properties.FirstOrDefault(i => i.Name == property.Name);
        if (entry != null)
        {
          var salesforceProperty = new SalesforceProfileProperty(property.Property, entry.SalesforceName);

          result.Add(new SalesforceProfilePropertyValue(property, salesforceProperty));
        }
      }
      return result;
    }

    protected virtual Dictionary<string, object> GetProperties(string loginName, string[] propertyNames)
    {
      Assert.ArgumentNotNullOrEmpty(loginName, "loginName");

      var salesforceUser = this.Cache.Users.GetByName(loginName) ?? this.ContactsApi.Get(loginName);

      return this.GetProperties(salesforceUser, propertyNames);
    }

    protected virtual Dictionary<string, object> GetProperties(ISalesforceContact contact, string[] propertyNames)
    {
      if (contact == null)
      {
        return new Dictionary<string, object>(0);
      }

      var dictionary = new Dictionary<string, object>(propertyNames.Length);

      foreach (var propertyName in propertyNames)
      {
        object value = contact.GetProperty<object>(propertyName);

        if (value != null)
        {
          dictionary[propertyName] = value;
        }
      }

      return dictionary;
    }

    protected virtual ProfileInfoCollection GetProfileInfoCollection(IEnumerable<ISalesforceContact> contacts)
    {
      Assert.ArgumentNotNull(contacts, "contacts");

      var result = new ProfileInfoCollection();

      foreach (var contact in contacts)
      {
        this.Cache.Users.Add(contact);

        result.Add(new ProfileInfo(contact.Login, false, DateTime.Now, DateTime.Now, 0));
      }

      return result;
    }

    protected virtual object ConvertProperty(object value, Type targetType)
    {
      if (value == null || value.GetType() == targetType)
      {
        return value;
      }

      JToken token = value as JToken;
      if (token != null)
      {
        try
        {
          return token.ToObject(targetType);
        }
        catch (Exception ex)
        {
          LogHelper.Warn(string.Format("Could not convert profile property. Source Type:{0}; Destination Type:{1}", value.GetType(), targetType), this, ex);
          return null;
        }
      }

      object tmpValue;
      if(this.ProfileConfiguration.InboundConverter.TryConvert(value, targetType, out tmpValue))
      {
        return tmpValue;
      }

      var stringValue = value as string;
      if (stringValue != null)
      {
        try
        {
          return JsonConvert.DeserializeObject(stringValue, targetType);
        }
        catch (Exception ex)
        {
          LogHelper.Warn(string.Format("Could not convert profile property. Source Type:{0}; Destination Type:{1}", value.GetType(), targetType), this, ex);
          return null;
        }
      }

      return null;
    }
  }
}