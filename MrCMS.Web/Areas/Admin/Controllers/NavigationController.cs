﻿using System;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.People;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using NHibernate;
using MrCMS.Helpers;

namespace MrCMS.Web.Areas.Admin.Controllers
{
    public class NavigationController : SystemController
    {
        private readonly INavigationService _service;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;

        public NavigationController(INavigationService service, IUserService userService, ISiteService siteService)
        {
            _service = service;
            _userService = userService;
            _siteService = siteService;
        }

        public PartialViewResult WebSiteTree()
        {
            return PartialView("WebsiteTreeList", _service.GetWebsiteTree());
        }

        public PartialViewResult MediaTree()
        {
            return PartialView("MediaTree", _service.GetMediaTree());
        }

        public PartialViewResult LayoutTree()
        {
            return PartialView("LayoutTree", _service.GetLayoutList());
        }

        public PartialViewResult UserList()
        {
            return PartialView("UserList", _service.GetUserList());
        }

        public PartialViewResult NavLinks()
        {
            var items = TypeHelper.GetAllConcreteTypesAssignableFrom<IAdminMenuItem>().Select(Activator.CreateInstance).Cast<IAdminMenuItem>().OrderBy(item => item.DisplayOrder);
            return PartialView("NavLinks", items);
        }

        [ChildActionOnly]
        public PartialViewResult LoggedInAs()
        {
            User user = _userService.GetCurrentUser(HttpContext);
            return PartialView(user);
        }

        public ActionResult SiteList()
        {
            var allSites = _siteService.GetAllSites();

            if (allSites.Count == 1)
                return new EmptyResult();

            return PartialView(allSites.BuildSelectItemList(site => site.Name, site => string.Format("http://{0}/admin/", site.BaseUrl),
                                                            site => _siteService.GetCurrentSite() == site,
                                                            emptyItemText: null));
        }
    }
}