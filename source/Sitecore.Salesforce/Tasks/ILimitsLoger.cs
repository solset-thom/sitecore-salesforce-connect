// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILimitsLoger.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ILimitsLoger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  public interface ILimitsLoger
  {
    ILimitLoger ApiDailyRequestsLoger { get; }

    ILimitLoger DataStorageLoger { get; }


    void LogStatisticInfo();

    void LogInformationAreNotAvailableWarning();
  }
}