namespace Sitecore.Salesforce.Caching
{
  using System;

  using Sitecore.Caching;

  public abstract class SalesforceCacheBase : CustomCache
  {
    protected readonly TimeSpan SlidingExpiration;

    protected SalesforceCacheBase(string name, long maxSize)
      : base(name, maxSize)
    {
    }

    protected SalesforceCacheBase(string name, long maxSize, TimeSpan slidingExpiration)
      : base(name, maxSize)
    {
      this.SlidingExpiration = slidingExpiration;
    }

    protected SalesforceCacheBase(string name, string maxSize)
      : this(name, StringUtil.ParseSizeString(maxSize), TimeSpan.Zero)
    {
    }

    protected SalesforceCacheBase(string name, string maxSize, string slidingExpiration)
      : this(name, StringUtil.ParseSizeString(maxSize), TimeSpan.Parse(slidingExpiration))
    {
    }
  }
}