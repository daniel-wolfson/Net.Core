using Crpm.Controllers;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Core;
using Crpm.Model.Data;
using Crpm.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Crpm.Dal.UnitTest
{
    public class OrganizationControllerTest : TestWebHost
    {
        private OrganizationService _orgService;
        private readonly OrganizationController _controller;

        public OrganizationControllerTest()
        {
            _orgService = base.GetRootService<OrganizationService>();
            _controller = new OrganizationController(_orgService);
        }

        [Fact]
        public async void GetOrganizationTree()
        {
            var expected = new OrganizationObjectData();
            var result = await _orgService.GetOrganizationTree();
            Assert.NotNull(result);
            Assert.NotEqual(result, expected);
        }

        [Fact(Skip ="method not used")]
        public async void GetUnitByGuid()
        {
            Unit expected = null;
            string UnitGuid = "aaaabbbbccccddddeeeeffffgggghhhh";
            var result = await _orgService.GetUnitByGuid(UnitGuid);
            Assert.NotNull(result);
            Assert.NotEqual(result, expected);
        }

        [Theory]
        [InlineData("aaaabbbbccccddddeeeeffffgggghhhh")]
        public async void SaveOrganizationObject(string parent_guid)
        {
            OrganizationObjectData data = new OrganizationObjectData() { guid = null, name = "org_for_test"+ new Random().Next(int.MinValue, int.MaxValue).ToString() };
            string result = await _orgService.SaveOrganizationObject(parent_guid, data);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("aaaabbbbccccddddeeeeffffgggghhhh")]
        public async void DeleteOrganizationOrObject(string parent_guid)
        {
            SaveOrganizationObject(parent_guid);

            List<string> organization_guid_list = _orgService.DbContext.OrganizationObject
                .Where(x => x.Name.Contains("org_test_")).Select(x => x.OrgObjGuid).ToList();

            bool result = await _orgService.DeleteOrganizationOrObject(organization_guid_list);
            Assert.True(result);
        }

        [Fact(Skip ="complex test")]
        public async void DragAndDrop()
        {
            string dest_org_guid = "";
            string drag_org_guid = "";
            List<string> org_children_guid_list = new List<string>();
            bool result = await _orgService.DragAndDrop(dest_org_guid, drag_org_guid, org_children_guid_list);
            Assert.True(result);
        }

        [Fact(Skip = "complex test")]
        public async void ApplyProperties()
        {
            var obj = new OrganizationObjectData(); var guid_list = new List<string>() { "1", "2" };
            bool result = await _orgService.ApplyProperties(obj, guid_list);
            Assert.True(result);
        }

        [Fact]
        public async void DuplicateOrganizationObject()
        {
            var expectedStatusCode = (int)HttpStatusCode.OK;
            var actualObject = new OrganizationObjectData()
            {
                activities = new List<ActivityTemplateDataInfo>() { new ActivityTemplateDataInfo() { name = "Test", description = "test" } },
                children = new List<OrganizationObjectData>(),
                descriptions = new List<DescriptionsData>()
            };
            var isRec = true;
            var result = await _controller.UpdateDuplicateOrganizationObject(actualObject, isRec) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, expectedStatusCode);
        }

        [Fact(Skip = "complex test")]
        public async void SaveFullTreeOrganization()
        {
            string parent_guid = "";
            OrganizationObjectData data = new OrganizationObjectData();
            string description_type = "";
            List<Task> tasksList = new List<Task>();
            OrganizationObjectData result = await _orgService.SaveFullTreeOrganizationObject(parent_guid, data, description_type, tasksList);
            Assert.NotNull(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    using var tempServiceScope = GeneralContext.CreateServiceScope();
                    //var userService = GeneralContext.GetService<UserService>();
                    var dbContext = tempServiceScope.ServiceProvider.GetService<CRPMContext>();
                    var results = dbContext.OrganizationObject.Where(x => x.Name.Contains("only_for_test"));
                    if (results != null && results.Any())
                        dbContext.OrganizationObject.RemoveRange(results);
                    dbContext.SaveChanges();

                    base.Dispose(disposing);
                }
                disposedValue = true;
            }
        }
    }
}
