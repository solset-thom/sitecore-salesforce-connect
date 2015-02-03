// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimitsLoger.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the LimitsLoger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Diagnostics;
  using Environment = System.Environment;

  public class LimitsLoger : ILimitsLoger
  {
    public LimitsLoger(string configurationName, SLimits limits)
    {
      this.ConfigurationName = configurationName;
      this.Limits = limits;

      this.ApiDailyRequestsLoger = new ApiDailyRequestsLoger(this.ConfigurationName, limits.DailyApiRequests);
      this.DataStorageLoger = new DataStorageLoger(this.ConfigurationName, limits.DataStorageMb);
    }


    public ILimitLoger ApiDailyRequestsLoger { get; private set; }

    public ILimitLoger DataStorageLoger { get; private set; }
    
    protected string ConfigurationName { get; private set; }

    protected SLimits Limits { get; private set; }
    

    public virtual void LogStatisticInfo()
    {
      LogHelper.Info(
        string.Format(
          "Account usage limits for Salesforce configuration:" + Environment.NewLine +
          "Configuration name: {0}" + Environment.NewLine +
          "API daily usage: {1} calls of {2} ({3}%)" + Environment.NewLine +
          "Data usage: {4}MB of {5}MB ({6}%)" + Environment.NewLine,
          this.ConfigurationName,
          this.ApiDailyRequestsLoger.UsedAmount,
          this.Limits.DailyApiRequests.Max,
          this.ApiDailyRequestsLoger.UsedPercents,
          this.DataStorageLoger.UsedAmount,
          this.Limits.DataStorageMb.Max,
          this.DataStorageLoger.UsedPercents),
        this);
    }

    public void LogInformationAreNotAvailableWarning()
    {
      LogHelper.Warn("Account usage limits: Limits information is not available! Configuration name: " + this.ConfigurationName, this);
    }
  }
}