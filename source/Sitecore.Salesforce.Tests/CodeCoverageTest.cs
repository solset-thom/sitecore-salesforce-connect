
namespace Sitecore.Salesforce.Tests
{
  using NUnit.Framework;

  using Sitecore.Salesforce.Diagnostics;

  [TestFixture]
  public class CodeCoverageTest
  {
    [Test]
    public void SomeTest2()
    {
      LogHelper.Warn("test", this);
    }
  }
}
