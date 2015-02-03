// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationResult.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the UpdateResponse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client.Data
{
  using System.Collections.Generic;

  using Newtonsoft.Json;

  using Sitecore.Salesforce.Client.Json.Converters;

  public class OperationResult
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("errors")]
    public List<object> Errors { get; set; }

    [JsonProperty("success")]
    [JsonConverter(typeof(BooleanTrueFalseConverter))]
    public bool Success { get; set; }

    public OperationResult()
    {
      this.Id = string.Empty;
      this.Errors = new List<object>(0);
    }
  }
}
