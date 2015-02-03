namespace Sitecore.Salesforce.Client.Exceptions
{
  using System;
  using System.Runtime.Serialization;

  using Sitecore.Salesforce.Client.Data.Errors;

  [Serializable]
  public class SalesforceException : Exception
  {
    public ErrorCode ErrorCode { get; set; }

    public SalesforceException()
    {
    }

    public SalesforceException(string message)
      : base(message)
    {
    }

    public SalesforceException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SalesforceException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
