﻿namespace Sitecore.Salesforce.Client.Security
{
  public interface IAuthenticator
  {
    IAuthToken Authenticate();
  }
}