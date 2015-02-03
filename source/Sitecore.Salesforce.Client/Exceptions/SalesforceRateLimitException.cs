namespace Sitecore.Salesforce.Client.Exceptions
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class SalesforceRateLimitException : SalesforceException
  {
    public SalesforceRateLimitException()
    {
    }

    public SalesforceRateLimitException(string message)
      : base(message)
    {
    }

    public SalesforceRateLimitException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SalesforceRateLimitException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}