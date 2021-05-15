using Crpm.Model.Data;
using FluentValidation;

namespace Crpm.Dal.Validators
{
    public class UserDetailsValiator2 : AbstractValidator<UserDetails>
    {
        public UserDetailsValiator2()
        {
            RuleFor(x => x.UserGuid).NotEmpty().NotNull();
            RuleFor(x => x.Password).NotEmpty().NotNull();
        }
    }
}
