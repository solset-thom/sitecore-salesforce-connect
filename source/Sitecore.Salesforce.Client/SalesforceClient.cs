// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesforceClient.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the SalesforceClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Client
{
  using System;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;

  using Newtonsoft.Json;

  using Sitecore.Diagnostics;
  using Sitecore.Integration.Common;
  using Sitecore.Salesforce.Client.Data.Errors;
  using Sitecore.Salesforce.Client.Exceptions;
  using Sitecore.Salesforce.Client.Security;

  public class SalesforceClient : ClientBase, ISalesforceClient
  {
    private IRetryer retryer;

    protected volatile SalesforceRateLimitException LastRateLimitException;

    protected readonly IAuthenticator Authenticator;

    protected IRetryer Retryer
    {
      get
      {
        return this.retryer;
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        Assert.ArgumentCondition(value.RepeatNumber >= 1, "value", "RepeatNumber could not be less than 1.");

        this.retryer = value;
      }
    }

    protected IAuthToken AuthToken { get; set; }

    protected int RateLimitWaiting { get; set; }

    public SalesforceClient(IAuthenticator authenticator)
      : this(new HttpClient(), authenticator)
    {
    }

    public SalesforceClient(HttpClient httpClient, IAuthenticator authenticator)
      : base(httpClient)
    {
      Assert.ArgumentNotNull(authenticator, "authenticator");

      this.Authenticator = authenticator;

      this.UpdateToken();

      this.Retryer = new ActionRetryer(1, TimeSpan.Zero);

      this.RateLimitWaiting = 120;
    }

    public void UpdateToken()
    {
      IAuthToken token = this.Authenticator.Authenticate();

      this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

      this.AuthToken = token;
    }

    public virtual T HttpGet<T>(string resource)
    {
      this.HandleRateLimitExceptionCache();

      try
      {
        return this.Retryer.Execute(() => this.HttpGetAsync<T>(resource).Result, this.RecoverRequest);
      }
      catch (AggregateException ex)
      {
        throw ex.InnerException;
      }
    }

    public virtual T HttpPost<T>(string resource, object data)
    {
      this.HandleRateLimitExceptionCache();

      try
      {
        return this.Retryer.Execute(() => this.HttpPostAsync<T>(resource, data).Result, this.RecoverRequest);
      }
      catch (AggregateException ex)
      {
        throw ex.InnerException;
      }
    }

    public virtual bool HttpPatch(string resource, object data)
    {
      this.HandleRateLimitExceptionCache();

      try
      {
        return this.Retryer.Execute(() => this.HttpPatchAsync<bool>(resource, data).Result, this.RecoverRequest);
      }
      catch (AggregateException ex)
      {
        throw ex.InnerException;
      }
    }

    public virtual bool HttpDelete(string resource)
    {
      this.HandleRateLimitExceptionCache();

      try
      {
        return this.Retryer.Execute(() => this.HttpDeleteAsync(resource).Result, this.RecoverRequest);
      }
      catch (AggregateException ex)
      {
        throw ex.InnerException;
      }
    }

    public virtual async Task<T> HttpGetAsync<T>(string resource)
    {
      Assert.ArgumentNotNullOrEmpty(resource, "resource");

      Uri uri = this.BuildUri(this.AuthToken.InstanceUrl, resource);

      var request = new HttpRequestMessage
      {
        RequestUri = uri,
        Method = HttpMethod.Get
      };

      var responseMessage = await this.HttpClient.SendAsync(request).ConfigureAwait(false);
      var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

      if (!responseMessage.IsSuccessStatusCode)
      {
        this.HandleErrorResponse(response);
      }

      return JsonConvert.DeserializeObject<T>(response);
    }

    public virtual async Task<T> HttpPostAsync<T>(string resource, object data)
    {
      Uri uri = this.BuildUri(this.AuthToken.InstanceUrl, resource);

      var json = JsonConvert.SerializeObject(data);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var responseMessage = await this.HttpClient.PostAsync(uri, content).ConfigureAwait(false);
      var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

      if (!responseMessage.IsSuccessStatusCode)
      {
        this.HandleErrorResponse(response);
      }

      return JsonConvert.DeserializeObject<T>(response);
    }

    public virtual async Task<bool> HttpPatchAsync<T>(string resource, object data)
    {
      Uri uri = this.BuildUri(this.AuthToken.InstanceUrl, resource);

      var request = new HttpRequestMessage
      {
        RequestUri = uri,
        Method = new HttpMethod("PATCH"),
        Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
      };


      var responseMessage = await this.HttpClient.SendAsync(request).ConfigureAwait(false);
      var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

      if (!responseMessage.IsSuccessStatusCode)
      {
        this.HandleErrorResponse(response);
      }

      return true;
    }

    public virtual async Task<bool> HttpDeleteAsync(string resource)
    {
      Uri uri = this.BuildUri(this.AuthToken.InstanceUrl, resource);

      var request = new HttpRequestMessage
      {
        RequestUri = uri,
        Method = HttpMethod.Delete
      };

      var responseMessage = await this.HttpClient.SendAsync(request);
      var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

      if (!responseMessage.IsSuccessStatusCode)
      {
        this.HandleErrorResponse(response);
      }

      return true;
    }


    protected virtual void HandleErrorResponse(string response)
    {
      var errors = JsonConvert.DeserializeObject<ErrorResponse[]>(response);
      if (errors.Length <= 0)
      {
        return;
      }

      var error = errors[0];

      switch (error.ErrorCode)
      {
        case ErrorCode.INVALID_SESSION_ID:
          throw new SalesforceAuthException(error.Message) { ErrorCode = error.ErrorCode };
        case ErrorCode.REQUEST_LIMIT_EXCEEDED:
          throw new SalesforceRateLimitException(error.Message) { ErrorCode = error.ErrorCode };
        case ErrorCode.CANNOT_INSERT_UPDATE_ACTIVATE_ENTITY:
        case ErrorCode.INSUFFICIENT_ACCESS_OR_READONLY:
        case ErrorCode.INVALID_TYPE:
          throw new SalesforcePermissionException(error.Message) { ErrorCode = error.ErrorCode };
        default:
          throw new SalesforceException(error.Message) { ErrorCode = error.ErrorCode };
      }
    }

    protected virtual void HandleRateLimitExceptionCache()
    {
      if (this.LastRateLimitException != null)
      {
        throw this.LastRateLimitException;
      }
    }

    protected virtual bool RecoverRequest(Exception exception)
    {
      var ex = exception is AggregateException ? exception.InnerException : exception;

      if (ex is SalesforceAuthException)
      {
        this.UpdateToken();
        return true;
      }

      var rateLimitException = ex as SalesforceRateLimitException;
      if (rateLimitException != null)
      {
        this.LastRateLimitException = rateLimitException;

        Task.Factory.StartNew(
          () =>
            {
              Thread.Sleep(this.RateLimitWaiting * 1000);
              this.LastRateLimitException = null;
            });

        return false;
      }

      return !(ex is SalesforceException);
    }
  }
}