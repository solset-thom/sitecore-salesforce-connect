
namespace Sitecore.Salesforce.Caching
{
  using System;

  using Reflection;

  public class StringArrayCache : SalesforceCacheBase
  {
    protected string Separator { get; set; }

    public StringArrayCache(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.Separator = "|";
    }

    public StringArrayCache(string name, string maxSize, string slidingExpiration)
      : base(name, maxSize, slidingExpiration)
    {
      this.Separator = "|";
    }

    public void Add(string key, string[] values)
    {
      string value = string.Join(this.Separator, values);

      base.InnerCache.Add(key, value, TypeUtil.SizeOfString(value), this.SlidingExpiration);
    }

    public string[] Get(string key)
    {
      string str = base.InnerCache[key] as string;

      if (!string.IsNullOrEmpty(str))
      {
        return str.Split(this.Separator.ToCharArray());
      }

      return null;
    }

    public void Remove(string key)
    {
      base.InnerCache.Remove(key);
    }
  }
}
