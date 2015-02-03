// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SLimits.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SLimits type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client.Data
{
  using Newtonsoft.Json;

  public class SLimits
  {
    [JsonProperty("DailyApiRequests")]
    public LimitData DailyApiRequests { get; set; }

    [JsonProperty("DataStorageMB")]
    public LimitData DataStorageMb { get; set; }

    [JsonProperty("ConcurrentAsyncGetReportInstances")]
    public LimitData ConcurrentAsyncGetReportInstances { get; set; }

    [JsonProperty("ConcurrentSyncReportRuns")]
    public LimitData ConcurrentSyncReportRuns { get; set; }

    [JsonProperty("DailyAsyncApexExecutions")]
    public LimitData DailyAsyncApexExecutions { get; set; }

    [JsonProperty("DailyStreamingApiEvents")]
    public LimitData DailyStreamingApiEvents { get; set; }

    [JsonProperty("DailyWorkflowEmails")]
    public LimitData DailyWorkflowEmails { get; set; }

    [JsonProperty("FileStorageMB")]
    public LimitData FileStorageMb { get; set; }

    [JsonProperty("HourlyDashboardRefreshes")]
    public LimitData HourlyDashboardRefreshes { get; set; }

    [JsonProperty("HourlyDashboardResults")]
    public LimitData HourlyDashboardResults { get; set; }

    [JsonProperty("HourlyDashboardStatuses")]
    public LimitData HourlyDashboardStatuses { get; set; }

    [JsonProperty("HourlySyncReportRuns")]
    public LimitData HourlySyncReportRuns { get; set; }

    [JsonProperty("HourlyTimeBasedWorkflow")]
    public LimitData HourlyTimeBasedWorkflow { get; set; }

    [JsonProperty("MassEmail")]
    public LimitData MassEmail { get; set; }

    [JsonProperty("SingleEmail")]
    public LimitData SingleEmail { get; set; }

    [JsonProperty("StreamingApiConcurrentClients")]
    public LimitData StreamingApiConcurrentClients { get; set; }


    public class LimitData
    {
      [JsonProperty("Remaining")]
      public int Remaining { get; set; }

      [JsonProperty("Max")]
      public int Max { get; set; }
    }
  }
}