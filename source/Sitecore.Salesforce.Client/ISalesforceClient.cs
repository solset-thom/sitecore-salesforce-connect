// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISalesforceClient.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ISalesforceClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client
{
  using System.Collections.Generic;
  using System.Linq;

  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Client.Data;

  public interface ISalesforceClient
  {
    T HttpGet<T>(string resource);

    T HttpPost<T>(string resource, object data);

    bool HttpPatch(string resource, object data);
    
    bool HttpDelete(string resource);
  }

  public static class SalesforceClientExtensions
  {
    public static QueryResult<T> Query<T>(this ISalesforceClient client, string query)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNullOrEmpty(query, "query");

      if (!query.StartsWith("query/?q="))
      {
        query = "query/?q=" + query;
      }

      return client.HttpGet<QueryResult<T>>(query);
    }

    public static IEnumerable<T> GetAll<T>(this ISalesforceClient client, QueryResult<T> queryResult)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNull(queryResult, "queryResult");

      foreach (var record in queryResult.Records)
      {
        yield return record;
      }

      while (!queryResult.Done)
      {
        queryResult = client.HttpGet<QueryResult<T>>(queryResult.NextRecordsUrl);

        foreach (var record in queryResult.Records)
        {
          yield return record;
        }
      }
    }

    public static IEnumerable<T> QueryAll<T>(this ISalesforceClient client, string query)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNullOrEmpty(query, "query");

      var queryResult = client.Query<T>(query);

      return client.GetAll(queryResult);
    }

    public static IEnumerable<T> QueryAllWithOffsetWorkaround<T>(this ISalesforceClient client, string query, int offset)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNullOrEmpty(query, "query");

      var queryResult = client.Query<T>(query);

      if (queryResult.Done)
      {
        return queryResult.Records.Skip(offset);
      }

      string url = queryResult.NextRecordsUrl.Split('-')[0] + "-" + offset;

      //request URL could be incorrect so catch exception
      try
      {
        queryResult = client.HttpGet<QueryResult<T>>(url);
      }
      catch
      {
        return Enumerable.Empty<T>();
      }

      return client.GetAll(queryResult);
    }
  }
}