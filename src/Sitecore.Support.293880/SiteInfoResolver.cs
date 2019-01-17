namespace Sitecore.Support.XA.Foundation.Multisite
{
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore;
  using Sitecore.Abstractions;
  using Sitecore.Data.Items;
  using Sitecore.DependencyInjection;
  using Sitecore.Web;
  using System.Collections.Generic;
  using Sitecore.XA.Foundation.Multisite.Extensions;
  using System.Linq;
  using System.Web;

  public class SiteInfoResolver : Sitecore.XA.Foundation.Multisite.SiteInfoResolver
  {
    private IEnumerable<SiteInfo> _sites;

    public override IEnumerable<SiteInfo> Sites
    {
      get
      {
        BaseSiteContextFactory service = ServiceLocator.ServiceProvider.GetService<BaseSiteContextFactory>();
        return _sites ?? (_sites = from s in service.GetSites()
                                   orderby s.RootPath descending
                                   select s);
      }
    }
    
    public override SiteInfo GetSiteInfo(Item item)
    {
      if (item != null)
      {
        SiteInfo[] array = DiscoverPossibleSites(item);
        if (item.IsSxaSite())
        {
          array = array.Where(x => x.Properties.AllKeys.Contains("IsSxaSite")).ToArray();
        }
        else
        {
          array = array.Where(x => x.Properties.AllKeys.Contains("enablePreview")).ToArray();
        }
        if (array.Length <= 1)
        {
          return array.FirstOrDefault();
        }
        if (HttpContext.Current != null)
        {
          SiteInfo siteInfo = ResolveSiteFromQuery(array, HttpContext.Current.Request);
          if (siteInfo != null)
          {
            return siteInfo;
          }
        }
        SiteInfo siteInfo2 = array.FirstOrDefault(delegate (SiteInfo s)
        {
          if (Context.Site != null)
          {
            return s.Name == Context.Site.Name;
          }
          return false;
        });
        if (siteInfo2 != null)
        {
          return siteInfo2;
        }
        if (HttpContext.Current != null)
        {
          SiteInfo siteInfo3 = ResolveSiteFromRequest(array, new HttpRequestWrapper(HttpContext.Current.Request));
          if (siteInfo3 != null)
          {
            return siteInfo3;
          }
        }
        return array.FirstOrDefault((SiteInfo s) => LanguagesMatch(s, item)) ?? array.FirstOrDefault();
      }
      return null;
    }
  }
}