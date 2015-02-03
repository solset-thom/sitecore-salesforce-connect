namespace Sitecore.Salesforce.Data
{
  using Newtonsoft.Json;
  using Reflection;
  using Sitecore.Caching;
  using Sitecore.Salesforce.Client.Data;

  public class SalesforceRole : SObject, ICacheable
  {
    [JsonProperty("Id")]
    public string Id { get; protected set; }

    [JsonProperty("Name")]
    public string Name { get; protected set; }

    public SalesforceRole()
    {
      this.Cacheable = true;
      this.Immutable = false;
    }

    public SalesforceRole(string name) : this()
    {
      this.Name = name;
    }

    public virtual bool IsValid()
    {
      return !string.IsNullOrEmpty(this.Id) && !string.IsNullOrEmpty(this.Name);
    }

    public long GetDataLength()
    {
      return TypeUtil.SizeOfString(this.Id) + TypeUtil.SizeOfString(this.Name);
    }

    public bool Cacheable { get; set; }
    public bool Immutable { get; private set; }
    public event DataLengthChangedDelegate DataLengthChanged;
  }
}