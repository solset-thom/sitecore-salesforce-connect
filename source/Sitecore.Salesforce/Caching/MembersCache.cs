namespace Sitecore.Salesforce.Caching
{
  using System;

  using Sitecore.Data.Events;
  using Sitecore.Events;

  public class MembersCache : UserRoleCacheBase
  {
    public MembersCache(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
    }

    public MembersCache(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
    }

    protected override void OnUsersToRolesAdded(object sender, EventArgs args)
    {
      this.RemoveRolesFromCache(args);
    }

    protected override void OnUsersToRolesAddedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UsersAddedToRolesRemoteEventArgs;
      if (remoteArgs == null)
      {
        return;
      }

      foreach (string roleName in remoteArgs.RoleNames)
      {
        //TODO:Review full names
        this.Remove(roleName);
        this.Remove(StringUtil.GetPostfix(roleName, '\\', roleName));
      }
    }

    protected override void OnUsersFromRolesRemoved(object sender, EventArgs args)
    {
      this.RemoveRolesFromCache(args);
    }

    protected override void OnUsersFromRolesRemovedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UsersRemovedFromRolesRemoteEventArgs;
      if (remoteArgs == null)
      {
        return;
      }

      foreach (string roleName in remoteArgs.RoleNames)
      {
        //TODO:Review full names
        this.Remove(roleName);
        this.Remove(StringUtil.GetPostfix(roleName, '\\', roleName));
      }
    }

    protected override void OnUserDeleted(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected override void OnUserDeletedRemote(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void RemoveRolesFromCache(EventArgs args)
    {
      string[] roleNames = Event.ExtractParameter<string[]>(args, 1);

      foreach (string roleName in roleNames)
      {
        //TODO:Review full names
        this.Remove(roleName);
        this.Remove(StringUtil.GetPostfix(roleName, '\\', roleName));
      }
    }
  }
}