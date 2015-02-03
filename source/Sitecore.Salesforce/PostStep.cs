
namespace Sitecore.Salesforce
{
  using System.Collections.Specialized;

  using Sitecore.Install.Framework;
  using Sitecore.Integration.Common.Package.PostSteps;

  public class PostStep : IPostStep
  {
    public void Run(ITaskOutput output, NameValueCollection metaData)
    {
      new ImportTranslations("Salesforce Translations").Run(output, metaData);
    }
  }
}
