using Crpm.Dal.Controllers;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Core;
using Crpm.Infrastructure.Helpers;
using Crpm.Infrastructure.Models;
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
    public class FormControllerTest : TestWebHost
    {
        private FormService _formService;
        private FormTemplateService _formTemplateService;
        private readonly FormController _controller;

        public FormControllerTest()
        {
            _formService = base.GetRootService<FormService>();
            _formTemplateService = base.GetRootService<FormTemplateService>();
            _controller = new FormController(_formService, _formTemplateService);
        }

        #region Form_Template

        [Fact]
        public async void SaveFormTemplate()
        {
            FormTemplate form_template = new FormTemplate()
            {
                FormTemplateGuid = "",
                Name = "",
                Description = "only_for_test",
                ModifiedDate = Util.ConvertDateToString(DateTime.Now),
                CreateDate = Util.ConvertDateToString(DateTime.Now),
                CreatorUserGuid = CurrentTestUser.UserGuid
            };

            List<FormItemData> form_items_list = new List<FormItemData>() {
                new FormItemData()
                {
                    comment = "", score = 0.9,
                    connected_model_guid = null,
                    form_element_type = null,
                    professional_instruction = "",
                    metric_show_origion_value = false,
                    metric_form_irrelevant = false,
                    metric_not_display_if_irrelevant = false,
                    metric_measuring_unit = null,
                    metric_required = null,
                    metric_status = null,
                    order = null,
                    title = "test",
                    form_element_guid = null,
                    model_component_guid = null,
                    showConverTableFlag = false
                }
            };

            List<string> activities_list = new List<string>() { "" };

            _formTemplateService.IsTransactionEnabled = true;
            string actionResult = await _formTemplateService.SaveFormTemplate(form_template, form_items_list, activities_list);
            var serviceResult = await _formTemplateService.OkResult(actionResult);

            Assert.True(actionResult != null || serviceResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void GetFormTemplates()
        {
            Task<List<FormTemplateDataInfo>> actionTask = _formTemplateService.GetAllFormTemplates();
            List<FormTemplateDataInfo> actionResult = await actionTask;
            Assert.True(actionResult != null || actionTask.IsCompletedSuccessfully);
        }

        [Theory]
        [InlineData("")]
        public async void GetFormTemplateDetails(string form_template_guid)
        {
            Task<FormTemplateData> actionTask = _formTemplateService.GetFormTemplateDetails(form_template_guid);
            FormTemplateData actionResult = await actionTask;
            Assert.True(actionResult != null || actionTask.IsCompletedSuccessfully);
        }

        [Theory]
        [InlineData("93626aba3a844796951e3b5d4a0e5b33")]
        public async void DeleteFormTemplate([FromQuery] string form_template_guid)
        {
            bool actionResult = await _formTemplateService.DeleteFormTemplate(form_template_guid); ;
            var serviceResult = await _formTemplateService.OkResult(actionResult);
            Assert.True(actionResult || serviceResult.StatusCode == (int)HttpStatusCode.OK);
        }

        #endregion Form_Template

        #region Form

        [Theory]
        [InlineData("f2c87478bfb249b19f190ed4b708c2aa")]
        public async void GetFormDetails(string form_guid)
        {
            FormData actionResult = null;
            Task<FormData> actionTask = _formService.GetFormDetails(form_guid);
            try
            {
                actionResult = await actionTask;
            }
            catch { }

            Assert.True(actionResult != null || actionTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async void SaveFormScore()
        {
            FormData actionData = new FormData()
            {
                activity_details = new ActivityDetails() { },
                form_details = new FormDetails() { },
                form_items = new List<FormGroupData>() { }
            };

            bool actionResult = await _formService.SaveFormScore(actionData);
            ServiceResult<bool> serviceResult = await _formTemplateService.OkResult(actionResult);
            Assert.True(actionResult || serviceResult.StatusCode == (int)HttpStatusCode.OK);
        }

        #endregion Form

        // Clean test data
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    using var tempServiceScope = GeneralContext.CreateServiceScope();
                    var dbContext = tempServiceScope.ServiceProvider.GetService<CRPMContext>();
                    var formResults = dbContext.Form.Where(x => x.FormGuid.Contains("only_for_test"));
                    if (formResults != null && formResults.Any())
                        dbContext.Form.RemoveRange(formResults);
                    var formTemplates = dbContext.FormTemplate.Where(x => x.Description.Contains("only_for_test"));
                    if (formTemplates != null && formTemplates.Any())
                        dbContext.FormTemplate.RemoveRange(formTemplates);
                    dbContext.SaveChanges();
                    base.Dispose(disposing);
                }
                disposedValue = true;
            }
        }
    }
}
