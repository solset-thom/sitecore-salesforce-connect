// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceMembershipProvider.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceMembershipProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Security
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Configuration.Provider;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Web.Security;

  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Api;
  using Sitecore.Salesforce.Caching;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Data;
  using Sitecore.Salesforce.Diagnostics;
  using Sitecore.Salesforce.Extensions;
  using Sitecore.Salesforce.Soql;

  public class SalesforceMembershipProvider : MembershipProvider
  {
    protected bool requiresQuestionAndAnswer;

    protected bool enablePasswordReset;

    protected int minRequiredPasswordLength;

    protected int minRequiredNonalphanumericCharacters;

    protected int maxInvalidPasswordAttempts;

    protected int passwordAttemptWindow;

    protected string passwordStrengthRegularExpression;

    protected bool requiresUniqueEmail;

    public override string ApplicationName { get; set; }

    public override bool EnablePasswordReset
    {
      get
      {
        return this.enablePasswordReset;
      }
    }

    public override bool RequiresQuestionAndAnswer
    {
      get
      {
        return this.requiresQuestionAndAnswer;
      }
    }

    public override int MinRequiredPasswordLength
    {
      get
      {
        return this.minRequiredPasswordLength;
      }
    }

    public override int MinRequiredNonAlphanumericCharacters
    {
      get
      {
        return this.minRequiredNonalphanumericCharacters;
      }
    }

    public override int MaxInvalidPasswordAttempts
    {
      get
      {
        return this.maxInvalidPasswordAttempts;
      }
    }

    public override int PasswordAttemptWindow
    {
      get
      {
        return this.passwordAttemptWindow;
      }
    }

    public override string PasswordStrengthRegularExpression
    {
      get
      {
        return this.passwordStrengthRegularExpression;
      }
    }

    public override bool RequiresUniqueEmail
    {
      get
      {
        return this.requiresUniqueEmail;
      }
    }

    public override bool EnablePasswordRetrieval
    {
      get
      {
        return false;
      }
    }
    
    public override MembershipPasswordFormat PasswordFormat
    {
      get
      {
        return MembershipPasswordFormat.Hashed;
      }
    }

    public bool Initialized { get; private set; }

    public bool ReadOnly { get; private set; }

    protected ISalesforceClient Client { get; set; }

    protected IContactsApi ContactsApi { get; set; }

    protected ISalesforceFieldMapping FieldMapping { get; set; }

    protected ISalesforceCacheConfiguration Cache { get; set; }

    public override void Initialize(string name, NameValueCollection config)
    {
      base.Initialize(name, config);

      try
      {
        if (SalesforceSettings.Disabled)
        {
          LogHelper.Info("Salesforce connector is disabled.", this);
          return;
        }

        if (MainUtil.GetBool(config["disabled"], false))
        {
          LogHelper.Info(string.Format("Provider is disabled. Provider name: {0}", this.Name), this);
          return;
        }

        var configuration = SalesforceManager.GetConfiguration(name);
        if (configuration == null)
        {
          LogHelper.Error("Initialization failed. Configuration is null.", this);
          return;
        }

        this.ApplicationName = config["applicationName"];

        this.ReadOnly = MainUtil.GetBool(config["readOnly"], false);

        this.enablePasswordReset = MainUtil.GetBool(config["enablePasswordReset"], true);
        this.minRequiredPasswordLength = MainUtil.GetInt(config["minRequiredPasswordLength"], 7);
        this.minRequiredNonalphanumericCharacters = MainUtil.GetInt(config["minRequiredNonalphanumericCharacters"], 0);
        this.maxInvalidPasswordAttempts = MainUtil.GetInt(config["maxInvalidPasswordAttempts"], 5);
        this.passwordAttemptWindow = MainUtil.GetInt(config["passwordAttemptWindow"], 0);
        this.passwordStrengthRegularExpression = StringUtil.GetString(new[] { config["passwordStrengthRegularExpression"], string.Empty }).Trim();

        this.requiresQuestionAndAnswer = MainUtil.GetBool(config["requiresQuestionAndAnswer"], false);

        if (!string.IsNullOrEmpty(this.PasswordStrengthRegularExpression))
        {
          try
          {
            new Regex(this.PasswordStrengthRegularExpression);
          }
          catch (ArgumentException exception)
          {
            throw new ProviderException(exception.Message, exception);
          }
        }

        this.requiresUniqueEmail = MainUtil.GetBool(config["requiresUniqueEmail"], false);

        this.Client = configuration.Client;
        this.ContactsApi = configuration.Api.ContactsApi;
        this.FieldMapping = configuration.FieldMapping;
        this.Cache = configuration.Cache;

        this.Initialized = true;
      }
      catch (Exception ex)
      {
        this.Initialized = false;

        LogHelper.Error(string.Format("Provider initialization error. Provider name: {0}", this.Name), this, ex);
      }
    }

    public override MembershipUser CreateUser(
      string username,
      string password,
      string email,
      string passwordQuestion,
      string passwordAnswer,
      bool isApproved,
      object providerUserKey,
      out MembershipCreateStatus status)
    {
      if (!this.Initialized)
      {
        status = MembershipCreateStatus.UserRejected;
        return null;
      }

      if (this.ReadOnly)
      {
        status = MembershipCreateStatus.UserRejected;
        throw new NotSupportedException(string.Format("Couldn't create user as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }


      OperationResult result = null;

      if (!this.InitializeCreate(ref username, ref password, ref email, ref passwordQuestion, ref passwordAnswer, out status))
      {
        LogHelper.Info(string.Format("Couldn't create Salesforce user. User name: {0}; Create status: {1}", username, status), this);
        return null;
      }

      var newContactProperties = new Dictionary<string, object>
                                    {
                                      { "LastName", username.Split('@')[0] },
                                      { this.FieldMapping.Email, email },
                                      {
                                        this.FieldMapping.Password,
                                        string.IsNullOrEmpty(password) ? password : password.GetSHA1Hash()
                                      },
                                      { this.FieldMapping.IsApproved, isApproved }
                                    };

      if (this.RequiresQuestionAndAnswer)
      {
        newContactProperties[this.FieldMapping.PasswordQuestion] = passwordQuestion;
        newContactProperties[this.FieldMapping.PasswordAnswer] = passwordAnswer;
      }

      newContactProperties[this.FieldMapping.Login] = username;

      try
      {
        result = this.ContactsApi.Create(newContactProperties);
      }
      catch (Exception ex)
      {
        LogHelper.Error(string.Format("Couldn't add user to the current domain. User name: {0}; Domain: {1}", username, this.Name), this, ex);
        status = MembershipCreateStatus.ProviderError;
        return null;
      }

      if (result != null)
      {
        var newUser = this.GetUser((object)result.Id, false);

        if (newUser != null)
        {
          status = MembershipCreateStatus.Success;
          LogHelper.Info(string.Format("Salesforce user has been created. User name: {0}", username), this);
          return newUser;
        }
      }

      status = MembershipCreateStatus.ProviderError;
      if (result != null && result.Errors.Count > 0)
      {
        LogHelper.Error(string.Format("Couldn't create Salesforce user. User name: {0}; Provider errors: {1}", username, string.Join(Environment.NewLine, result.Errors)), this);
      }
      else
      {
        LogHelper.Error(string.Format("Couldn't create Salesforce user. User name: {0}", username), this);
      }

      return null;
    }
    
    public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
    {
      if (!this.Initialized)
      {
        return false;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't change password question or(and) password answer as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      try
      {
        var contact = this.ContactsApi.Get(username);
        if (contact == null)
        {
          LogHelper.Warn(string.Format("Couldn't find user during password question and answer changing operation. User name: {0}", username), this);
          return false;
        }

        bool isRightPassword = this.ContactsApi.CheckHashedFieldEquality(contact.Id, this.FieldMapping.Password, password);

        if (!isRightPassword)
        {
          LogHelper.Warn(string.Format("Changing of password question and answer for Salesforce user has not been successful. User name: {0}", username), this);
          return false;
        }

        contact.SetProperty(this.FieldMapping.PasswordQuestion, newPasswordQuestion);
        contact.SetProperty(this.FieldMapping.PasswordAnswer, newPasswordAnswer);

        var result = this.ContactsApi.Update(contact);
        if (result)
        {
          LogHelper.Info(string.Format("Password question and answer for Salesforce user have been changed. User name: {0}", username), this);
          return true;
        }
      }
      catch (Exception ex)
      {
        LogHelper.Error(string.Format("Exception during changing password question and answer for Salesforce user. User name: {0}", username), this, ex);
      }

      return false;
    }

    public override string GetPassword(string username, string answer)
    {
      throw new NotSupportedException("Geting password is not supported by Salesforce.");
    }

    public override bool ChangePassword(string username, string oldPassword, string newPassword)
    {
      if (!this.Initialized)
      {
        return false;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't change password as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      if (!this.IsPasswordValid(newPassword))
      {
        return false;
      }

      var contact = this.ContactsApi.Get(username);

      if (contact == null)
      {
        LogHelper.Warn(string.Format("Couldn't find Salesforce user during password changing operation. User name: {0}", username), this);
        return false;
      }

      bool isRightPassword = this.ContactsApi.CheckHashedFieldEquality(contact.Id, this.FieldMapping.Password, oldPassword);

      if (!isRightPassword)
      {
        LogHelper.Warn(string.Format("Changing password for Salesforce user has not been successful. User name: {0}", username), this);
        return false;
      }

      contact.SetProperty(this.FieldMapping.Password, newPassword.GetSHA1Hash());
      contact.SetProperty(this.FieldMapping.LastPasswordChangedDate, DateTime.UtcNow.ToString("s") + "Z");

      var result = this.ContactsApi.Update(contact);

      if (result)
      {
        LogHelper.Info(string.Format("Password for Salesforce user has been changed. User name: {0}", username), this);
        return true;
      }

      return false;
    }

    public override string ResetPassword(string username, string answer)
    {
      if (!this.Initialized)
      {
        throw new NotSupportedException(string.Format("Salesforce provider isn't initialized. Provider name: {0}", this.Name));
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't reset password as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      if (!this.EnablePasswordReset)
      {
        throw new NotSupportedException(string.Format("Salesforce provider is not configured to reset password. Provider name: {0}", this.Name));
      }

      var contact = this.ContactsApi.Get(username);

      if (contact == null)
      {
        throw new MembershipPasswordException(string.Format("Couldn't reset password. Salesforce user has not been found. User name: {0}", username));
      }

      if (this.RequiresQuestionAndAnswer)
      {
        bool isRightAnswer = this.ContactsApi.CheckHashedFieldEquality(contact.Id, this.FieldMapping.PasswordAnswer, answer);

        if (!isRightAnswer)
        {
          throw new MembershipPasswordException(string.Format("Couldn't reset password. Security answer is wrong. User name: {0}", username));
        }
      }

      var newPassword = Membership.GeneratePassword((this.MinRequiredPasswordLength < 14) ? 14 : this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);

      contact.SetProperty(this.FieldMapping.Password, newPassword.GetSHA1Hash());
      contact.SetProperty(this.FieldMapping.LastPasswordChangedDate, DateTime.UtcNow.ToString("s") + "Z");

      var result = this.ContactsApi.Update(contact);

      if (result)
      {
        LogHelper.Info(string.Format("Password for Salesforce user has been reset. User name: {0}", username), this);
        return newPassword;
      }

      throw new MembershipPasswordException("Could not reset password on Salesforce. Check log files for details.");
    }

    public override void UpdateUser(MembershipUser user)
    {
      Assert.ArgumentNotNull(user, "user");
      
      if (!this.Initialized)
      {
        throw new NotSupportedException(string.Format("Salesforce provider isn't initialized. Provider name: {0}", this.Name));
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't update user as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      var salesforceContact = new SalesforceContact(this.FieldMapping, user);

      if (string.IsNullOrEmpty(salesforceContact.Email) && this.RequiresUniqueEmail)
      {
        var contacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.Email, user.Email, ComparisonOperator.Contains, 0, 2);
        if (contacts.Count > 1)
        {
          throw new NotSupportedException(string.Format("Couldn't update Salesforce user. Salesforce provider requires a unique email. User name: {0}", user.UserName));
        }
      }

      if (this.ContactsApi.Update(salesforceContact))
      {
        this.Cache.Users.RemoveByKey(user.ProviderUserKey as string);
        LogHelper.Info(string.Format("Salesforce user has been updated. User name: {0}", user.UserName), this);
      }
      else
      {
        LogHelper.Warn(string.Format("Could not update Salesforce user. User name: {0}", user.UserName), this);
      }
    }

    public override bool ValidateUser(string username, string password)
    {
      if (!this.Initialized)
      {
        return false;
      }

      string name = StringUtil.GetPostfix(username, '\\', username);

      var contacts = this.ContactsApi.FindByFieldValue(this.FieldMapping.Login, name, ComparisonOperator.Equals, 0, 2);

      if (contacts.Count == 0)
      {
        return false;
      }

      if (contacts.Count > 1)
      {
        LogHelper.Warn(string.Format("Unable to resolve user. There are multiple users with the same login name. Login name: {0}", name), this);
        return false;
      }

      bool isRightPassword = this.ContactsApi.CheckHashedFieldEquality(contacts[0].Id, this.FieldMapping.Password, password);

      // TODO: May be this not our responsibility.
      //if (isRightPassword)
      //{
      //  contacts[0].SetProperty(this.FieldMapping.LastLoginDate, DateTime.UtcNow.ToString("s") + "Z");
      //  // TODO: Very hight additional load.
      //  // contacts[0].SetProperty(this.FieldMapping.LastActivityDate, DateTime.UtcNow.ToString("s") + "Z");
      //  this.ContactsApi.Update(contacts[0]);
      //}

      return isRightPassword;
    }

    public override bool UnlockUser(string userName)
    {
      return true;
    }

    public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
    {
      Assert.ArgumentNotNull(providerUserKey, "providerUserKey");
      
      if (!this.Initialized)
      {
        return null;
      }

      var user = this.FindUser(
        userCache => userCache.GetByKey((string)providerUserKey),
        () => this.ContactsApi.Get(providerUserKey));

      // TODO: Very hight additional load.
      // if (user != null && userIsOnline)
      // {
      // this.UpdateLastActivity(user as SalesforceMembershipUser);
      // }
      
      return user;
    }

    public override MembershipUser GetUser(string username, bool userIsOnline)
    {
      if (!this.Initialized)
      {
        return null;
      }

      if (string.IsNullOrEmpty(username) || username.Contains(@"\"))
      {
        return null;
      }

      var user = this.FindUser(userCache => userCache.GetByName(username), () => this.ContactsApi.Get(username));

      // TODO: Very hight additional load.
      // if (user != null && userIsOnline)
      // {
      // this.UpdateLastActivity(user as SalesforceMembershipUser);
      // }

      return user;
    }

    public override string GetUserNameByEmail(string email)
    {
      if (!this.Initialized)
      {
        return null;
      }

      int totalRecords;
      var users = this.FindUsersByEmail(email, 0, int.MaxValue, out totalRecords);

      if (users.Count > 0)
      {
        foreach (MembershipUser user in users)
        {
          return user.UserName;
        }
      }

      return null;
    }

    public override bool DeleteUser(string username, bool deleteAllRelatedData)
    {
      if (!this.Initialized)
      {
        return false;
      }

      if (this.ReadOnly)
      {
        throw new NotSupportedException(string.Format("Couldn't delete user as Salesforce provider is in Read-Only mode. Provider name: {0}", this.Name));
      }

      return this.ContactsApi.Delete(username);
    }

    public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new MembershipUserCollection();
      }

      return this.FindUsers(
        () => this.ContactsApi.GetAll(pageIndex, pageSize),
        () => this.ContactsApi.GetTotalCount(),
        out totalRecords);
    }

    public override int GetNumberOfUsersOnline()
    {
      throw new NotSupportedException("Geting online users are not supported by Salesforce.");
    }

    public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new MembershipUserCollection();
      }

      return
        this.FindUsers(
          () => this.ContactsApi.FindByFieldValue(this.FieldMapping.Login, usernameToMatch, ComparisonOperator.Contains, pageIndex, pageSize),
          () => this.ContactsApi.GetTotalCountByFieldValue(this.FieldMapping.Login, usernameToMatch, ComparisonOperator.Contains),
          out totalRecords);
    }

    public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      if (!this.Initialized)
      {
        totalRecords = 0;
        return new MembershipUserCollection();
      }

      return
        this.FindUsers(
          () => this.ContactsApi.FindByFieldValue(this.FieldMapping.Email, emailToMatch, ComparisonOperator.Contains, pageIndex, pageSize),
          () => this.ContactsApi.GetTotalCountByFieldValue(this.FieldMapping.Email, emailToMatch, ComparisonOperator.Contains),
          out totalRecords);
    }


    protected MembershipUser FindUser(Func<UserCache, ISalesforceContact> getUserFromCache, Func<ISalesforceContact> getUser)
    {
      Assert.ArgumentNotNull(getUserFromCache, "getUserFromCache");
      Assert.ArgumentNotNull(getUser, "getUser");

      if (!this.Initialized)
      {
        return null;
      }

      var contact = getUserFromCache(this.Cache.Users) ?? getUser();
      
      if (contact == null)
      {
        return null;
      }

      if (string.IsNullOrEmpty(contact.Login))
      {
        return null;
      }

      this.Cache.Users.Add(contact);

      return new SalesforceMembershipUser(this.Name, contact);
    }

    protected virtual MembershipUserCollection FindUsers(Func<IEnumerable<ISalesforceContact>> getUsers, Func<int> getTotalCount, out int totalRecords)
    {
      Assert.ArgumentNotNull(getUsers, "getUsers");

      totalRecords = 0;
      try
      {
        totalRecords = getTotalCount();

        var users = getUsers();

        var collection = new MembershipUserCollection();
        foreach (var salesforceUser in users)
        {
          if (!string.IsNullOrEmpty(salesforceUser.Login))
          {
            this.Cache.Users.Add(salesforceUser);
            collection.Add(new SalesforceMembershipUser(this.Name, salesforceUser));
          }
        }

        return collection;
      }
      catch (Exception ex)
      {
        LogHelper.Error("Operation failed.", this, ex);
        return new MembershipUserCollection();
      }
    }

    protected virtual bool InitializeCreate(
      ref string username,
      ref string password,
      ref string email,
      ref string passwordQuestion,
      ref string passwordAnswer,
      out MembershipCreateStatus status)
    {
      if (string.IsNullOrEmpty(username) || (username.Length > 256))
      {
        status = MembershipCreateStatus.InvalidUserName;
        return false;
      }

      username = username.Trim();

      try
      {
        if (this.ContactsApi.Get(username) != null)
        {
          status = MembershipCreateStatus.DuplicateUserName;
          return false;
        }

        if (string.IsNullOrEmpty(email))
        {
          status = MembershipCreateStatus.InvalidEmail;
          return false;
        }

        if (this.RequiresUniqueEmail)
        {
          if (this.ContactsApi.FindByFieldValue(this.FieldMapping.Email, email, ComparisonOperator.Contains, 0, 1).Count > 0)
          {
            status = MembershipCreateStatus.DuplicateEmail;
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.Error(string.Format("Exception during initializing Salesforce user. User name: {0}", username), this, ex);
        status = MembershipCreateStatus.ProviderError;
        return false;
      }

      if ((string.IsNullOrEmpty(password) || (password.Length > 128)) || (!this.IsPasswordValid(password)))
      {
        status = MembershipCreateStatus.InvalidPassword;
        return false;
      }

      password = password.Trim();

      if (passwordQuestion != null)
      {
        passwordQuestion = passwordQuestion.Trim();

        if (passwordQuestion == string.Empty || (passwordQuestion.Length > 256))
        {
          status = MembershipCreateStatus.InvalidQuestion;
          return false;
        }
      }

      if (passwordAnswer != null)
      {
        passwordAnswer = passwordAnswer.Trim();

        if (passwordAnswer == string.Empty || (passwordAnswer.Length > 128))
        {
          status = MembershipCreateStatus.InvalidAnswer;
          return false;
        }
      }

      status = MembershipCreateStatus.Success;

      return true;
    }

    protected virtual bool IsPasswordValid(string password)
    {
      if (password.Length < this.MinRequiredPasswordLength)
      {
        return false;
      }

      int num = 0;
      foreach (char ch in password)
      {
        if (!char.IsLetterOrDigit(ch))
        {
          num++;
        }
      }

      if (num < this.MinRequiredNonAlphanumericCharacters)
      {
        return false;
      }

      return string.IsNullOrEmpty(this.PasswordStrengthRegularExpression) || Regex.IsMatch(password, this.PasswordStrengthRegularExpression);
    }

    protected virtual void UpdateLastActivity(SalesforceMembershipUser user)
    {
      var contact = user.SalesforceContact;
      contact.SetProperty(this.FieldMapping.LastActivityDate, DateTime.UtcNow.ToString("s") + "Z");
      this.ContactsApi.Update(contact);
    }
  }
}