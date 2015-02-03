// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimitLoger.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ILimitLoger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  using System;

  using Sitecore.Salesforce.Client.Data;

  public abstract class LimitLoger : ILimitLoger
  {
    protected LimitLoger(string configurationName, SLimits.LimitData limit)
    {
      this.ConfigurationName = configurationName;
      this.Limit = limit;

      this.UsedAmount = limit.Max - limit.Remaining;
      this.UsedPercents = (int)Math.Round((double)this.UsedAmount * 100 / limit.Max);
    }


    public int UsedAmount { get; private set; }
    
    public int UsedPercents { get; private set; }

    protected string ConfigurationName { get; private set; }

    protected SLimits.LimitData Limit { get; private set; }
    

    public abstract void LogPointWarning();

    public abstract void LogHasReachedError(); 
  }
}