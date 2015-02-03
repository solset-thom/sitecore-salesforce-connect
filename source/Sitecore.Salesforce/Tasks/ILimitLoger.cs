// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILimitLoger.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ILimitLoger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tasks
{
  public interface ILimitLoger
  {
    int UsedAmount { get; }

    int UsedPercents { get; }
    

    void LogPointWarning();

    void LogHasReachedError(); 
  }
}