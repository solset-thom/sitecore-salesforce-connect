// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactsApiTests.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ContactsApiHelperTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tests.Api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using FakeDb;

  using NUnit.Framework;

  using Sitecore.Configuration;
  using Sitecore.Integration.Common.Providers;
  using Sitecore.Salesforce.Api;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Data;
  using Sitecore.Salesforce.Extensions;

  [TestFixture]
  [Category("Integration")]
  public class ContactsApiTests
  {
    private ISalesforceClient Client { get; set; }
    private IContactsApi ContactsApi { get; set; }
    private ISalesforceFieldMapping FieldMapping { get; set; }

    [TestFixtureSetUp]
    public void SetUpTest()
    {
      ProviderHelper<SalesforceProvider, ProviderCollection<SalesforceProvider>>.DefaultProvider = new SalesforceProvider();

      var configuration = SalesforceManager.GetConfiguration("salesforce");

      this.Client = configuration.Client;
      this.ContactsApi = configuration.Api.ContactsApi;
      this.FieldMapping = configuration.FieldMapping;
    }

    [TestFixtureTearDown]
    public void TearDownTest()
    {
    }

    [Test]
    public void GetAllTest()
    {
      var clientContacts = this.ContactsApi.GetAll(0, int.MaxValue);

      Assert.IsTrue(clientContacts.Any());
    }

    [TestCase("rose@edge.com", true)]
    [TestCase("a_young", true)]
    [TestCase("dickenson.com", true)]
    [TestCase("dickenson", true)]
    [TestCase("notRealEmail", false)]
    public void FindByEmailTest(string email, bool shouldBeFound)
    {
      var clientContacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.Email, email, Soql.ComparisonOperator.Contains, 0, 1);

      Assert.IsTrue(clientContacts.Any() == shouldBeFound);
    }

    [TestCase("rose@edge.com", true)]
    [TestCase("rose@edge", true)]
    [TestCase("a_young", true)]
    [TestCase("notRealName", false)]
    public void FindByNameTest(string loginName, bool shouldBeFound)
    {
      var clientContacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.Login, loginName, Soql.ComparisonOperator.Contains, 0, 1);

      Assert.IsTrue(clientContacts.Any() == shouldBeFound);
    }

    [TestCase("a_young@dickenson.com")]
    [TestCase("lboyle@uog.com")]
    public void GetTest(string loginName)
    {
      var contact = this.ContactsApi.Get(loginName);
      Assert.NotNull(contact);
      Assert.AreEqual(loginName, contact.Login);
    }
    
    [TestCase("003w000001F0avLAAR")]
    [TestCase("003w000001F0avMAAR")]
    public void GetTest(object id)
    {
      var contact = this.ContactsApi.Get(id);

      Assert.NotNull(contact);
      Assert.AreEqual(id, contact.Id);
    }

    [TestCase("CreateTest", "12345", "test@mail.com", "How are you?", "Ok", true, true)]
    [TestCase("CreateTest", "12345", "test@mail.com", null, null, true, true)]
    [TestCase(null, "12345", "test@mail.com", null, null, true, false)]
    [TestCase("CreateTest", null, "test@mail.com", null, null, true, true)]
    [TestCase("CreateTest", null, "test@mail.com", "How are you?", "Ok", true, true)]
    [TestCase(null, null, null, null, null, true, false)]
    [TestCase("CreateTest", "12345", null, "How are you?", "Ok", true, true)]
    [TestCase("CreateTest", "12345 ", "test@mail.com", "How are you?", "Ok", true, true)]
    [TestCase(" CreateTest ", "12345", "test@mail.com", "How are you?", "Ok", true, true)]
    [TestCase("CreateTest", "12345", "test@mail.com ", "How are you?", "Ok", true, true)]
    [TestCase("?&|!\\<>{}[]()^~*:\"'+-@", "12345", "rrrrr@test.com", "How's are you?", "Ok", true, true)]
    public void CreateTest(
      string loginName,
      string password,
      string email,
      string passwordQuestion,
      string passwordAnswer,
      bool isApproved,
      bool shouldBeTrue)
    {
      var newContactProperties = new Dictionary<string, object>
            {
              { "LastName", (loginName != null) ? loginName.Trim() : null },
              { "Email", (email != null) ? email.Trim() : null },
              { "SC_Password__c", (password != null) ? password.Trim() : null },
              { "SC_PasswordQuestion__c", (passwordQuestion != null) ? passwordQuestion.Trim() : null },
              { "SC_PasswordAnswer__c", (passwordAnswer != null) ? passwordAnswer.Trim() : null },
              { "SC_IsApproved__c", isApproved }
            };

      OperationResult result = null;

      try
      {
        result = this.ContactsApi.Create(newContactProperties);
      }
      catch (Exception ex)
      {
      }
      finally
      {
        if (this.ContactsApi.Get(newContactProperties[this.FieldMapping.Login] as string) != null)
        {
          this.ContactsApi.Delete(newContactProperties[this.FieldMapping.Login] as string);
        }
      }

      if (shouldBeTrue)
      {
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(!string.IsNullOrEmpty(result.Id));
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(!result.Errors.Any());
      }
      else
      {
        if (result != null)
        {
          Assert.IsTrue(!result.Success);
          Assert.IsTrue(string.IsNullOrEmpty(result.Id));
          Assert.IsTrue(result.Errors.Any());
        }
        else
        {
          Assert.IsNull(result);
        }
      }
    }

    [Explicit]
    [TestCase("ALH", "12345", "test@mail.com", "How are you?", "Ok", true, 1, 100)]
    public void CreateXNumberContacts(
      string loginName,
      string password,
      string email,
      string passwordQuestion,
      string passwordAnswer,
      bool isApproved,
      int fromSuffix,
      int toSuffix)
    {
      for (int i = fromSuffix; i <= toSuffix; i++)
      {
        var newContactProperties = new Dictionary<string, object>
              {
                {"LastName", loginName + i},
                {"Email", email},
                {"IsApproved", isApproved},
                {"Password", password},
                {"PasswordQuestion", passwordQuestion},
                {"PasswordAnswer", passwordAnswer}
              };

        bool success = this.ContactsApi.Create(newContactProperties).Success;

        Assert.IsTrue(success);
      }
    }

    [Explicit]
    [Test]
    public void FixAllContactsLoginNames()
    {
      var clientContacts = this.ContactsApi.GetAll(0, int.MaxValue);

      foreach (ISalesforceContact contact in clientContacts)
      {
        if (string.IsNullOrEmpty(contact.Login))
        {
          ISalesforceContact updateContact = new SalesforceContact(this.FieldMapping)
          {
            Id = contact.Id,
            Login = string.IsNullOrEmpty(contact.Email) ? contact.GetProperty<string>("Name") : contact.Email
          };

          updateContact.SetProperty("SC_Password__c", "123".GetSHA1Hash());

          var result = this.ContactsApi.Update(updateContact);
          Assert.IsTrue(result);
        }
      }
    }

    [Explicit]
    [TestCase("ALH", 1, 100)]
    public void DeleteXNumberContacts(string nameBase, int fromSuffix, int toSuffix)
    {
      for (int i = fromSuffix; i <= toSuffix; i++)
      {
        try
        {
          this.ContactsApi.Delete(nameBase + i);
        }
        catch
        {
        }
      }
    }

    [TestCase("Name", "Rose Gonzalez", Soql.ComparisonOperator.Equals, Result = 1)]
    [TestCase("Name", "ose Gonzal", Soql.ComparisonOperator.Contains, Result = 1)]
    [TestCase("Name", "ose Gonzal", Soql.ComparisonOperator.Equals, Result = 0)]
    public int GetTotalCountByFieldValue(string fieldName, string fieldValue, Soql.ComparisonOperator comparisonOperator)
    {
      return this.ContactsApi.GetTotalCountByFieldValue(fieldName, fieldValue, comparisonOperator);
    }

    [TestCase("Name", "ose Gonzal", Soql.ComparisonOperator.Equals, false)]
    [TestCase("LastName", "Gonzalez", Soql.ComparisonOperator.Equals, true)]
    [TestCase("Name", "Rose", Soql.ComparisonOperator.StartsWith, true)]
    public void FindByFieldValue(string fieldName, string fieldValue, Soql.ComparisonOperator comparisonOperator, bool expected)
    {
      var result = this.ContactsApi.FindByFieldValue(fieldName, fieldValue, comparisonOperator, 0, int.MaxValue);

      Assert.NotNull(result);
      Assert.AreEqual(expected, result.Any());
    }

    [Test]
    public void DeleteTest()
    {
      var newContactProperties = new Dictionary<string, object>
            {
              { "LastName", Environment.UserName + "_DeleteTest" },
              { "Email", Environment.UserName + "DeleteTest@sitecore.net" },
              { "SC_Password__c", "123" },
              { "SC_PasswordQuestion__c", "How are you?" },
              { "SC_PasswordAnswer__c", "Ok" },
              { "SC_IsApproved__c", true }
            };

      this.ContactsApi.Create(newContactProperties);
      bool result = this.ContactsApi.Delete(newContactProperties[this.FieldMapping.Login] as string);

      Assert.IsTrue(result);
    }

    [Test]
    public void UpdateTest()
    {
      var newContactProperties = new Dictionary<string, object>
            {
              { "LastName", Environment.UserName + "_UpdateTest" },
              { "Email", Environment.UserName + "UpdateTest@sitecore.net" },
              { "SC_Password__c", "123" },
              { "SC_PasswordQuestion__c", "How are you?" },
              { "SC_PasswordAnswer__c", "Ok" },
              { "SC_IsApproved__c", true }
            };

      var createResult = this.ContactsApi.Create(newContactProperties);

      ISalesforceContact contact = new SalesforceContact(this.FieldMapping, new Dictionary<string, object>(0))
      {
        Id = createResult.Id,
        Login = newContactProperties["LastName"] as string,
        Email = "checktest@mail.ua"
      };

      var result = this.ContactsApi.Update(contact);

      var updated = this.ContactsApi.Get(contact.Login);

      Assert.IsTrue(result);
      Assert.IsTrue(updated.Email == contact.Email);

      Assert.IsTrue(this.ContactsApi.Delete(contact.Login));
    }

    [TestCase("CheckHashedFieldEqualityTest", "SC_Password__c", "123")]
    public void CheckHashedFieldEqualityTest(string loginName, string fieldName, string password)
    {
      //TODO: should be changed.
      var newContactProperties = new Dictionary<string, object>
            {
              { "LastName", Environment.UserName + "_" + loginName },
              { "Email", Environment.UserName + loginName + "@sitecore.net" },
              { "SC_Password__c", password.GetSHA1Hash() }
            };

      OperationResult contact = this.ContactsApi.Create(newContactProperties);

      bool result = this.ContactsApi.CheckHashedFieldEquality(contact.Id, fieldName, password);

      Assert.True(result);

      Assert.IsTrue(this.ContactsApi.Delete(newContactProperties[this.FieldMapping.Login] as string));
    }
  }
}