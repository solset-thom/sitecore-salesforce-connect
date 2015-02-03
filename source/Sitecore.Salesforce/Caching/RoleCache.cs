
namespace Sitecore.Salesforce.Caching
{
  using System;
  using Data;

  using Sitecore.Data.Events;
  using Sitecore.Events;

  public class RoleCache : SalesforceCacheBase
  {
    public RoleCache(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize,slidingExpiration)
    {
      this.EventsSubscribe();
    }

    public RoleCache(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.EventsSubscribe();
    }

    public void Add(SalesforceRole role)
    {
      if (role != null && role.IsValid())
      {
        base.InnerCache.Add(role.Name, role, role.GetDataLength(), this.SlidingExpiration); 
      } 
    }

    public SalesforceRole Get(string roleName)
    {
      return base.InnerCache[roleName] as SalesforceRole;
    }

    public void Remove(string roleName)
    {
      base.Remove(roleName);
    }

    protected void EventsSubscribe()
    {
      Event.Subscribe("role:deleted", this.OnDelete);
      Event.Subscribe("role:deleted:remote", this.OnDeleteRemote);
    }

    protected virtual void OnDelete(object sender, EventArgs args)
    {
      string roleName = Event.ExtractParameter<string>(args, 0);

      //TODO:Review full names
      this.Remove(roleName);
      this.Remove(StringUtil.GetPostfix(roleName, '\\', roleName));
    }

    protected virtual void OnDeleteRemote(object sender, EventArgs args)
    {
      var remoteArgs = args as RoleDeletedRemoteEventArgs;
      if (remoteArgs != null)
      {
        //TODO:Review full names
        this.Remove(remoteArgs.RoleName);
        this.Remove(StringUtil.GetPostfix(remoteArgs.RoleName, '\\', remoteArgs.RoleName));
      }
    }
  }
}
