// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimitsAgentTests.cs" company="Sitecore A/S">
//   Copyright (C) 2014 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the LimitsAgentTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Salesforce.Tests.Tasks
{
  using System.Diagnostics.CodeAnalysis;
  using System.Reflection;

  using Moq;

  using NUnit.Framework;

  using Sitecore.Salesforce.Client.Data;
  using Sitecore.Salesforce.Tasks;

  [TestFixture]
  public class LimitsAgentTest
  {
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1123:DoNotPlaceRegionsWithinElements", Justification = "Reviewed. Suppression is OK here.")]
    private object[] processLimitsTestCase =
      {
        #region Test Case 1
        new object[] // Test Case 1
          {
            "salesforce", // Virtual Configuration Name
            new SLimits   // Salesforce limits
              {
                DailyApiRequests = new SLimits.LimitData { Remaining = 900, Max = 1000 },
                DataStorageMb = new SLimits.LimitData { Remaining = 5, Max = 5 }
              },

            true, // <logStatistic>
            5,    // <apiUsageThreshold>
            0,    // <dataStorageThreshold>

            // Log Results
            new[] // Count of Limits Agent log messages.
              { 
                Times.Once(),   // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
                Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
                Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
                Times.Never(),  // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
                Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
                Times.Never()   // Log ERROR: Salesforce account _ reached its _MB data limit!
              }
          },
        #endregion

        #region Test Case 2
        new object[] // Test Case 2
          {
            "salesforce", // Virtual Configuration Name
            new SLimits   // Salesforce limits
              {
                DailyApiRequests = new SLimits.LimitData { Remaining = 100, Max = 15000 },
                DataStorageMb = new SLimits.LimitData { Remaining = 5, Max = 5 }
              },

            false, // <logStatistic>
            90,    // <apiUsageThreshold>
            90,    // <dataStorageThreshold>

            // Log Results
            new[] // Count of Limits Agent log messages.
              { 
                Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
                Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
                Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
                Times.Never(),  // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
                Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
                Times.Never()   // Log ERROR: Salesforce account _ reached its _MB data limit!
              }
          },
          #endregion

        #region Test Case 3
        new object[] // Test Case 3
        {
          "salesforce", // Virtual Configuration Name
          new SLimits   // Salesforce limits
            {
              DailyApiRequests = new SLimits.LimitData { Remaining = 0, Max = 15000 },
              DataStorageMb = new SLimits.LimitData { Remaining = 5, Max = 5 }
            },

          false, // <logStatistic>
          90,    // <apiUsageThreshold>
          90,    // <dataStorageThreshold>

          // Log Results
          new[] // Count of Limits Agent log messages.
            { 
              Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
              Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
              Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
              Times.Once(),   // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
              Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
              Times.Never()   // Log ERROR: Salesforce account _ reached its _MB data limit!
            }
        },
        #endregion

        #region Test Case 4
        new object[] // Test Case 4
        {
          "salesforce", // Virtual Configuration Name
          new SLimits   // Salesforce limits
            {
              DailyApiRequests = new SLimits.LimitData { Remaining = 10000, Max = 15000 },
              DataStorageMb = new SLimits.LimitData { Remaining = 0, Max = 5 }
            },

          false, // <logStatistic>
          90,    // <apiUsageThreshold>
          90,    // <dataStorageThreshold>

          // Log Results
          new[] // Count of Limits Agent log messages.
            { 
              Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
              Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
              Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
              Times.Never(),  // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
              Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
              Times.Once()    // Log ERROR: Salesforce account _ reached its _MB data limit!
            }
        },
        #endregion

        #region Test Case 5
        new object[] // Test Case 5
        {
          "salesforce", // Virtual Configuration Name
          new SLimits   // Salesforce limits
            {
              DailyApiRequests = new SLimits.LimitData { Remaining = 10000, Max = 15000 },
              DataStorageMb = new SLimits.LimitData { Remaining = 1, Max = 5 }
            },

          false, // <logStatistic>
          90,    // <apiUsageThreshold>
          80,    // <dataStorageThreshold>

          // Log Results
          new[] // Count of Limits Agent log messages.
            { 
              Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
              Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
              Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
              Times.Never(),  // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
              Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
              Times.Never()   // Log ERROR: Salesforce account _ reached its _MB data limit!
            }
        },
        #endregion

        #region Test Case 6
        new object[] // Test Case 6
        {
          "salesforce", // Virtual Configuration Name
          null,         // Salesforce limits

          true,         // <logStatistic>
          90,           // <apiUsageThreshold>
          80,           // <dataStorageThreshold>

          // Log Results
          new[] // Count of Limits Agent log messages.
            { 
              Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
              Times.Once(),   // Log WARN: Account usage limits: Limits information are not available! Configuration: _
              Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
              Times.Never(),  // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
              Times.Never(),  // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
              Times.Never()   // Log ERROR: Salesforce account _ reached its _MB data limit!
            }
        },
        #endregion

        #region Test Case 7
        new object[] // Test Case 7
          {
            "salesforce", // Virtual Configuration Name
            new SLimits   // Salesforce limits
              {
                DailyApiRequests = new SLimits.LimitData { Remaining = 0, Max = 1000 },
                DataStorageMb = new SLimits.LimitData { Remaining = 0, Max = 5 }
              },

            false, // <logStatistic>
            90,    // <apiUsageThreshold>
            90,    // <dataStorageThreshold>

            // Log Results
            new[] // Count of Limits Agent log messages.
              { 
                Times.Never(),  // Log INFO: Account usage limits for Salesforce configuration _: API daily usage: _ calls of _ ...
                Times.Never(),  // Log WARN: Account usage limits: Limits information are not available! Configuration: _
                Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed API daily calls (_ of _).
                Times.Once(),   // Log ERROR: Salesforce account _ has reached its _ API daily call limit!
                Times.Once(),   // Log WARN: Salesforce account _ has used _% of its allowed data (_MB of _MB).
                Times.Once()    // Log ERROR: Salesforce account _ reached its _MB data limit!
              }
          },
        #endregion
      };

    [Test]
    [TestCaseSource("processLimitsTestCase")]
    public void ProcessLimitsTest(
      string configurationName,
      SLimits limits, 
      bool logStatistic, 
      int apiUsageThreshold, 
      int dataStorageThreshold,
      Times[] result)
    {
      var mockedApiDailyRequestsLoger = new Moq.Mock<ILimitLoger>();
      if (limits != null)
      {
        var apiDailyRequestsLoger = new ApiDailyRequestsLoger(configurationName, limits.DailyApiRequests);
        mockedApiDailyRequestsLoger.SetupGet(p => p.UsedAmount).Returns(apiDailyRequestsLoger.UsedAmount);
        mockedApiDailyRequestsLoger.SetupGet(p => p.UsedPercents).Returns(apiDailyRequestsLoger.UsedPercents);
      }

      mockedApiDailyRequestsLoger.Setup(m => m.LogPointWarning()).Verifiable();
      mockedApiDailyRequestsLoger.Setup(m => m.LogHasReachedError()).Verifiable();


      var mockedDataStorageLoger = new Moq.Mock<ILimitLoger>();
      if (limits != null)
      {
        var dataStorageLoger = new ApiDailyRequestsLoger(configurationName, limits.DataStorageMb);
        mockedDataStorageLoger.SetupGet(p => p.UsedAmount).Returns(dataStorageLoger.UsedAmount);
        mockedDataStorageLoger.SetupGet(p => p.UsedPercents).Returns(dataStorageLoger.UsedPercents);
      }

      mockedDataStorageLoger.Setup(m => m.LogPointWarning()).Verifiable();
      mockedDataStorageLoger.Setup(m => m.LogHasReachedError()).Verifiable();

      
      var mockedLoger = new Moq.Mock<ILimitsLoger>();
      mockedLoger.SetupGet(p => p.ApiDailyRequestsLoger).Returns(mockedApiDailyRequestsLoger.Object);
      mockedLoger.SetupGet(p => p.DataStorageLoger).Returns(mockedDataStorageLoger.Object);
      mockedLoger.Setup(m => m.LogStatisticInfo()).Verifiable();
      mockedLoger.Setup(m => m.LogInformationAreNotAvailableWarning()).Verifiable();

      var agent = new LimitsAgent();
      agent.LogStatistic = logStatistic;
      agent.ApiUsageThreshold = apiUsageThreshold;
      agent.DataStorageThreshold = dataStorageThreshold;
      var logerField = typeof(LimitsAgent).GetField("loger", BindingFlags.Instance | BindingFlags.NonPublic);
      if (logerField != null)
      {
        logerField.SetValue(agent, mockedLoger.Object);
      }

      agent.ProcessLimits(limits);

      mockedLoger.Verify(m => m.LogStatisticInfo(), result[0]);
      mockedLoger.Verify(m => m.LogInformationAreNotAvailableWarning(), result[1]);
      mockedApiDailyRequestsLoger.Verify(m => m.LogPointWarning(), result[2]);
      mockedApiDailyRequestsLoger.Verify(m => m.LogHasReachedError(), result[3]);
      mockedDataStorageLoger.Verify(m => m.LogPointWarning(), result[4]);
      mockedDataStorageLoger.Verify(m => m.LogHasReachedError(), result[5]);
    }
  }
}