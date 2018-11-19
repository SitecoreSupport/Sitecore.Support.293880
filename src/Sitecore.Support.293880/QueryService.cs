using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.StringExtensions;
using Sitecore.XA.Foundation.Abstractions;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore.XA.Foundation.SitecoreExtensions.Services;
using Sitecore.XA.Foundation.TokenResolution;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Support.XA.Foundation.SitecoreExtensions.Services
{
  public class QueryService: IQueryService
  {
    protected IContext Context
    {
      get;
    } = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<IContext>();


    public string Resolve(string query, string itemId)
    {
      Item item2 = Context.ContentDatabase.Items[itemId];
      #region fix
      System.Collections.Specialized.NameValueCollection scFormCollection =
        this.Context.Items["SC_FORM"] as System.Collections.Specialized.NameValueCollection;
      if (scFormCollection != null)
      {
        string currentCELanguage = scFormCollection["scLanguage"];
        if (currentCELanguage != "")
        {
          item2 = item2.Database.GetItem(item2.ID, Sitecore.Globalization.Language.Parse(currentCELanguage));
        }
      }
      #endregion
      if (item2 != null)
      {
        query = TokenResolver.Resolve(query, item2);
        if (query.IsNullOrEmpty())
        {
          return string.Empty;
        }
        Item[] array = item2.SelectItemsWithLanguage(query);
        if (array != null)
        {
          return string.Join("|", from item in array
            select item.Paths.FullPath);
        }
      }
      return string.Empty;
    }
  }
}