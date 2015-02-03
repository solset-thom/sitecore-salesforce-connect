// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the StringExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Extensions
{
  using System;
  using System.Security.Cryptography;
  using System.Text;

  public static class StringExtensions
  {
    public static string GetSHA1Hash(this string str)
    {
      var managed = new SHA1Managed();
      var bytes = Encoding.UTF8.GetBytes(str);
      return Convert.ToBase64String(managed.ComputeHash(bytes));
    }
  }
}