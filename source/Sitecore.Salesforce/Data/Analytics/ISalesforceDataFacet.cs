namespace Sitecore.Salesforce.Data.Analytics
{
  using Sitecore.Analytics.Model.Entities;
  using Sitecore.Analytics.Model.Framework;

  public interface ISalesforceDataFacet : IFacet , IWriteSalesforceData
  {
    IContactPersonalInfo Personal { get; }

    IEmailAddress Email { get;}

    IPhoneNumber Phone { get; }
  }
}