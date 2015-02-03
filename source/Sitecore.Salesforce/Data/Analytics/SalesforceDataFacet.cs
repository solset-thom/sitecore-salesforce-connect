
namespace Sitecore.Salesforce.Data.Analytics
{
  using System;

  using Sitecore.Analytics.Model.Entities;
  using Sitecore.Analytics.Model.Framework;

  [Serializable]
  public class SalesforceDataFacet : Facet, ISalesforceDataFacet
  {
    public SalesforceDataFacet()
    {
      base.EnsureElement<IContactPersonalInfo>("Personal");
      base.EnsureElement<IEmailAddress>("Email");
      base.EnsureElement<IPhoneNumber>("Phone");
    }

    public IContactPersonalInfo Personal
    {
      get
      {
        return base.GetElement<IContactPersonalInfo>("Personal");
      }
    }

    public IEmailAddress Email
    {
      get
      {
        return base.GetElement<IEmailAddress>("Email");
      }
    }

    public IPhoneNumber Phone
    {
      get
      {
        return base.GetElement<IPhoneNumber>("Phone");
      }
    }

    public virtual void WriteSalesforceData(ISalesforceContact contact)
    {
      var personal = this.Personal;

      personal.FirstName = contact.GetProperty<string>("FirstName");
      personal.Surname = contact.GetProperty<string>("LastName");
      personal.JobTitle = contact.GetProperty<string>("JobTitle");
      personal.BirthDate = contact.GetProperty<DateTime?>("Birthdate");

      this.Email.SmtpAddress = contact.Email;

      this.Phone.Number = contact.GetProperty<string>("Phone");
    }
  }
}
