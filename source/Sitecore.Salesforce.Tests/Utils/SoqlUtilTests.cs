namespace Sitecore.Salesforce.Tests.Utils
{
  using NUnit.Framework;

  [TestFixture]
  public class SoqlUtilTests
  {
    [TestCase(@"my\email&@test.com", Result = @"my\\email&@test.com")]
    [TestCase(@"my'email\$@test.com", Result = @"my\'email\\$@test.com")]
    [TestCase("my \"emai", Result = "my \\\"emai")]
    [TestCase(null, Result = null)]
    [TestCase(@"", Result = "")]
    [TestCase(@"mail@test.com", Result = @"mail@test.com")]
    public string EscapeTest(string value)
    {
      return Salesforce.Utils.SoqlUtil.Escape(value);
    }
  }
}
