// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactsApi.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the ContactApiHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Sitecore.Diagnostics;
  using Sitecore.Salesforce.Client;
  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Configuration;
  using Sitecore.Salesforce.Data;
  using Sitecore.Salesforce.Data.Profile;
  using Sitecore.Salesforce.Extensions;

  using Soql;

  public class ContactsApi : IContactsApi
  {
    protected readonly ISalesforceClient Client;

    protected readonly ISalesforceFieldMapping FieldMapping;

    protected readonly string FieldNamesString;

    public string ContactObjectName { get; set; }

    protected int MaxSalesforceOffset { get; set; }

    public ContactsApi(ISalesforceClient client, ISalesforceFieldMapping fieldMapping, IEnumerable<ISalesforceProfileProperty> properties)
    {
      Assert.ArgumentNotNull(client, "client");
      Assert.ArgumentNotNull(fieldMapping, "fieldMapping");
      Assert.ArgumentNotNull(properties, "properties");

      this.MaxSalesforceOffset = 2000;

      this.Client = client;
      this.FieldMapping = fieldMapping;

      var fields = new HashSet<string>(fieldMapping.GetAllFields());
      fields.UnionWith(properties.Select(i => i.SalesforceName));
      
      fields.Remove(null);
      fields.Remove(string.Empty);
      fields.Remove(fieldMapping.Password);
      fields.Remove(fieldMapping.PasswordAnswer);

      this.FieldNamesString = string.Join(",", fields);
    }

    public virtual List<ISalesforceContact> GetAll(int pageIndex, int pageSize)
    {
      var query = string.Format("SELECT {0} FROM {1} WHERE {2} !=''",this.FieldNamesString, this.ContactObjectName, this.FieldMapping.Login);

      return this.GetContactsWithPaging(query, pageIndex, pageSize);
    }

    public virtual List<ISalesforceContact> FindByFieldValue(string fieldName, object fieldValue, ComparisonOperator comparisonOperator, int pageIndex, int pageSize)
    {
      var query = string.Format("SELECT {0} FROM {1} WHERE {2} {3}", 
        this.FieldNamesString,
        this.ContactObjectName,
        fieldName,
        ComparisonOperatorParser.Parse(comparisonOperator, fieldValue.ToString()));

      return this.GetContactsWithPaging(query, pageIndex, pageSize);
    }

    public virtual bool CheckHashedFieldEquality(string userId, string fieldName, string value)
    {
      var query = string.Format("SELECT id FROM {0} WHERE Id = '{1}' AND {2} = '{3}'", this.ContactObjectName, userId, fieldName, value.GetSHA1Hash());

      var response = this.Client.Query<Dictionary<string, object>>(query);

      return response.Records.Count > 0;
    }

    public virtual int GetTotalCount()
    {
      return this.Client.Query<int>(string.Format("SELECT count() FROM {0} WHERE {1} != ''", this.ContactObjectName, this.FieldMapping.Login)).TotalSize;
    }
    
    public virtual ISalesforceContact Get(string loginName)
    {
      var query = string.Format(
        "SELECT {0} FROM {1} WHERE {3} {2}",
        this.FieldNamesString,
        this.ContactObjectName,
        ComparisonOperatorParser.Parse(ComparisonOperator.Equals, loginName),
        this.FieldMapping.Login);

      var response = this.Client.Query<Dictionary<string, object>>(query);

      var properties = response.Records.FirstOrDefault();

      return properties != null ? new SalesforceContact(this.FieldMapping, properties) : null;
    }

    public virtual ISalesforceContact Get(object providerUserKey)
    {
      var id = (string)providerUserKey;

      var query = string.Format("SELECT {0} FROM {1} WHERE Id {2}", this.FieldNamesString, this.ContactObjectName, ComparisonOperatorParser.Parse(ComparisonOperator.Equals, id));

      var response = this.Client.Query<Dictionary<string, object>>(query);

      var properties = response.Records.FirstOrDefault();

      return properties != null ? new SalesforceContact(this.FieldMapping, properties) : null;
    }

    public virtual OperationResult Create(Dictionary<string, object> contactProperties)
    {
      var result = this.Client.HttpPost<OperationResult>(string.Format("sobjects/{0}", this.ContactObjectName), contactProperties);

      return result;
    }

    public virtual bool Delete(string loginName)
    {
      var contact = this.Get(loginName);

      if (contact != null)
      {
        return this.Client.HttpDelete(string.Format("sobjects/{0}/{1}",this.ContactObjectName, contact.Id));
      }

      return false;
    }
    
    public virtual bool Update(ISalesforceContact contact)
    {
      var properties = new Dictionary<string, object>(contact.Properties.Count);

      foreach (var pair in contact.Properties)
      {
        properties[pair.Key] = pair.Value;
      }

      properties.Remove("Id");
      properties.Remove("Name");
      properties.Remove("CreatedDate");

      var result = this.Client.HttpPatch(string.Format("sobjects/{0}/{1}", this.ContactObjectName, contact.Id), properties);
      return result;
    }

    public virtual int GetTotalCountByFieldValue(string fieldName, object fieldValue, ComparisonOperator comparisonOperator)
    {
      string expression = ComparisonOperatorParser.Parse(comparisonOperator, fieldValue.ToString());
      return this.Client.Query<int>(string.Format("SELECT count() FROM {0} WHERE {1} {2}", this.ContactObjectName, fieldName, expression)).TotalSize;
    }

    public virtual List<ISalesforceContact> FindInactiveByLoginName(string loginName, ComparisonOperator comparisonOperatorByName, DateTime inactiveSinceDate, int pageIndex, int pageSize)
    {
      string nameExpression = ComparisonOperatorParser.Parse(comparisonOperatorByName, loginName);

      var query = string.Format(
        "SELECT {0} FROM {1} WHERE {5} <='{2}' AND {4} {3}",
        this.FieldNamesString,
        this.ContactObjectName,
        inactiveSinceDate,
        nameExpression,
        this.FieldMapping.Login,
        this.FieldMapping.LastActivityDate);

      return this.GetContactsWithPaging(query, pageIndex, pageSize);
    }

    public virtual int GetTotalInactiveByLoginName(string loginName, ComparisonOperator comparisonOperatorByName, DateTime inactiveSinceDate)
    {
      string expression = ComparisonOperatorParser.Parse(comparisonOperatorByName, loginName);
      return
        this.Client.Query<int>(
          string.Format(
            "SELECT count() FROM {0} WHERE {4} <='{1}' AND {3} {2}",
            this.ContactObjectName,
            inactiveSinceDate,
            expression,
            this.FieldMapping.Login,
            this.FieldMapping.LastActivityDate)).TotalSize;
    }

    protected virtual List<ISalesforceContact> GetContactsWithPaging(string query, int pageIndex, int pageSize)
    {
      IEnumerable<Dictionary<string, object>> result;

      int offset = pageIndex * pageSize;
      if (offset > this.MaxSalesforceOffset)
      {
        result = this.Client.QueryAllWithOffsetWorkaround<Dictionary<string, object>>(query, offset).Take(pageSize);
      }
      else
      {
        var tmpQuery = query + string.Format(" LIMIT {0} OFFSET {1}", pageSize, offset);
        result = this.Client.QueryAll<Dictionary<string, object>>(tmpQuery);
      }

      return new List<ISalesforceContact>(result.Select(record => new SalesforceContact(this.FieldMapping, record)));
    }
  }
}