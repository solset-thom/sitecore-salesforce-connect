namespace Sitecore.Salesforce.Caching
{
  using System;

  using Sitecore.Events;

  public abstract class UserRoleCacheBase : StringArrayCache
  {
    protected UserRoleCacheBase(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.EventsSubscribe();
    }

    protected UserRoleCacheBase(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.EventsSubscribe();
    }

    protected void EventsSubscribe()
    {
      Event.Subscribe("roles:configChanged", this.OnRolesConfigChanged);
      Event.Subscribe("role:deleted", this.OnRoleDeleted);
      Event.Subscribe("role:deleted:remote", this.OnRoleDeletedRemote);
      Event.Subscribe("roles:rolesAdded", this.OnRolesToRolesAdded);
      Event.Subscribe("roles:rolesAdded:remote", this.OnRolesToRolesAddedRemote);
      Event.Subscribe("roles:rolesRemoved", this.OnRolesFromRolesRemoved);
      Event.Subscribe("roles:rolesRemoved:remote", this.OnRolesFromRolesRemovedRemote);
      Event.Subscribe("roles:relationsRemoved", this.OnRolesRelationsRemoved);
      Event.Subscribe("roles:relationsRemoved:remote", this.OnRolesRelationsRemovedRemote);
      Event.Subscribe("roles:usersAdded", this.OnUsersToRolesAdded);
      Event.Subscribe("roles:usersAdded:remote", this.OnUsersToRolesAddedRemote);
      Event.Subscribe("roles:usersRemoved", this.OnUsersFromRolesRemoved);
      Event.Subscribe("roles:usersRemoved:remote", this.OnUsersFromRolesRemovedRemote);
      Event.Subscribe("user:deleted", this.OnUserDeleted);
      Event.Subscribe("user:deleted:remote", this.OnUserDeletedRemote);
    }

    protected virtual void OnRolesConfigChanged(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRoleDeleted(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRoleDeletedRemote(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesToRolesAdded(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesToRolesAddedRemote(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesFromRolesRemoved(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesFromRolesRemovedRemote(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesRelationsRemoved(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected virtual void OnRolesRelationsRemovedRemote(object sender, EventArgs args)
    {
      this.Clear();
    }

    protected abstract void OnUsersToRolesAdded(object sender, EventArgs args);

    protected abstract void OnUsersToRolesAddedRemote(object sender, EventArgs args);

    protected abstract void OnUsersFromRolesRemoved(object sender, EventArgs args);

    protected abstract void OnUsersFromRolesRemovedRemote(object sender, EventArgs args);

    protected abstract void OnUserDeleted(object sender, EventArgs args);

    protected abstract void OnUserDeletedRemote(object sender, EventArgs args);
  }
}