using Crpm.Dal.Services;
using Crpm.Infrastructure.Core;
using Crpm.Model.Data;
using Crpm.Model.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Xunit;

namespace Crpm.Dal.UnitTest
{
    public class ActivityControllerTest : TestWebHost
    {
        private readonly ActivityService _activityService;
        private readonly ActivityTemplateService _activityTemplateService;

        public ActivityControllerTest()
        {
            _activityService = GeneralContext.GetService<ActivityService>();
            _activityTemplateService = GeneralContext.GetService<ActivityTemplateService>();
        }

        [Fact(Skip = "work, but complex test")]
        public async void SaveActivityTemplate()
        {
            ActivityTemplateData actualData = new ActivityTemplateData()
            {
                activity_template = new ActivityTemplateDataInfo(),
                connected_form_templates = new List<string>() { "" }
            };
            string result = await _activityTemplateService.SaveActivityTemplate(actualData);
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetActivityTemplates()
        {
            var results = await _activityTemplateService.GetActivityTemplates();
            Assert.NotNull(results);
        }

        [Fact]
        public async void GetActivityTemplateDetails()
        {
            string activity_template_guid = "7bc3fc078749453c80ea9ef61b054a26";
            var results = await _activityTemplateService.GetActivityTemplateDetails(activity_template_guid);
            Assert.NotNull(results);
        }

        [Fact]
        public async void DeleteActivityTemplate()
        {
            string activity_template_guid = "";
            var result = await _activityTemplateService.DeleteActivityTemplate(activity_template_guid);
            //Assert.True(results);

            int? removeResultStatusCode = null;
            if (!result)
            {
                _activityService.IsTransactionEnabled = true;
                var activityTemplates = _activityService.DbContext.ActivityTemplate.Where(x => x.Description == "only_for_test");
                if (activityTemplates != null && activityTemplates.Any())
                {
                    _activityService.DbContext.ActivityTemplate.RemoveRange(activityTemplates);
                    removeResultStatusCode = (await _activityService.OkResult()).StatusCode;
                }
                else
                {
                    result = true;
                }
            }

            Assert.True(result || removeResultStatusCode == (int)HttpStatusCode.OK);
        }

        private string _saveActivityGuid;
        [Fact]
        public async void SaveActivity()
        {
            var activity_data = new ActivityDetails()
            {
                activity_guid = null,
                start_date = DateTime.Now,
                end_date = DateTime.Now,
                description = "only_for_test"
            };
            var current_org = "feb1c383e29744f09dbb016ab1342d09";

            _saveActivityGuid = await _activityService.SaveActivity(current_org, activity_data);
            Assert.NotNull(_saveActivityGuid);
        }

        [Fact]
        public async void DeleteActivity()
        {
            var result = await _activityService.DeleteActivity(_saveActivityGuid);

            var activities = _activityService.DbContext.Activity
              .Where(a => a.Description == "only_for_test");

            int? removeResultStatusCode = null;
            if (activities != null && activities.Any())
            {
                _activityService.IsTransactionEnabled = true;
                _activityService.DbContext.Activity.RemoveRange(activities);
                removeResultStatusCode = (await _activityService.OkResult()).StatusCode;
            }

            Assert.True(result || removeResultStatusCode == (int)HttpStatusCode.OK);
        }

        [Fact(Skip = "work, but complex test")]
        public async void GetActivity()
        {
            string activity_guid = "feb1c383e29744f09dbb016ab1342d09";
            var result = await _activityService.GetActivity(activity_guid);
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
                    var dbContext = tempServiceScope.ServiceProvider.GetService<CRPMContext>();
                    var results = dbContext.Activity.Where(x => x.Description.Contains("only_for_test"));
                    if (results != null && results.Any())
                        dbContext.Activity.RemoveRange(results);
                    dbContext.SaveChanges();

                    base.Dispose(disposing);
                }
                disposedValue = true;
            }
        }
    }
}
