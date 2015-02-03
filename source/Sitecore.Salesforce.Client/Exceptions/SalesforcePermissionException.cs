namespace Sitecore.Salesforce.Client.Exceptions
{
  using System;
  using System.Runtime.Serialization;
  using System.Text;

  [Serializable]
  public class SalesforcePermissionException : SalesforceException
  {
    public SalesforcePermissionException()
    {
    }

    public SalesforcePermissionException(string message)
      : base(message)
    {
    }

    public SalesforcePermissionException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SalesforcePermissionException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override string Message
    {
      get
      {
        var messageBuilder = new StringBuilder();
        messageBuilder.Append("Sitecore attempted to make a change in Salesforce that is not allowed by the current Salesforce permissions configuration.").AppendLine();
        messageBuilder.AppendLine("* Solution: Contact your Sitecore administrator for assistance or configure the Salesforce permissions described in the product documentation");
        messageBuilder.AppendFormat("* Salesforce message: {0}", base.Message).AppendLine();
        messageBuilder.AppendFormat("* Salesforce error code: {0}", base.ErrorCode);
        return messageBuilder.ToString();
      }
    }
  }
}
