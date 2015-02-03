// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataStorageLoger.cs" company="Sitecore A/S">
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

  public class DataStorageLoger : LimitLoger
  {
    public DataStorageLoger(string configurationName, SLimits.LimitData limit)
      : base(configurationName, limit)
    {
    }

    public override void LogPointWarning()
    {
      LogHelper.Warn(
        string.Format(
          "Salesforce configuration {0} has used {1}% of its allowed data ({2}MB of {3}MB).",
          this.ConfigurationName,
          this.UsedPercents,
          this.UsedAmount,
          this.Limit.Max),
        this);
    }

    public override void LogHasReachedError()
    {
      LogHelper.Error(string.Format("Salesforce configuration {0} reached its {1}MB data limit!", this.ConfigurationName, this.Limit.Max), this);
    }
  }
}
