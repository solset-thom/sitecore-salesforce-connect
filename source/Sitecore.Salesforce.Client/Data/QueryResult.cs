// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryResult.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the QueryResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client.Data
{
  using System.Collections.Generic;

  using Newtonsoft.Json;

  using Sitecore.Salesforce.Client.Json.Converters;

  public class QueryResult<T>
  {
    [JsonProperty("records")]
    public ICollection<T> Records { get; set; }

    [JsonProperty("totalSize")]
    public int TotalSize { get; set; }

    [JsonProperty("done")]
    [JsonConverter(typeof(BooleanTrueFalseConverter))]
    public bool Done { get; set; }

    [JsonProperty("nextRecordsUrl")]
    public string NextRecordsUrl { get; set; }

    public QueryResult()
    {
      this.Records = new List<T>();
    }
  }
}