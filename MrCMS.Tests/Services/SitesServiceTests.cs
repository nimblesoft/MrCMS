﻿using System;
using System.Linq;
using System.Web;
using FakeItEasy;
using FluentAssertions;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.People;
using MrCMS.Services;
using MrCMS.Website;
using NHibernate;
using Xunit;
using MrCMS.Helpers;

namespace MrCMS.Tests.Services
{
    public class SitesServiceTests : InMemoryDatabaseTest
    {
        private HttpRequestBase httpRequestBase;

        [Fact]
        public void SitesService_GetAllSites_CallsAndReturnsSessionQueryOverSitesToList()
        {
            var session = A.Fake<ISession>();
            var sitesService = GetSitesService(session);

            sitesService.GetAllSites();

            A.CallTo(() => session.QueryOver<Site>()).MustHaveHappened();
        }

        [Fact]
        public void SitesService_GetAllSites_ReturnsPersistedSites()
        {
            var sitesService = GetSitesService();
            Enumerable.Range(1, 10)
                      .ForEach(i => Session.Transact(sess => sess.SaveOrUpdate(new Site {Name = "Site " + i})));

            var allSites = sitesService.GetAllSites();

            allSites.Should().HaveCount(10);
        }

        [Fact]
        public void SitesService_AddSite_ShouldPersistSiteToSession()
        {
            var sitesService = GetSitesService();
            var user = new User();
            Session.Transact(session => session.Save(user));
            MrCMSApplication.OverriddenUser = user;
            var site = new Site();

            sitesService.AddSite(site);

            Session.QueryOver<Site>().RowCount().Should().Be(1);
        }

        [Fact]
        public void SitesService_AddSite_CallsSessionSaveOnPassedSite()
        {
            var session = A.Fake<ISession>();
            var sitesService = GetSitesService(session);
            var site = new Site();

            sitesService.AddSite(site);

            A.CallTo(() => session.Save(site)).MustHaveHappened();
        }

        [Fact]
        public void SitesService_AddSite_ShouldAddCurrentUserToSiteUsers()
        {
            var sitesService = GetSitesService();
            var user = new User();
            Session.Transact(session => session.Save(user));
            MrCMSApplication.OverriddenUser = user;
            var site = new Site();

            sitesService.AddSite(site);

            site.Users.Should().Contain(user);
        }

        [Fact]
        public void SitesService_AddSite_ShouldAddSiteToCurrentUsersSites()
        {
            var sitesService = GetSitesService();
            var user = new User();
            Session.Transact(session => session.Save(user));
            MrCMSApplication.OverriddenUser = user;
            var site = new Site();

            sitesService.AddSite(site);

            user.Sites.Should().Contain(site);
        }

        [Fact]
        public void SitesService_SaveSite_CallsSessionUpdateOnPassedSite()
        {
            var session = A.Fake<ISession>();
            var sitesService = GetSitesService(session);
            var site = new Site();

            sitesService.SaveSite(site);

            A.CallTo(() => session.Update(site)).MustHaveHappened();
        }

        [Fact]
        public void SitesService_DeleteSite_ShouldCallDeleteOnPassedSite()
        {
            var session = A.Fake<ISession>();
            var sitesService = GetSitesService(session);
            var site = new Site();

            sitesService.DeleteSite(site);

            A.CallTo(() => session.Delete(site)).MustHaveHappened();
        }

        [Fact]
        public void SitesService_DeleteSite_ShouldRemoveSiteFromSession()
        {
            var sitesService = GetSitesService();
            var site = new Site();
            Session.Transact(session => session.Save(site));
            Session.QueryOver<Site>().RowCount().Should().Be(1);

            sitesService.DeleteSite(site);

            Session.QueryOver<Site>().RowCount().Should().Be(0);
        }

        [Fact]
        public void SitesService_GetSite_CallsSessionGetWithPassedId()
        {
            var session = A.Fake<ISession>();
            var sitesService = GetSitesService(session);

            sitesService.GetSite(1);

            A.CallTo(() => session.Get<Site>(1)).MustHaveHappened();
        }

        [Fact]
        public void SitesService_GetSite_ReturnsResultFromSessionGetAsResult()
        {
            var session = A.Fake<ISession>();
            var site = new Site();
            A.CallTo(() => session.Get<Site>(1)).Returns(site);
            var sitesService = GetSitesService(session);

            sitesService.GetSite(1).Should().Be(site);
        }

        [Fact]
        public void SitesService_GetCurrentSite_ByDefaultReturnsFirstSite()
        {
            var site1 = new Site { BaseUrl = "test1" };
            var site2 = new Site { BaseUrl = "test2" };
            Session.Transact(session =>
            {
                session.Save(site1);
                session.Save(site2);
            });
            var sitesService = GetSitesService();
            A.CallTo(() => httpRequestBase.Url).Returns(new Uri("http://www.example.com/"));

            sitesService.GetCurrentSite().Should().Be(site1);
        }

        [Fact]
        public void SitesService_GetCurrentSite_IfUrlMatchesReturnsMatchingSite()
        {
            var site1 = new Site { BaseUrl = "test1" };
            var site2 = new Site { BaseUrl = "http://www.example.com" };
            Session.Transact(session =>
            {
                session.Save(site1);
                session.Save(site2);
            });
            var sitesService = GetSitesService();
            A.CallTo(() => httpRequestBase.Url).Returns(new Uri("http://www.example.com/"));

            sitesService.GetCurrentSite().Should().Be(site2);
        }
        
        private SiteService GetSitesService(ISession session = null)
        {
            httpRequestBase = A.Fake<HttpRequestBase>();
            return new SiteService(session ?? Session, httpRequestBase);
        }
    }
}