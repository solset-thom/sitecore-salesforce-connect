namespace Sitecore.Salesforce.Pipelines.SubmitContact
{
  using System.Collections.Generic;
  using System.Web.Security;

  using Sitecore.Analytics.Pipelines.SubmitContact;
  using Sitecore.Salesforce.Data.Analytics;
  using Sitecore.Salesforce.Security;
  using Sitecore.Security.Accounts;
  using Sitecore.Security.Domains;

  public class WriteSalesforceData : SubmitContactProcessor
  {
    public string SalesforceFacet { get; set; }

    public List<string> Domains { get; private set; }

    public WriteSalesforceData()
    {
      this.Domains = new List<string>(1);
    }

    public override void Process(SubmitContactArgs args)
    {
      if (!this.IsValidContext(args))
      {
        return;
      }

      var user = this.GetUser(args.Contact.Identifiers.Identifier);
      if (user == null)
      {
        return;
      }

      var contact = user.SalesforceContact;
      ISalesforceDataFacet facet = args.Contact.GetFacet<ISalesforceDataFacet>(this.SalesforceFacet);

      facet.WriteSalesforceData(contact);
    }

    protected virtual bool IsValidContext(SubmitContactArgs args)
    {
      if (args.Contact == null)
      {
        return false;
      }

      string identifier = args.Contact.Identifiers.Identifier;
      if (string.IsNullOrEmpty(identifier))
      {
        return false;
      }

      if (this.Domains.Count > 0 && !this.Domains.Contains(Domain.ExtractDomainName(identifier)))
      {
        return false;
      }

      return true;
    }

    protected virtual SalesforceMembershipUser GetUser(string identifier)
    {
      if (string.IsNullOrEmpty(identifier))
      {
        return null;
      }

      MembershipUser user = Membership.GetUser(identifier);
      if (user == null)
      {
        return null;
      }

      var wrapper = user as MembershipUserWrapper;
      if (wrapper != null)
      {
        return wrapper.InnerUser as SalesforceMembershipUser;
      }

      return user as SalesforceMembershipUser;
    }
  }
}
