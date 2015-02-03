
namespace Sitecore.Salesforce.Security
{
  using System;
  using System.Collections.Specialized;
  using System.Linq;
  using System.Web.Security;

  using Diagnostics;
  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Api;
  using Sitecore.Salesforce.Configuration;

  public class SalesforceRoleProvider : RoleProvider
  {
    public override string ApplicationName { get; set; }

    protected bool Initialized { get; set; }

    public bool ReadOnly { get; set; }

    protected IRolesApi RolesApi { get; set; }
    
    protected IContactsApi ContactsApi { get; set; }

    protected ISalesforceCacheConfiguration Cache { get; set; }

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
        this.RolesApi = configuration.Api.RolesApi;
        this.Cache = configuration.Cache;

        this.ApplicationName = config["applicationName"];
        this.ReadOnly = MainUtil.GetBool(config["readOnly"], false);
        this.Initialized = true;
      }
      catch (Exception ex)
      {
        this.Initialized = false;
        LogHelper.Error(string.Format("Provider couldn't be initialized. Provider name: {0}", this.Name), this, ex);
      }
    }

    public override bool IsUserInRole(string userName, string roleName)
    {
      Assert.ArgumentNotNull(userName, "userName");
      Assert.ArgumentNotNull(roleName, "roleName");

      if (!this.Initialized)
      {
        return false;
      }

      roleName = StringUtil.GetPostfix(roleName, '\\', roleName);

      return this.GetRolesForUser(userName).Contains(roleName);
    }

    public override string[] GetRolesForUser(string userName)
    {
      Assert.ArgumentNotNull(userName, "userName");

      if (!this.Initialized)
      {
        return new string[0];
      }

      userName = StringUtil.GetPostfix(userName, '\\', userName);
      var roleNames = this.Cache.MemberOf.Get(userName);

      if (roleNames != null)
      {
        return roleNames;
      }
      
      try
      {
        var roles = this.RolesApi.GetRolesForUser(userName);

        if (roles == null || roles.Count == 0)
        {
          return new string[0];
        }

        roleNames = roles.Select(r => r.Name).ToArray();

        this.Cache.MemberOf.Add(userName, roleNames);

        return roleNames;
      }
      catch (Exception ex)
      {
        LogHelper.Error("Operation failed.", this, ex);
      }
      
      return new string[0];
    }

    public override void CreateRole(string roleName)
    {
      Assert.ArgumentNotNull(roleName, "roleName");

      if (!this.Initialized)
      {
        throw new ApplicationException("Couldn't add role to Salesforce. Check log file for details.");
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't add role as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      var role = this.Cache.Roles.Get(roleName) ?? this.RolesApi.Get(roleName);

      if (role != null)
      {
        throw new NotSupportedException(string.Format("The role already exists. Role name: {0}", roleName));
      }

      var result = this.RolesApi.Create(roleName);
      if (result == null || !result.Success)
      {
        LogHelper.Warn(string.Format("Couldn't create role on Salesforce. Role name: {0}", roleName), this);
        throw new ApplicationException("Couldn't create role on Salesforce. Check log file for details.");
      }

      LogHelper.Info(string.Format("Role has been created on Salesforce. Role name: {0}", roleName), this);
    }

    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      Assert.ArgumentNotNull(roleName, "roleName");

      if (!this.Initialized)
      {
        return false;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't delete role as Salesforce provider is in Read-Only mode.", this.Name));
      }

      //TODO: Cache?
      var role = this.Cache.Roles.Get(roleName) ?? this.RolesApi.Get(roleName);
      if (role == null)
      {
        LogHelper.Warn(string.Format("The role doesn't exist on Salesforce. Role name: {0}", roleName), this);
        return false;
      }

      var isDeleted = this.RolesApi.DeleteById(role.Id);
      if (isDeleted)
      {
        LogHelper.Info(string.Format("The role has been deleted from Salesforce. Role name: {0}", roleName), this);
      }

      return isDeleted;
    }

    public override bool RoleExists(string roleName)
    {
      Assert.ArgumentNotNull(roleName, "roleName");

      if (!this.Initialized)
      {
        return false;
      }

      roleName = StringUtil.GetPostfix(roleName, '\\', roleName);

      var role = this.Cache.Roles.Get(roleName);
      if (role != null)
      {
        return true;
      }

      try
      {
        role = this.RolesApi.Get(roleName);
        if (role != null)
        {
          this.Cache.Roles.Add(role);
          return true;
        }
      }
      catch (Exception ex)
      {
        LogHelper.Error("Operation failed.", this, ex);
        return false;
      }

      return false;
    }

    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
    {
      Assert.ArgumentNotNull(usernames, "usernames");
      Assert.ArgumentNotNull(roleNames, "roleNames");

      if (!this.Initialized)
      {
        return;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't add users to roles as Salesforce provider is in Read-Only mode. Provider name: {0}",this.Name));
      }

      foreach (var roleName in roleNames)
      {
        foreach (var userName in usernames)
        {
          var role = this.Cache.Roles.Get(roleName) ?? this.RolesApi.Get(roleName);
          var user = this.Cache.Users.GetByName(userName) ?? this.ContactsApi.Get(userName);

          var result = this.RolesApi.AddUserToRole(role.Id, user.Id);
          if (result.Success)
          {
            LogHelper.Info(string.Format("User has been added to Salesforce role. User name: {0}; Role name: {1}", userName, roleName), this);
          }
          else
          {
            LogHelper.Warn(string.Format("Couldn't add user to Salesforce role. User name: {0}; Role name: {1}", userName, roleName), this);
          }
        }
      }
    }

    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
    {
      Assert.ArgumentNotNull(usernames, "usernames");
      Assert.ArgumentNotNull(roleNames, "roleNames");
      
      if (!this.Initialized)
      {
        return;
      }
      
      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't remove users from roles as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      foreach (var roleName in roleNames)
      {
        foreach (var userName in usernames)
        {
          if (this.RolesApi.DeleteUserFromRole(roleName, userName))
          {
            LogHelper.Info(string.Format("User has been removed from Salesforce role. User name: {0}; Role name: {1}", userName, roleName), this);
          }
          else
          {
            LogHelper.Warn(string.Format("Couldn't remove user from Salesforce role. User name: {0}; Role name: {1}", userName, roleName), this);
          }
        }
      }
    }

    public override string[] GetUsersInRole(string roleName)
    {
      Assert.ArgumentNotNull(roleName, "roleName");

      if (!this.Initialized)
      {
        return new string[0];
      }

      roleName = StringUtil.GetPostfix(roleName, '\\', roleName);

      var userNames = this.Cache.Members.Get(roleName);
      if (userNames != null)
      {
        return userNames;
      }

      userNames = this.RolesApi.GetUsersInRole(roleName).ToArray();

      this.Cache.Members.Add(roleName, userNames);

      return userNames;
    }

    public override string[] GetAllRoles()
    {
      if (!this.Initialized)
      {
        return new string[0];
      }
      try
      {
        var roles = RolesApi.GetAllRoles();

        if (roles.Count == 0)
        {
          return new string[0];
        }

        foreach (var role in roles)
        {
          this.Cache.Roles.Add(role);
        }

        return roles.Select(r => r.Name).ToArray();
      }
      catch (Exception ex)
      {
        LogHelper.Error("Operation failed.", this, ex);
        return new string[0];
      }
    }

    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
    {
      Assert.ArgumentNotNull(roleName, "roleName");
      
      if (!this.Initialized)
      {
        return new string[0];
      }

      roleName = StringUtil.GetPostfix(roleName, '\\', roleName);

      var cache = this.Cache.Members.Get(roleName);

      if (cache != null)
      {
        return cache.Where(r => r.Contains(usernameToMatch)).ToArray();
      }

      var usersInRole = this.RolesApi.FindUsersInRole(roleName, usernameToMatch).ToArray();

      this.Cache.Members.Add(roleName, usersInRole);

      return usersInRole;
    }
  }
}