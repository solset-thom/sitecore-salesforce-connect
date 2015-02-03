
namespace Sitecore.Salesforce.Tests.Api
{
  using System;
  using System.Linq;
  using NUnit.Framework;
  using Salesforce.Api;

  using Sitecore.Salesforce.Client;

  [TestFixture]
  [Category("Integration")]
  public class RolesApiTests
  {
    protected ISalesforceClient Client;
    protected IRolesApi RolesApi;
    protected IContactsApi ContactsApi;

    [TestFixtureSetUp]
    public void SetUpTest()
    {
      var configuration = SalesforceManager.GetConfiguration("salesforce");

      this.Client = configuration.Client;
      this.ContactsApi = configuration.Api.ContactsApi;
      this.RolesApi = configuration.Api.RolesApi;

      this.InitializeRoles("Test-Role-Get", "Test-Role-1");

      this.InitializeUsersInRole("Test-Role-1", "tester-rolevich@test.com");
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {

    }

    public void InitializeRoles(params string[] roleNames)
    {
      var roles = this.RolesApi.GetAllRoles().ToList();
      foreach (var role in roleNames)
      {
        if (!roles.Any(r => r.Name.Equals(role)))
        {
          this.RolesApi.Create(role);
        }
      }
    }

    public void InitializeUsersInRole(string roleName, params string[] loginNames)
    {
      var usersFromRole = this.RolesApi.GetUsersInRole(roleName).ToList();
      if (loginNames != null)
      {
        if (!usersFromRole.Any())
        {
          foreach (var userName in loginNames)
          {
            this.RolesApi.AddUserToRole(roleName, userName);
          }
        }

        foreach (var userName in usersFromRole)
        {
          if (!loginNames.Contains(userName))
          {
            this.RolesApi.AddUserToRole(roleName, userName);
          }
        }
      }
    }

    [TestCase("Test-Role-Get")]
    public void GetTest(string roleName)
    {
      var role = this.RolesApi.Get(roleName);

      Assert.IsNotNull(role);
      StringAssert.AreEqualIgnoringCase(roleName, role.Name);
    }

    [Test]
    public void GetAllRolesTest()
    {
      var roles = this.RolesApi.GetAllRoles().ToArray();
      Assert.IsNotNull(roles);
      Assert.IsTrue(roles.Any());
    }

    [TestCase("tester-rolevich@test.com")]
    public void GetRolesForUserTest(string loginName)
    {
      var roles = this.RolesApi.GetRolesForUser(loginName);
      Assert.IsNotNull(roles);
      Assert.IsTrue(roles.Any());
    }

    [TestCase("Test-Role-1")]
    public void GetUsersInRoleTest(string roleName)
    {
      var users = this.RolesApi.GetUsersInRole(roleName);
      Assert.IsNotNull(users);
      Assert.IsTrue(users.Any());
    }

    [TestCase("Test-Role-1", "tester-rolevich@test.com", "tester")]
    [TestCase("Test-Role-1", "tester-rolevich@test.com", "rolevich")]
    [TestCase("Test-Role-1", "tester-rolevich@test.com", "tester-rolevich@test.com")]
    [TestCase("Test-Role-1", "tester-rolevich@test.com", "ter-rolevich")]
    public void FindUsersInRoleTest(string roleName, string loginName, string userNameToMatch)
    {
      var users = this.RolesApi.FindUsersInRole(roleName, userNameToMatch);
      Assert.IsNotNull(users);
      Assert.IsTrue(users.Any());
    }

    [TestCase("Test-Role-{0}-Create")]
    [TestCase("Test-Role-{0}?&|!<>[]()^~*:\"'+-Create")]
    public void CreateTest(string roleName)
    {
      roleName = string.Format(roleName, Environment.UserName);

      var role = this.RolesApi.Get(roleName);
      if (role != null)
      {
        Assert.IsTrue(this.RolesApi.DeleteById(role.Id));
      }
      var result = this.RolesApi.Create(roleName);
      Assert.IsTrue(result.Success);
      Assert.IsTrue(this.RolesApi.DeleteById(result.Id));
    }

    [TestCase("rose@edge.com", "Test-role-{0}-add-user")]
    public void AddUserToRoleTest(string loginName, string roleName)
    {
      roleName = string.Format(roleName, Environment.UserName);

      var role = this.RolesApi.Get(roleName);
      var contact = this.ContactsApi.Get(loginName);
      Assert.NotNull(contact);
      string roleId;
      if (role != null)
      {
        roleId = role.Id;
      }
      else
      {
        var result = this.RolesApi.Create(roleName);
        roleId = result.Id;
      }
      
      var result2 = this.RolesApi.AddUserToRole(roleId, contact.Id);

      Assert.NotNull(result2);
      Assert.IsTrue(result2.Success);

      Assert.IsTrue(this.RolesApi.DeleteUserFromRole(roleName, loginName));
    }
  }
}