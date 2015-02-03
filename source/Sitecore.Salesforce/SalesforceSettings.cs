// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the Settings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce
{
  using Sitecore.Configuration;

  public static class SalesforceSettings
  {
    public static readonly bool Disabled = Settings.GetBoolSetting("Salesforce.Disabled", false);
  }
}