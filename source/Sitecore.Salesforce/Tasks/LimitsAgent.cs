// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimitsAgent.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the APILimitsCheckingAgent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  using Sitecore.Salesforce.Client.Data;

  public class LimitsAgent
  {
    protected ILimitsLoger loger;

    public virtual bool LogStatistic { get; set; }

    public virtual int ApiUsageThreshold { get; set; }

    public virtual int DataStorageThreshold { get; set; }

    
    public virtual void Run()
    {
      foreach (var configuration in SalesforceManager.GetConfigurations())
      {
        SLimits limits = SalesforceManager.GetSalesforceLimits(configuration.Client);
        this.loger = new LimitsLoger(configuration.Name, limits);
        
        this.ProcessLimits(limits);
      }
    }

    public virtual void ProcessLimits(SLimits limits)
    {
      if (limits != null)
      {
        if (this.LogStatistic)
        {
          this.loger.LogStatisticInfo();
        }

        this.ProcessLimit(limits.DailyApiRequests, this.loger.ApiDailyRequestsLoger, this.ApiUsageThreshold);
        this.ProcessLimit(limits.DataStorageMb, this.loger.DataStorageLoger, this.DataStorageThreshold);
      }
      else
      {
        this.loger.LogInformationAreNotAvailableWarning();
      }
    }

    protected virtual void ProcessLimit(SLimits.LimitData limit, ILimitLoger limitLoger, int threshold)
    {
      if (threshold >= 0 && threshold <= 100)
      {
        if (limitLoger.UsedPercents >= threshold)
        {
          limitLoger.LogPointWarning();
        }
      }

      if (limit.Remaining == 0)
      {
        limitLoger.LogHasReachedError();
      }
    }
  }
}