namespace Sitecore.Salesforce.Client.Data.Errors
{
  public interface IErrorResponse
  {
    ErrorCode ErrorCode { get; }

    string Message { get;}
  }
}