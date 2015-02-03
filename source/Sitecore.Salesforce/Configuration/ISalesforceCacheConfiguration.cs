namespace Sitecore.Salesforce.Configuration
{
  using Sitecore.Salesforce.Caching;

  public interface ISalesforceCacheConfiguration : ISalesforceConfigurationEntry
  {
    UserCache Users { get; }

    RoleCache Roles { get; }

    MembersCache Members { get; }

    MemberOfCache MemberOf { get; }

    void ClearAll();
  }
}