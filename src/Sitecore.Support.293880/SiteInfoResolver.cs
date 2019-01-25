namespace Sitecore.Support.XA.Foundation.Multisite
{
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.Abstractions;
  using Sitecore.Data.Items;
  using Sitecore.DependencyInjection;
  using Sitecore.Web;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.XA.Foundation.Multisite.Extensions;
  using System.Web;

  public class SiteInfoResolver : Sitecore.XA.Foundation.Multisite.SiteInfoResolver
  {
    private IEnumerable<SiteInfo> _sites;

    public override IEnumerable<SiteInfo> Sites
    {
      get
      {
        BaseSiteContextFactory siteContextFactory = ServiceLocator.ServiceProvider.GetService<BaseSiteContextFactory>();
        return _sites ?? (_sites = from s in siteContextFactory.GetSites()
                                   orderby s.RootPath descending
                                   select s);
      }
    }
    
    public override SiteInfo GetSiteInfo(Item item)
    {
      if (item != null)
      {
        SiteInfo[] possibleSites = base.DiscoverPossibleSites(item);
        if (item.IsSxaSite())
        {
          possibleSites = possibleSites.Where(x => x.Properties.AllKeys.Contains("IsSxaSite")).ToArray();
        }
        else
        {
          possibleSites = possibleSites.Where(x => x.Properties.AllKeys.Contains("enablePreview")).ToArray();
        }
        if (possibleSites.Length <= 1)
        {
          return possibleSites.FirstOrDefault();
        }
        if (HttpContext.Current != null)
        {
          SiteInfo siteFromQuery = base.ResolveSiteFromQuery(possibleSites, HttpContext.Current.Request);
          if (siteFromQuery != null)
          {
            return siteFromQuery;
          }
        }
        SiteInfo fromSitecore = possibleSites.FirstOrDefault((SiteInfo s) => Context.Site != null && s.Name == Context.Site.Name);
        if (fromSitecore != null)
        {
          return fromSitecore;
        }
        if (HttpContext.Current != null)
        {
          SiteInfo siteInfo = base.ResolveSiteFromRequest(possibleSites, new HttpRequestWrapper(HttpContext.Current.Request));
          if (siteInfo != null)
          {
            return siteInfo;
          }
        }
        return possibleSites.FirstOrDefault((SiteInfo s) => base.LanguagesMatch(s, item)) ?? possibleSites.FirstOrDefault();
      }
      return null;
    }
  }
}