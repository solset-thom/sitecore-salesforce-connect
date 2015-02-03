namespace Sitecore.Salesforce.Client.Exceptions
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class SalesforceAuthException : SalesforceException
  {
    public SalesforceAuthException()
    {
    }

    public SalesforceAuthException(string message)
      : base(message)
    {
    }

    public SalesforceAuthException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SalesforceAuthException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}