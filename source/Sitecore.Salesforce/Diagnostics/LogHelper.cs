// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogHelper.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Log Helper
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Diagnostics
{
  using System;
  using Sitecore.Diagnostics;
  using Web.UI.Sheer;

  /// <summary>
  /// Log Helper
  /// </summary>
  public static class LogHelper
  {
    /// <summary>
    /// Message Prefix
    /// </summary>
    private const string MessagePrefix = "Sitecore.Salesforce *** ";

    /// <summary>
    /// Error message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="owner">
    /// The owner.
    /// </param>
    /// <param name="exception">
    /// The exception.
    /// </param>
    public static void Error(string message, object owner, Exception exception = null)
    {
      Log.Error(GetMessage(message, owner, exception), exception, owner);
    }

    /// <summary>
    /// Warn message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="owner">
    /// The owner.
    /// </param>
    /// <param name="exception">
    /// The exception.
    /// </param>
    public static void Warn(string message, object owner, Exception exception = null)
    {
      Log.Warn(GetMessage(message, owner, exception), exception, owner);
    }

    /// <summary>
    /// Info message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="owner">
    /// The owner.
    /// </param>
    public static void Info(string message, object owner)
    {
      Log.Info(GetMessage(message, owner), owner);
    }

    /// <summary>
    /// Debug message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    public static void Debug(string message)
    {
      Log.Debug(GetMessage(message, null));
    }

    /// <summary>
    /// Debug message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="owner">
    /// The owner.
    /// </param>
    public static void Debug(string message, object owner)
    {
      Log.Debug(GetMessage(message, owner), owner);
    }

    /// <summary>
    /// Get message
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="owner">
    /// The owner.
    /// </param>
    /// <param name="exception">
    /// The exception.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    private static string GetMessage(string message, object owner, Exception exception = null)
    {
      string str = MessagePrefix + message;

      if (exception == null && owner != null)
      {
        str += " (Called by: " + owner.GetType().Name + ")";
      }

      return str;
    }

    public static void ShowMessage(string message, object owner, string logMessage = null, Exception exception = null)
    {
      try
      {
        if (SheerResponse.OutputEnabled)
        {
          SheerResponse.Alert(message, new string[0]);
        }
        Error(logMessage ?? message, owner, exception);
      }
      catch
      {
        Error(logMessage ?? message, owner, exception);
      }
    }
  }
}