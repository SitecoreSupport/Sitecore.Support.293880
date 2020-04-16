namespace Sitecore.Support.XA.Foundation.Multisite.Services
{
    using Sitecore.Data.Items;
    using Sitecore.Sites;
    using Sitecore.XA.Foundation.Multisite.Enums;
    using Sitecore.XA.Foundation.Multisite.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CrossSiteLinkingService : Sitecore.XA.Foundation.Multisite.Services.CrossSiteLinkingService, ICrossSiteLinkingService
    {
        public new IEnumerable<Item> GetStartItems(Item item)
        {
            CrossSiteLinkingMode? siteLinkSettings = GetSiteLinkSettings(item);
            List<Item> list = new List<Item>();
            if (siteLinkSettings.HasValue)
            {
                switch (siteLinkSettings.GetValueOrDefault())
                {
                    case CrossSiteLinkingMode.ItselfOnly:
                        var homeItem = MultisiteContext.GetHomeItem(item);
                        if (homeItem != null)
                        {
                            list.Add(homeItem);
                        }
                        break;
                    case CrossSiteLinkingMode.LinkableSitesInTenant:
                        {
                            Item tenantItem = MultisiteContext.GetTenantItem(item);
                            {
                                foreach (SiteContext site in GetSites(item))
                                {
                                    string startPath = site.StartPath;
                                    if (startPath.StartsWith(tenantItem.Paths.Path, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Item item3 = item.Database.GetItem(startPath);
                                        if (item3.Paths.LongID.StartsWith(tenantItem.Paths.LongID, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (item3 != null)
                                            {
                                                list.Add(item3);
                                            }
                                        }
                                    }
                                }
                                return list;
                            }
                        }
                    case CrossSiteLinkingMode.AllLinkableSites:
                        {
                            foreach (SiteContext site2 in GetSites(item))
                            {
                                Item item2 = item.Database.GetItem(site2.StartPath);
                                if (item2 != null)
                                {
                                    list.Add(item2);
                                }
                            }
                            return list;
                        }
                }
            }
            return list;
        }
    }
}