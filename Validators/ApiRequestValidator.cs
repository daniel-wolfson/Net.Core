﻿using Crpm.Infrastructure.Interfaces;
using Crpm.Model.Data;
using FluentValidation;

namespace Crpm.Infrastructure.Validators
{
    class ApiRequestValidator : AbstractValidator<UserDetails>
    {
        public ApiRequestValidator()
        {
            RuleFor(x => x.UserGuid)
                .NotEmpty()
                //.Must
                .NotNull();

            //public List<EntityDescription> entity_description_list { get; set; }
            //public List<Descriptions> description_list { get; set; }
            //public List<ConvertionTable> convertion_table { get; set; }
            //public List<MeasuringUnit> measuring_unit { get; set; }
            //public List<CalenderRollup> calender_rollup { get; set; }
            //public List<RollupMethod> rollup_method { get; set; }
            //public List<string> model_orgs_list { get; set; }
            //public List<Connected_Model> connected_model_list { get; set; }
        }
    }
}
