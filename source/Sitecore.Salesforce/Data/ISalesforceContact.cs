
namespace Sitecore.Salesforce.Data
{
  using System;
  using System.Collections.Generic;
  using Sitecore.Caching;

  public interface ISalesforceContact : ICacheable
  {
    IReadOnlyDictionary<string, object> Properties { get; }

    T GetProperty<T>(string name, T defaultValue = default(T));

    void SetProperty<T>(string name, T value);


    string Id { get; }

    string Login { get; }

    string Email { get; }

    string PasswordQuestion { get; }

    string Description { get; }

    bool IsApproved { get; }

    bool IsLockedOut { get; }

    DateTime CreatedDate { get; }

    DateTime LastLoginDate { get; }

    DateTime LastActivityDate { get; }

    DateTime LastPasswordChangedDate { get; }

    DateTime LastLockoutDate { get; }
  }
}