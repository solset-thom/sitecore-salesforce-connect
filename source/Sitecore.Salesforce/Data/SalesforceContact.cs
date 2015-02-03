// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceContact.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceContact type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Data
{
  using System;
  using System.Collections.Generic;
  using System.Web.Security;

  using Sitecore.Caching;
  using Sitecore.Diagnostics;
  using Sitecore.Reflection;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;

  public class SalesforceContact : SObject, ISalesforceContact
  {
    protected static readonly DateTime SalesforceMinDate = new DateTime(1700, 1, 1);

    protected readonly ISalesforceFieldMapping FieldMapping;

// ReSharper disable InconsistentNaming
    protected readonly Dictionary<string, object> properties = new Dictionary<string, object>();
// ReSharper restore InconsistentNaming


// ReSharper disable InconsistentNaming
    protected bool cacheable = true;
// ReSharper restore InconsistentNaming

    protected bool DisableDataLengthChanged;

    public SalesforceContact(ISalesforceFieldMapping fieldMapping)
    {
      Assert.ArgumentNotNull(fieldMapping, "fieldMapping");

      this.FieldMapping = fieldMapping;
    }


    public SalesforceContact(ISalesforceFieldMapping fieldMapping, IEnumerable<KeyValuePair<string, object>> properties) 
      : this(fieldMapping)
    {
      Assert.ArgumentNotNull(properties, "properties");

      foreach (var pair in properties)
      {
        this.properties[pair.Key] = pair.Value;
      }
    }

    public SalesforceContact(ISalesforceFieldMapping fieldMapping, MembershipUser user)
      : this(fieldMapping)
    {
      this.DisableDataLengthChanged = true;
      try
      {
        this.Id = user.ProviderUserKey as string;
        this.Login = user.UserName;
        this.Email = user.Email;
        this.Description = user.Comment;
        this.IsApproved = user.IsApproved;
        this.LastLoginDate = user.LastLoginDate.ToUniversalTime();
        this.LastActivityDate = user.LastActivityDate.ToUniversalTime();
        this.LastPasswordChangedDate = user.LastPasswordChangedDate.ToUniversalTime();
      }
      finally
      {
        this.DisableDataLengthChanged = false;
        this.RaiseDataLengthChanged();
      }
    }

    public IReadOnlyDictionary<string, object> Properties
    {
      get
      {
        return this.properties;
      }
    }

    public T GetProperty<T>(string name, T defaultValue = default(T))
    {
      object obj;

      if (this.properties.TryGetValue(name, out obj) && obj is T)
      {
        return (T)obj;
      }

      return defaultValue;
    }

    public void SetProperty<T>(string name, T value)
    {
      this.properties[name] = value;

      this.RaiseDataLengthChanged();
    }

    public string Id
    {
      get { return this.GetProperty<string>(this.FieldMapping.Id); }
      set { this.SetProperty(this.FieldMapping.Id, value); }
    }

    public string Login
    {
      get { return this.GetProperty<string>(this.FieldMapping.Login); }
      set { this.SetProperty(this.FieldMapping.Login, value); }
    }

    public string Email
    {
      get { return this.GetProperty<string>(this.FieldMapping.Email); }
      set { this.SetProperty(this.FieldMapping.Email, value); }
    }

    public string PasswordQuestion
    {
      get { return this.GetProperty<string>(this.FieldMapping.PasswordQuestion); }
      set { this.SetProperty(this.FieldMapping.PasswordQuestion, value); }
    }

    public string Description
    {
      get { return this.GetProperty<string>(this.FieldMapping.Description); }
      set { this.SetProperty(this.FieldMapping.Description, value); }
    }

    public bool IsApproved     
    {
      get { return this.GetProperty<bool>(this.FieldMapping.IsApproved); }
      set { this.SetProperty(this.FieldMapping.IsApproved, value); }
    }

    public bool IsLockedOut { get { return false; } }

    public DateTime CreatedDate
    {
      get { return this.GetProperty(this.FieldMapping.CreatedDate, SalesforceMinDate); }
      set
      {
        this.SetProperty(this.FieldMapping.CreatedDate, value >= SalesforceMinDate ? value : SalesforceMinDate);
      }
    }

    public DateTime LastLoginDate
    {
      get { return this.GetProperty(this.FieldMapping.LastLoginDate, SalesforceMinDate); }
      set
      {
        this.SetProperty(this.FieldMapping.LastLoginDate, value >= SalesforceMinDate ? value : SalesforceMinDate);
      }
    }

    public DateTime LastActivityDate
    {
      get { return this.GetProperty(this.FieldMapping.LastActivityDate, SalesforceMinDate); }
      set
      {
        this.SetProperty(this.FieldMapping.LastActivityDate, value >= SalesforceMinDate ? value : SalesforceMinDate);
      }
    }

    public DateTime LastPasswordChangedDate
    {
      get { return this.GetProperty(this.FieldMapping.LastPasswordChangedDate, SalesforceMinDate); }
      set
      {
        this.SetProperty(this.FieldMapping.LastPasswordChangedDate, value >= SalesforceMinDate ? value : SalesforceMinDate);
      }
    }

    public DateTime LastLockoutDate
    {
      get
      {
        return SalesforceMinDate;
      }
    }

    #region ICacheable

    long ICacheable.GetDataLength()
    {
      // TODO: consider what properties should be included
      return 
        TypeUtil.SizeOfString(this.Id) +
        TypeUtil.SizeOfString(this.Login) +
        TypeUtil.SizeOfString(this.Email) +
        TypeUtil.SizeOfString(this.Description);
    }

    bool ICacheable.Cacheable
    {
      get
      {
        return cacheable;
      }
      set
      {
        cacheable = value;
      }
    }

    bool ICacheable.Immutable
    {
      get
      {
        return false;
      }
    }

    public event DataLengthChangedDelegate DataLengthChanged;

    protected void RaiseDataLengthChanged()
    {
      if (this.DisableDataLengthChanged)
      {
        return;
      }

      var dataLengthChanged = this.DataLengthChanged;
      if (dataLengthChanged != null)
      {
        dataLengthChanged(this);
      }
    }

    #endregion
  }
}