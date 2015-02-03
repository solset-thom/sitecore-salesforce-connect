
namespace Sitecore.Salesforce.Caching
{
  using System;
  using System.Web.Security;

  using Reflection;

  using Sitecore.Data.Events;
  using Sitecore.Events;
  using Sitecore.Salesforce.Data;

  public class UserCache : SalesforceCacheBase
  {
    public UserCache(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.EventsSubscribe();
    }

    public UserCache(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.EventsSubscribe();
    }

    public void Add(ISalesforceContact salesforceUser)
    {
      base.InnerCache.Add(salesforceUser.Login, salesforceUser.Id, TypeUtil.SizeOfString(salesforceUser.Id), this.SlidingExpiration);
      base.InnerCache.Add(salesforceUser.Id, salesforceUser, salesforceUser.GetDataLength(), this.SlidingExpiration); 
    }

    public ISalesforceContact GetByKey(string userKey)
    {
      return base.InnerCache[userKey] as ISalesforceContact;
    }

    public ISalesforceContact GetByName(string userName)
    {
      object obj = base.InnerCache[userName];
      if (obj != null)
      {
        return this.GetByKey((string)obj);
      }

      return null;
    }

    public void RemoveByKey(string userKey)
    {
      ISalesforceContact cachibleObject = this.GetByKey(userKey);
      if (cachibleObject != null)
      {
        base.InnerCache.Remove(cachibleObject.Id);
        base.InnerCache.Remove(cachibleObject.Login);
      }
    }

    public void RemoveByName(string salesforceUserName)
    {
      object obj = base.InnerCache[salesforceUserName];
      if (obj != null)
      {
        this.RemoveByKey((string)obj);
      }
    }

    protected void EventsSubscribe()
    {
      Event.Subscribe("user:updated", this.OnUserUpdated);
      Event.Subscribe("user:updated:remote", this.OnUserUpdatedRemote);
      Event.Subscribe("user:deleted", this.OnUserDeleted);
      Event.Subscribe("user:deleted:remote", this.OnUserDeletedRemote);
    }

    protected virtual void OnUserUpdated(object sender, EventArgs args)
    {
      var user = Event.ExtractParameter<MembershipUser>(args, 0);

      if (user.ProviderUserKey != null)
      {
        this.RemoveByKey(user.ProviderUserKey.ToString());
      }

      //TODO:Review full names
      this.RemoveByName(user.UserName);
      this.RemoveByName(StringUtil.GetPostfix(user.UserName, '\\', user.UserName));
    }

    protected virtual void OnUserUpdatedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UserUpdatedRemoteEventArgs;
      if (remoteArgs != null)
      {
        //TODO:Review full names
        this.RemoveByName(remoteArgs.UserName);
        this.RemoveByName(StringUtil.GetPostfix(remoteArgs.UserName, '\\', remoteArgs.UserName));
      }
    }

    protected virtual void OnUserDeleted(object sender, EventArgs args)
    {
      string userName = Event.ExtractParameter<string>(args, 0);

      //TODO:Review full names
      this.RemoveByName(userName);
      this.RemoveByName(StringUtil.GetPostfix(userName, '\\', userName));
    }

    protected virtual void OnUserDeletedRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as UserDeletedRemoteEventArgs;
      if (remoteArgs != null)
      {
        //TODO:Review full names
        this.RemoveByName(remoteArgs.UserName);
        this.RemoveByName(StringUtil.GetPostfix(remoteArgs.UserName, '\\', remoteArgs.UserName));
      }
    }
  }
}