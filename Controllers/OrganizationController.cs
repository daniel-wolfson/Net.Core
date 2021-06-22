using Crpm.Dal;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Auth;
using Crpm.Infrastructure.Core;
using Crpm.Infrastructure.Filters;
using Crpm.Model.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Crpm.Controllers
{
    [Route("api/DalApi/[controller]")]
    [EnableCors(PolicyTypes.ApiCorsPolicy)]
    [ApiController]
    public class OrganizationController : ControllerBaseAction
    {
        private readonly OrganizationService _orgService;

        public OrganizationController(OrganizationService orgService)
        {
            _orgService = orgService;
        }

        [HttpGet("GetOrganizationType")]
        [ApiCache(typeof(DescriptionTypeData), true)]
        public async Task<IActionResult> GetOrganizationType()
        {
            List<DescriptionTypeData> result = await _orgService.GetOrganizationType();
            return await _orgService.OkResult(result);
        }

        [HttpGet("GetOrganizationTree")]
        [ApiCache(typeof(OrganizationObjectData), true)]
        public async Task<IActionResult> GetOrganizationTree()
        {
            var result = await _orgService.GetOrganizationTree();
            // TODO: only for example
            // var units = result.children.GetFlattenList().Where(x => x.guid == "d67b47ed8e8742faab5d5ad548474fc6");
            return await _orgService.OkResult(result ?? new OrganizationObjectData());
        }

        [HttpGet("GetUnitByGuid")]
        public async Task<IActionResult> GetUnitByGuid(string UnitGuid)
        {
            var result = await _orgService.GetUnitByGuid(UnitGuid);
            return await _orgService.OkResult(result);
        }

        [HttpPost("SaveOrganizationObject")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> SaveOrganizationObject([FromQuery] string parent_guid, [FromBody] OrganizationObjectData data)
        {
            string result = await _orgService.SaveOrganizationObject(parent_guid, data);
            return await _orgService.OkResult(result);
        }

        [HttpPost("DeleteOrganizationOrObject")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> DeleteOrganizationOrObject([FromBody] List<string> organization_guid_list)
        {
            bool result = await _orgService.DeleteOrganizationOrObject(organization_guid_list);
            return await _orgService.OkResult(result);
        }

        [HttpPost("UpdateDragAndDrop")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> UpdateDragAndDrop([FromQuery] string dest_org_guid, [FromQuery] string drag_org_guid, [FromBody] List<string> org_children_guid_list)
        {
            bool result = await _orgService.DragAndDrop(dest_org_guid, drag_org_guid, org_children_guid_list);
            return await _orgService.OkResult(result);
        }

        [HttpPost("UpdateApplyProperties")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> UpdateApplyProperties([FromBody] (OrganizationObjectData obj, List<string> guid_list) data)
        {
            bool result = await _orgService.ApplyProperties(data.obj, data.guid_list);
            return await _orgService.OkResult(result);
        }

        [HttpPost("UpdateDuplicateOrganizationObject")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> UpdateDuplicateOrganizationObject([FromBody] OrganizationObjectData obj, bool isRec)
        {
            //TODO:replace dynamic
            //var obj = Util.JsonConvert<OrganizationObjectData>(organization_object);
            //string result = await _orgService.DuplicateOrganizationObject(obj, isRec);

            string result = await _orgService.DuplicateOrganizationObject(obj, isRec);
            return await _orgService.OkResult(result);
        }

        [HttpGet("GetChildOrgModels")]
        public async Task<IActionResult> GetChildOrgModels([FromQuery] string org_obj_guid)
        {
            var result = await _orgService.GetChildOrgModels(org_obj_guid);
            return Ok(result);
        }

        [HttpGet("GetOrgName")]
        public async Task<IActionResult> GetOrgName([FromQuery] string orgObjGuid)
        {
            var result = await _orgService.GetOrgName(orgObjGuid);
            return Ok(result);
        }

        [HttpPost("GetOrgByModel")]
        public async Task<IActionResult> GetOrgByModel([FromBody] (Dictionary<string, List<OrgModels>> orgModelsDict, string orgObjGuid, string modelComponentGuid) data)
        {
            var result = await _orgService.GetOrgByModel(data.orgModelsDict, data.orgObjGuid, data.modelComponentGuid);
            return Ok(result);
        }

        [HttpGet("GetOrgObjChildrenByModel")]
        public async Task<IActionResult> GetOrgObjChildrenByModel([FromQuery] string parent_org_obj_guid, [FromQuery] string origin_model_component_guid)
        {
            var result = await _orgService.GetOrgObjChildrenByModel(parent_org_obj_guid, origin_model_component_guid);
            return Ok(result);
        }

        [HttpPost("UpdatePermissionUnits")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> UpdatePermissionUnits([FromQuery] Guid ownerUnit, [FromBody] string[] units)
        {
            HttpStatusCode result = await _orgService.UpdatePermissionUnits(ownerUnit, units);
            return await _orgService.OkResult(result);
        }

        #region Interface
        [HttpPost("SaveFullTreeOrganization")]
        [ApiCache(typeof(OrganizationObjectData))]
        public async Task<IActionResult> SaveFullTreeOrganization([FromQuery] string parent_guid, [FromBody] OrganizationObjectData data, [FromQuery] string description_type)
        {
            List<Task> tasksList = new List<Task>();
            OrganizationObjectData result = await _orgService.SaveFullTreeOrganizationObject(parent_guid, data, description_type, tasksList);
            return await _orgService.OkResult(result);
        }
        #endregion
    }
}
