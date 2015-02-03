// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesApi.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the RolesApi type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Api
{
  using System.Collections.Generic;
  using System.Linq;
  using Client.Data;

  using Newtonsoft.Json.Linq;

  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Data;
  using Soql;

  public class RolesApi : IRolesApi
  {
    protected static readonly SalesforceRoleNameEqualityComparer NameEqualityComparer = new SalesforceRoleNameEqualityComparer();

    protected readonly ISalesforceClient Client;

    protected readonly ISalesforceFieldMapping FieldMapping;

    public RolesApi(ISalesforceClient client, ISalesforceFieldMapping fieldMapping)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNull(fieldMapping, "fieldMapping");

      this.Client = client;
      this.FieldMapping = fieldMapping;
    }

    public string RoleObjectName { get; set; }

    public string RoleAssociationObjectName { get; set; }

    public string RelatedContactFieldName { get; set; }

    public string RelatedSitecoreRoleFieldName { get; set; }

    public virtual List<SalesforceRole> GetRolesForUser(string loginName)
    {
      Assert.ArgumentNotNullOrEmpty(loginName, "loginName");

      var query = string.Format(
        "SELECT {0}__r.Id, {0}__r.Name FROM {1}__c WHERE {2}__r.{3} {4}",
        this.RelatedSitecoreRoleFieldName,
        this.RoleAssociationObjectName,
        this.RelatedContactFieldName,
        this.FieldMapping.Login,
        ComparisonOperatorParser.Parse(ComparisonOperator.Equals, loginName));

      return this.GetRolesByQuery(query);
    }

    public virtual List<SalesforceRole> GetAllRoles()
    {
      var query = string.Format("SELECT Id, Name FROM {0}__c", this.RoleObjectName);

      return new List<SalesforceRole>(this.Client.QueryAll<SalesforceRole>(query));
    }

    public virtual SalesforceRole Get(string roleName)
    {
      var query = string.Format("SELECT Id, Name FROM {0}__c WHERE Name {1}", this.RoleObjectName, ComparisonOperatorParser.Parse(ComparisonOperator.Equals, roleName));

      return this.Client.QueryAll<SalesforceRole>(query).FirstOrDefault();
    }

    public virtual List<string> GetUsersInRole(string roleName)
    {
      Assert.ArgumentNotNullOrEmpty(roleName, "roleName");

      string expression = ComparisonOperatorParser.Parse(ComparisonOperator.Equals, roleName);
      
      var query = string.Format(
        "SELECT {0}__r.{4} FROM {1}__c WHERE {2}__r.Name {3}",
        this.RelatedContactFieldName,
        this.RoleAssociationObjectName,
        this.RelatedSitecoreRoleFieldName,
        expression,
        this.FieldMapping.Login);

      return this.GetUsersByQuery(query);
    }

    public virtual List<string> FindUsersInRole(string roleName, string usernameToMatch)
    {
      Assert.ArgumentNotNullOrEmpty(roleName, "roleName");
      Assert.ArgumentNotNullOrEmpty(usernameToMatch, "usernameToMatch");

      var query = string.Format(
        "SELECT {0}__r.{5} FROM {1}__c WHERE {2}__r.Name = '{3}' AND {0}__r.{5} LIKE '%{4}%'",
        this.RelatedContactFieldName,
        this.RoleAssociationObjectName,
        this.RelatedSitecoreRoleFieldName,
        roleName,
        usernameToMatch,
        this.FieldMapping.Login);

      return this.GetUsersByQuery(query);
    }
    
    public virtual OperationResult Create(string roleName)
    {
      Assert.ArgumentNotNullOrEmpty(roleName, "roleName");
      var result = this.Client.HttpPost<OperationResult>(
        string.Format("sobjects/{0}__c", this.RoleObjectName), 
        new Dictionary<string, string> { { "Name", roleName } });

      return result;
    }

    public virtual bool DeleteById(string roleId)
    {
      Assert.ArgumentNotNullOrEmpty(roleId, "roleId");

      return this.Client.HttpDelete(string.Format("sobjects/{0}__c/{1}", this.RoleObjectName, roleId));
    }

    public virtual OperationResult AddUserToRole(string roleId, string userId)
    {
      Assert.ArgumentNotNullOrEmpty(roleId, "roleId");
      Assert.ArgumentNotNullOrEmpty(userId, "userId");

      var data = new Dictionary<string, string>()
      {
        { string.Format("{0}__c", this.RoleObjectName), roleId },
        { string.Format("{0}__c", this.RelatedContactFieldName), userId }
      };

      var result = this.Client.HttpPost<OperationResult>(string.Format("sobjects/{0}__c", this.RoleAssociationObjectName), data);

      return result;
    }

    public virtual bool DeleteUserFromRole(string roleName, string loginName)
    {
      var roleAssociation = this.Client.QueryAll<RoleAssociation>(string.Format(
        "SELECT Id FROM {0}__c WHERE {1}__r.{5} {2} AND {3}__r.Name {4}",
        this.RoleAssociationObjectName,
        this.RelatedContactFieldName,
        ComparisonOperatorParser.Parse(ComparisonOperator.Equals, loginName),
        this.RelatedSitecoreRoleFieldName,
        ComparisonOperatorParser.Parse(ComparisonOperator.Equals, roleName),
        this.FieldMapping.Login)).FirstOrDefault();

      return roleAssociation != null && this.Client.HttpDelete(string.Format("sobjects/{0}__c/{1}", this.RoleAssociationObjectName, roleAssociation.Id));
    }


    protected virtual List<SalesforceRole> GetRolesByQuery(string query)
    {
      Assert.ArgumentNotNullOrEmpty(query, "query");

      var result = this.Client.QueryAll<JToken>(query);

      ISet<SalesforceRole> roles = new HashSet<SalesforceRole>(NameEqualityComparer);

      foreach (var role in this.ParseFromRef<SalesforceRole>(result, this.RoleObjectName))
      {
        if (role.IsValid())
        {
          roles.Add(role);
        }
      }

      return new List<SalesforceRole>(roles);
    }

    protected virtual List<string> GetUsersByQuery(string query)
    {
      Assert.ArgumentNotNullOrEmpty(query, "query");

      var result = this.Client.QueryAll<JToken>(query);

      ISet<string> users = new HashSet<string>();

      foreach (var userProperties in this.ParseFromRef<Dictionary<string, object>>(result, this.RelatedContactFieldName))
      {
        object tmp;
        userProperties.TryGetValue(this.FieldMapping.Login, out tmp);
        
        string login = tmp as string;
        if (!string.IsNullOrEmpty(login))
        {
          users.Add(login);
        }
      }

      return new List<string>(users);
    }

    protected virtual IEnumerable<T> ParseFromRef<T>(IEnumerable<JToken> tokens, string objectName)
      where T : class
    {
      Assert.ArgumentNotNull(tokens, "tokens");

      foreach (JToken token in tokens)
      {
        var roleToken = token[objectName + "__r"];
        if (roleToken != null)
        {
          var obj = roleToken.ToObject<T>();
          if (obj != null)
          {
            yield return obj;
          }
        }
      }
    }

    public class SalesforceRoleNameEqualityComparer : IEqualityComparer<SalesforceRole>
    {
      public bool Equals(SalesforceRole x, SalesforceRole y)
      {
        if (ReferenceEquals(x, y))
        {
          return true;
        }

        if (x == null)
        {
          return y == null;
        }

        return y != null && x.Name == y.Name;
      }

      public int GetHashCode(SalesforceRole obj)
      {
        return obj.Name.GetHashCode();
      }
    }
  }
}