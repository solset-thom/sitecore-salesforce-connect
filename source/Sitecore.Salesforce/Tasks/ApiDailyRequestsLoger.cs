// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDailyRequestsLoger.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ApiDailyRequestsLoger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Diagnostics;

  public class ApiDailyRequestsLoger : LimitLoger
  {
    public ApiDailyRequestsLoger(string configurationName, SLimits.LimitData limit)
      : base(configurationName, limit)
    {
    }

    public override void LogPointWarning()
    {
      LogHelper.Warn(
        string.Format(
          "Salesforce configuration {0} has used {1}% of its allowed API daily calls ({2} of {3}).",
          this.ConfigurationName,
          this.UsedPercents,
          this.UsedAmount,
          this.Limit.Max),
        this);
    }

    public override void LogHasReachedError()
    {
      LogHelper.Error(string.Format("Salesforce configuration {0} has reached its {1} API daily call limit!", this.ConfigurationName, this.Limit.Max), this);
    }
  }
}
