namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Caching;
  using Sitecore.Salesforce.Diagnostics;

  public class SalesforceCacheConfiguration : SalesforceConfigurationEntry, ISalesforceCacheConfiguration
  {
    public UserCache Users { get; set; }

    public RoleCache Roles { get; set; }

    public MembersCache Members { get; set; }

    public MemberOfCache MemberOf { get; set; }

    public virtual void ClearAll()
    {
      if (!this.IsValid())
      {
        return;
      }

      this.Users.Clear();
      this.Roles.Clear();
      this.Members.Clear();
      this.MemberOf.Clear();
    }

    public override bool IsValid()
    {
      if (this.Users == null)
      {
        LogHelper.Error("Users property is null.", this);
        return false;
      }
      if (this.Roles == null)
      {
        LogHelper.Error("Roles property is null.", this);
        return false;
      }
      if (this.Members == null)
      {
        LogHelper.Error("Members property is null.", this);
        return false;
      }
      if (this.MemberOf == null)
      {
        LogHelper.Error("MemberOf is null", this);
        return false;
      }

      return true;
    }
  }
}