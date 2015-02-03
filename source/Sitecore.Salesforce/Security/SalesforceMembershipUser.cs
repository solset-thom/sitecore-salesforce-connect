// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceMembershipUser.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceMembershipUser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Security
{
  using System.Web.Security;

  using Sitecore.Salesforce.Data;

  public class SalesforceMembershipUser : MembershipUser
  {
    public SalesforceMembershipUser(string providerName, ISalesforceContact salesforceContact)
      : base(
          providerName,
          salesforceContact.Login,
          salesforceContact.Id,
          salesforceContact.Email,
          salesforceContact.PasswordQuestion,
          salesforceContact.Description,
          salesforceContact.IsApproved,
          false,
          creationDate: salesforceContact.CreatedDate.ToLocalTime(),
          lastLoginDate: salesforceContact.LastLoginDate.ToLocalTime(),
          lastActivityDate: salesforceContact.LastActivityDate.ToLocalTime(),
          lastPasswordChangedDate: salesforceContact.LastPasswordChangedDate.ToLocalTime(),
          lastLockoutDate: salesforceContact.LastLockoutDate)
    {
      this.SalesforceContact = salesforceContact;
    }

    public ISalesforceContact SalesforceContact { get; protected set; }
  }
}