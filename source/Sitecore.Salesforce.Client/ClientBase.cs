namespace Sitecore.Salesforce.Client
{
  using System;
  using System.Net.Http;
  using System.Net.Http.Headers;

  using Sitecore.Diagnostics;

  public abstract class ClientBase : IDisposable
  {
    private volatile bool disposed;

    private string userAgent;
    private string apiVersion;

    protected readonly HttpClient HttpClient;


    protected ClientBase(HttpClient httpClient)
    {
      Assert.ArgumentNotNull(httpClient, "httpClient");

      this.HttpClient = httpClient;
      this.EnsureHeaders();
    }


    public string UserAgent
    {
      get
      {
        return this.userAgent ?? "Sitecore Salesforce Connector";
      }
      set
      {
        this.userAgent = value;
        this.EnsureHeaders();
      }
    }

    public string ApiVersion
    {
      get
      {
        return this.apiVersion ?? "v31.0";
      }
      set
      {
        this.apiVersion = value;
        this.EnsureHeaders();
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.disposed)
      {
        this.disposed = true;
        this.HttpClient.Dispose();
      }
    }

    protected void EnsureHeaders()
    {
      var headers = this.HttpClient.DefaultRequestHeaders;

      headers.UserAgent.Clear();
      headers.UserAgent.TryParseAdd(this.UserAgent + "/" + this.ApiVersion);

      headers.Accept.Clear();
      headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    protected virtual Uri BuildUri(string instanceUrl, string resourceName)
    {
      if (resourceName.StartsWith("/services/data", StringComparison.OrdinalIgnoreCase))
      {
        return new Uri(instanceUrl + resourceName);
      }

      string url = string.Format("{0}/services/data/{1}/{2}", instanceUrl, this.ApiVersion, resourceName);
      return new Uri(url);
    }
  }
}