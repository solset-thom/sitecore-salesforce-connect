
namespace Sitecore.Salesforce.Tests.Security
{
  using System;
  using System.Linq;
  using System.Collections.Specialized;

  using NUnit.Framework;
  using Salesforce.Security;

  [TestFixture]
  [Category("Integration")]
  public class SalesforceRoleProviderTests
  {
    private SalesforceRoleProvider roleProvider;
    
    [TestFixtureSetUp]
    public void SetUpTest()
    {
      this.roleProvider = new SalesforceRoleProvider();
      this.roleProvider.Initialize("salesforce", new NameValueCollection() { { "readOnly", "false" }, { "applicationName", "sitecore" } });

      this.InitializeRoles("Test-Role-IsUserInRole", "Test-Role-1", "Test-Role-{0}-AddUsersToRoles-1", "Test-Role-{0}-AddUsersToRoles-2");
      this.InitializeUsersInRole(roleName: "Test-Role-IsUserInRole", userNames: "rose@edge.com");
      this.InitializeUsersInRole(roleName: "Test-Role-1", userNames: "tester-rolevich@test.com");
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {
    }

    public void InitializeRoles(params string[] roleNames)
    {
      var roles = this.roleProvider.GetAllRoles();
      foreach (var role in roleNames)
      {
        string strRole = role.Replace("{0}", Environment.UserName);
        if (!roles.Any(r => r.Equals(strRole)))
        {
          this.roleProvider.CreateRole(strRole);
        }
      }
    }

    public void InitializeUsersInRole(string roleName, params string[] userNames)
    {
      roleName = roleName.Replace("{0}", Environment.UserName);

      var notAssignedUsers = userNames.Where(userName => !this.roleProvider.IsUserInRole(userName, roleName)).ToArray();

      if (notAssignedUsers.Length > 0)
      {
        this.roleProvider.AddUsersToRoles(notAssignedUsers, new[] { roleName });
      }
    }

    [TestCase("rose@edge.com", "Test-Role-IsUserInRole", Result = true)]
    [TestCase("no name", "Test-Role-IsUserInRole", Result = false)]
    public bool IsUserInRoleTest(string userName, string roleName)
    {
      return this.roleProvider.IsUserInRole(userName, roleName);
    }

    [TestCase("no name", Result = new string[0])]
    [TestCase("rose@edge.com", Result = new string[] { "Test-Role-IsUserInRole" })]
    public string[] GetRolesForUserTest(string userNmae)
    {
      var roles = this.roleProvider.GetRolesForUser(userNmae);
      Assert.IsNotNull(roles);
      return roles;
    }

    [TestCase("Test-Role-Create-{0}")]
    public void CreateRoleTest(string roleName)
    {
      roleName = string.Format(roleName, Environment.UserName);

      this.roleProvider.DeleteRole(roleName, true);
      this.roleProvider.CreateRole(roleName);
      Assert.IsTrue(this.roleProvider.RoleExists(roleName));
      Assert.IsTrue(this.roleProvider.DeleteRole(roleName, true));
    }

    [TestCase(new string[] { "rose@edge.com", "jrogers@burlington.com" }, new string[] { "Test-Role-{0}-AddUsersToRoles-1", "Test-Role-{0}-AddUsersToRoles-2" })]
    public void AddUsersToRoles(string[] userNames, string[] roleNames)
    {
      roleNames = roleNames.Select(rn => rn.Replace("{0}", Environment.UserName)).ToArray();

      try
      {
        this.roleProvider.AddUsersToRoles(userNames, roleNames);
        foreach (var roleName in roleNames)
        {
          Assert.IsTrue(this.roleProvider.GetUsersInRole(roleName).Any(userNames.Contains));
        }
      }
      finally
      {
        this.roleProvider.RemoveUsersFromRoles(userNames, roleNames);
      }
    }

    [Test]
    public void GetAllRoles()
    {
      var roles = this.roleProvider.GetAllRoles();
      Assert.NotNull(roles);
      Assert.IsTrue(roles.Length > 0);
    }

    [TestCase("Test-Role-1", "tester")]
    [TestCase("Test-Role-1", "rolevich")]
    [TestCase("Test-Role-1", "tester-rolevich")]
    [TestCase("Test-Role-1", "ter-rolevich")]
    public void FindUsersInRoleTest(string roleName, string userNameToMatch)
    {
      var users = this.roleProvider.FindUsersInRole(roleName, userNameToMatch);
      Assert.NotNull(users);
      Assert.IsTrue(users.Any());
    }
  }
}
