// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContactsApi.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the IContactApiHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Api
{
  using System.Collections.Generic;

  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Data;
  using Sitecore.Salesforce.Soql;

  public interface IContactsApi
  {
    List<ISalesforceContact> GetAll(int pageIndex, int pageSize);

    List<ISalesforceContact> FindInactiveByLoginName(string loginName, ComparisonOperator comparisonOperatorByName, System.DateTime inactiveSinceDate, int pageIndex, int pageSize);

    List<ISalesforceContact> FindByFieldValue(string fieldName, object fieldValue, ComparisonOperator comparisonOperator, int pageIndex, int pageSize);

    bool CheckHashedFieldEquality(string userId, string fieldName, string value);

    int GetTotalCount();

    int GetTotalCountByFieldValue(string fieldName, object fieldValue, ComparisonOperator comparisonOperator);

    int GetTotalInactiveByLoginName(string loginName, ComparisonOperator comparisonOperatorByName, System.DateTime inactiveSinceDate);
    
    ISalesforceContact Get(string loginName);

    ISalesforceContact Get(object providerUserKey);

    OperationResult Create(Dictionary<string, object> contact);

    bool Delete(string loginName);

    bool Update(ISalesforceContact contact);
  }
}