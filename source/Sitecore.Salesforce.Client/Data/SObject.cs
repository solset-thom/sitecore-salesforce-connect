// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SObject.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client.Data
{
  using Newtonsoft.Json;

  public class SObject
  {
    [JsonProperty("attributes")]
    public SAttributes Attributes { get; set; }

    public class SAttributes
    {
      [JsonProperty("type")]
      public string Type { get; set; }

      [JsonProperty("url")]
      public string Url { get; set; }
    }
  }
}