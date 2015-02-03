// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceMembershipProviderTests.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceMembershipProviderTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tests.Security
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Diagnostics;
  using System.Web.Security;

  using NUnit.Framework;

  using Sitecore.Salesforce.Api;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Extensions;
  using Sitecore.Salesforce.Security;

  using System.Threading.Tasks;

  public class SalesforceMembershipProviderTests
  {
    private Stopwatch timer = new Stopwatch();

    private SalesforceMembershipProvider membershipProvider;

    private ISalesforceClient Client { get; set; }

    private IContactsApi ContactsApi { get; set; }

    [TestFixtureSetUp]
    [Category("Integration")]
    public void SetUpTest()
    {
      this.membershipProvider = new SalesforceMembershipProvider();
      this.membershipProvider.Initialize(
        "salesforce",
        new NameValueCollection
          {
            { "applicationName", "sitecore" },
            { "readOnly", "false" }, 
            { "enablePasswordReset", "true" },
            { "minRequiredPasswordLength", "1" },
            { "passwordStrengthRegularExpression", ".*" },
            { "minRequiredNonalphanumericCharacters", "0" },
            { "maxInvalidPasswordAttempts", "5" },
            { "passwordAttemptWindow", "0" },
            { "requiresUniqueEmail", "true" }
          });

      var configuration = SalesforceManager.GetConfiguration("salesforce");

      this.Client = configuration.Client;
      this.ContactsApi = configuration.Api.ContactsApi;
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {
    }

    [Category("Integration")]
    [TestCase("salesforce", "ValidateUserTest@sitecore.net", "123", true)]
    public void ValidateUserTest(string domain, string loginName, string password, bool shouldBeFound)
    {
      string domainLogin = domain + "\\" + Environment.UserName + loginName;

      var newContactProperties = new Dictionary<string, object>
          {
            { "LastName", Environment.UserName + loginName.Split('@')[1] },
            { "Email", Environment.UserName + loginName },
            { "SC_Password__c", password.GetSHA1Hash() }
          };

      OperationResult contact = this.ContactsApi.Create(newContactProperties);

      this.timer.Start();
      bool result = this.membershipProvider.ValidateUser(domainLogin, password);
      this.timer.Stop();

      Assert.IsTrue(result == shouldBeFound);
      this.ContactsApi.Delete(newContactProperties["Email"] as string);

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }

    // Templorary
    [Category("Integration")]
    [Category("Performance")]
    //[TestCase(1)]
    //[TestCase(10)]
    //[TestCase(100)]
    //[TestCase(500)]
    //[TestCase(1000)]
    //[TestCase(1500)]
    //[TestCase(1750)]
    //[TestCase(1950)]
    //[TestCase(1999)]
    //[TestCase(2000)]
    //[TestCase(2500)]
    public void ValidateUserPfTest(int sessions)
    {
      string loginName = "ValidateUserPSTest@sitecore.net";
      string password = "123";

      var newContactProperties = new Dictionary<string, object>
          {
            { "LastName", Environment.UserName + loginName.Split('@')[1] },
            { "Email", Environment.UserName + loginName },
            { "SC_Password__c", password.GetSHA1Hash() }
          };

      OperationResult contact = this.ContactsApi.Create(newContactProperties);
      var tasks = new List<Task>();

      this.timer.Start();

      for (int i = 0; i < sessions; i++)
      {
        var task = Task.Factory.StartNew(this.ValidateUserTask);
        tasks.Add(task);
      }

      Task.WaitAll(tasks.ToArray());
      this.timer.Stop();

      this.ContactsApi.Delete(newContactProperties["Email"] as string);

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }

    private void ValidateUserTask()
    {
      bool result = this.membershipProvider.ValidateUser("salesforce\\" + Environment.UserName + "ValidateUserPSTest@sitecore.net", "123");

      Assert.IsTrue(result);
    }


    [Category("Integration")]
    [Category("Performance")]
    //[TestCase(1)]
    //[TestCase(10)]
    //[TestCase(100)]
    //[TestCase(500)]
    //[TestCase(1000)]
    //[TestCase(1500)]
    //[TestCase(1750)]
    //[TestCase(1950)]
    //[TestCase(1999)]
    //[TestCase(2000)]
    //[TestCase(2500)]
    public void GetUserPfTest(int sessions)
    {
      string loginName = "GetUserPfTest@sitecore.net";
      string password = "123";

      var newContactProperties = new Dictionary<string, object>
          {
            { "LastName", Environment.UserName + loginName.Split('@')[1] },
            { "Email", Environment.UserName + loginName },
            { "SC_Password__c", password.GetSHA1Hash() }
          };

      OperationResult contact = this.ContactsApi.Create(newContactProperties);
      var tasks = new List<Task>();

      this.timer.Start();

      for (int i = 0; i < sessions; i++)
      {
        var task = Task.Factory.StartNew(this.GetUserTask);
        tasks.Add(task);
      }

      Task.WaitAll(tasks.ToArray());
      this.timer.Stop();

      this.ContactsApi.Delete(newContactProperties["Email"] as string);

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }

    private void GetUserTask()
    {
      var user = this.membershipProvider.GetUser(Environment.UserName + "GetUserPfTest@sitecore.net", false);

      Assert.IsNotNull(user);
    }

    [Category("Integration")]
    [Category("Performance")]
    //[TestCase(1)]
    //[TestCase(10)]
    //[TestCase(100)]
    //[TestCase(500)]
    //[TestCase(1000)]
    //TestCase(1500)]
    //[TestCase(1750)]
    //[TestCase(1500)]
    //[TestCase(1999)]
    //[TestCase(2000)]
    //[TestCase(2500)]
    public void GetAllUserPfTest(int sessions)
    {
      var tasks = new List<Task>();

      this.timer.Start();

      for (int i = 0; i < sessions; i++)
      {
        var task = Task.Factory.StartNew(() => this.GetAllUserTask(i));
        tasks.Add(task);
      }

      Task.WaitAll(tasks.ToArray());
      this.timer.Stop();

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }

    private void GetAllUserTask(int sessions)
    {
      int total;
      MembershipUserCollection users = null;

      try
      {
        users = this.membershipProvider.GetAllUsers(0, int.MaxValue, out total);
      }
      catch (Exception ex)
      {
        this.timer.Stop();
        Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
        this.timer.Reset();
      }

      Assert.IsNotNull(users);
      Assert.IsTrue(users.Count > 0);
    }

    [Category("Integration")]
    [Category("Performance")]
    //[TestCase(1)]
    //[TestCase(10)]
    //[TestCase(100)]
    //[TestCase(500)]
    //[TestCase(1000)]
    //[TestCase(1500)]
    //[TestCase(1750)]
    //[TestCase(1950)]
    //[TestCase(1999)]
    //[TestCase(2000)]
    public void CreateUserPfTest(int sessions)
    {
      string loginName = "CreateUserPfTest";
      string password = "123";

      var tasks = new List<Task>();

      this.timer.Start();

      for (int i = 0; i < sessions; i++)
      {
        var task = Task.Factory.StartNew(() => this.CreateUserTask(loginName + i + "@sitecore.net", password));
        tasks.Add(task);
      }

      Task.WaitAll(tasks.ToArray());
      this.timer.Stop();

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }

    private void CreateUserTask(string name, string password)
    {
      MembershipCreateStatus status;
      var user = this.membershipProvider.CreateUser(name, password, name, "What?", "Dogs", true, null, out status);

      Assert.IsNotNull(user);
      Assert.IsTrue(status == MembershipCreateStatus.Success);
    }
    
    [Category("Integration")]
    [Category("Performance")]
    //[TestCase(0, 15)]
    //[TestCase(5, 15)]
    //[TestCase(10, 15)]
    //[TestCase(15, 15)]
    //[TestCase(20, 15)]
    //[TestCase(25, 15)]
    //[TestCase(30, 15)]
    //[TestCase(35, 15)]
    //[TestCase(40, 15)]
    //[TestCase(45, 15)]
    //[TestCase(50, 15)]
    //[TestCase(60, 15)]    // 900
    //[TestCase(70, 15)]
    //[TestCase(80, 15)]    // 1200
    //[TestCase(90, 15)]
    //[TestCase(100, 15)]
    //[TestCase(120, 15)]
    //[TestCase(140, 15)]
    //[TestCase(160, 15)]   // 2500
    //[TestCase(180, 15)]
    //[TestCase(200, 15)]   // 3000
    //[TestCase(250, 15)]
    //[TestCase(300, 15)]
    //[TestCase(650, 15)]   // < 10000
    public void GetAllUsersPagingPfTest(int pageIndex, int pageSize)
    {
      int total;

      this.timer.Start();

      MembershipUserCollection users = this.membershipProvider.GetAllUsers(pageIndex, pageSize, out total);

      this.timer.Stop();

      Assert.Pass("Working time: " + this.timer.Elapsed.ToString("g"));
      this.timer.Reset();
    }
  }
}