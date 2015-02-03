namespace Sitecore.Salesforce.Api
{
  using System.Collections.Generic;
  using Client.Data;
  using Data;

  public interface IRolesApi
  {
    List<SalesforceRole> GetRolesForUser(string loginName);

    List<SalesforceRole> GetAllRoles();
    
    SalesforceRole Get(string roleName);

    List<string> GetUsersInRole(string roleName);

    List<string> FindUsersInRole(string roleName, string usernameToMatch);

    OperationResult Create(string roleName);

    bool DeleteById(string roleId);

    OperationResult AddUserToRole(string roleId, string userId);
    
    bool DeleteUserFromRole(string roleName, string loginName);
  }
}