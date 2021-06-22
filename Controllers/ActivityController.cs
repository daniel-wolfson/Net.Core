using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Auth;
using Crpm.Infrastructure.Helpers;
using Crpm.Model.Data;

namespace Crpm.Dal.Controllers
{
    [Route("api/DalApi/[controller]")]
    [EnableCors(PolicyTypes.ApiCorsPolicy)]
    [ApiController]
    public class ActivityController : ControllerBaseAction
    {
        private readonly ActivityService _activityService;
        private readonly ActivityTemplateService _activityTemplateService;

        public ActivityController(ActivityService activityService, ActivityTemplateService activityTemplateService)
        {
            _activityService = activityService;
            _activityTemplateService = activityTemplateService;
        }

        #region Activity_Template

        [HttpPost("SaveActivityTemplate")]
        public async Task<IActionResult> SaveActivityTemplate([FromBody] ActivityTemplateData activity_template_data)
        {
            string result = await _activityTemplateService.SaveActivityTemplate(activity_template_data);
            return await _activityService.OkResult(result);
        }

        [HttpGet("GetActivityTemplates")]
        public async Task<List<ActivityTemplateDataInfo>> GetActivityTemplates()
        {
            var results = await _activityTemplateService.GetActivityTemplates();
            return results;
        }

        [HttpGet("GetActivityTemplateDetails")]
        public async Task<ActivityTemplateData> GetActivityTemplateDetails([FromQuery]string activity_template_guid)
        {
            var results = await _activityTemplateService.GetActivityTemplateDetails(activity_template_guid);
            return results;
        }

        [HttpGet("DeleteActivityTemplate")]
        public async Task<IActionResult> DeleteActivityTemplate([FromQuery]string activity_template_guid)
        {
            var results = await _activityTemplateService.DeleteActivityTemplate(activity_template_guid);
            return await _activityService.OkResult(results);
        }

        #endregion Activity_Template

        #region Activity

        [HttpPost("SaveActivity")]
        public async Task<IActionResult> SaveActivity([FromBody](ActivityDetails activity_data, string current_org) data)
        {
            string result = await _activityService.SaveActivity(data.current_org, data.activity_data);
            return await _activityService.OkResult(result);
        }

        [HttpGet("DeleteActivity")]
        public async Task<IActionResult> DeleteActivity([FromQuery]string activity_guid)
        {
            var result = await _activityService.DeleteActivity(activity_guid);
            return await _activityService.OkResult(result);
        }

        [HttpGet("GetActivity")]
        public async Task<ActivityDetails> GetActivity([FromQuery]string activity_guid)
        {
            var result = await _activityService.GetActivity(activity_guid);
            return result;
        }

        #endregion Activity
    }
}
