namespace Sitecore.Salesforce.Pipelines.SubmitContact
{
  using System;

  using Sitecore.Analytics;
  using Sitecore.Analytics.Model;
  using Sitecore.Pipelines;

  [Obsolete("Just for testing. Should be removed in future.")]
  public class TestIdentity
  {
    public string UserName { get; set; }

    public void Process(PipelineArgs args)
    {
      if (Tracker.IsActive && Tracker.Current.Contact.Identifiers.IdentificationLevel != ContactIdentificationLevel.Known)
      {
        Tracker.Current.Session.Identify(this.UserName);
      }
    }
  }
}