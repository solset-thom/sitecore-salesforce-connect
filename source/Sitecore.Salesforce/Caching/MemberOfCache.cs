namespace Sitecore.Salesforce.Caching
{
  using System;

  using Sitecore.Data.Events;
  using Sitecore.Events;

  public class MemberOfCache : UserRoleCacheBase
  {
    public MemberOfCache(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
    }

    public MemberOfCache(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
    }

    protected override void OnUsersToRolesAdded(object sender, EventArgs args)
    {
      this.RemoveUsersFromCache(args);
    }

    protected override void OnUsersToRolesAddedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UsersAddedToRolesRemoteEventArgs;
      if (remoteArgs != null)
      {
        foreach (string userName in remoteArgs.UserNames)
        {
          //TODO:Review full names
          this.Remove(userName);
          this.Remove(StringUtil.GetPostfix(userName, '\\', userName));
        }
      }
    }

    protected override void OnUsersFromRolesRemoved(object sender, EventArgs args)
    {
      this.RemoveUsersFromCache(args);
    }

    protected override void OnUsersFromRolesRemovedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UsersRemovedFromRolesRemoteEventArgs;
      if (remoteArgs != null)
      {
        foreach (string userName in remoteArgs.UserNames)
        {
          //TODO:Review full names
          this.Remove(userName);
          this.Remove(StringUtil.GetPostfix(userName, '\\', userName));
        }
      }
    }

    protected override void OnUserDeleted(object sender, EventArgs args)
    {
      this.RemoveUserFromCache(args);
    }

    protected override void OnUserDeletedRemote(object sender, EventArgs args)
    {
      var remoteEventArgs = args as UserDeletedRemoteEventArgs;
      if (remoteEventArgs != null)
      {
        //TODO:Review full names
        this.Remove(remoteEventArgs.UserName);
        this.Remove(StringUtil.GetPostfix(remoteEventArgs.UserName, '\\', remoteEventArgs.UserName));
      }
    }

    protected virtual void RemoveUsersFromCache(EventArgs args)
    {
      string[] userNames = Event.ExtractParameter<string[]>(args, 0);

      foreach (string userName in userNames)
      {
        //TODO:Review full names
        this.Remove(userName);
        this.Remove(StringUtil.GetPostfix(userName, '\\', userName));
      }
    }

    protected virtual void RemoveUserFromCache(EventArgs args)
    {
      string userName = Event.ExtractParameter<string>(args, 0);

      //TODO:Review full names
      this.Remove(userName);
      this.Remove(StringUtil.GetPostfix(userName, '\\', userName));
    }
  }
}