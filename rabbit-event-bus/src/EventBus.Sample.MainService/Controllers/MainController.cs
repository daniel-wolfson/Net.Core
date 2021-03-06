using Common;
using EventBus.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EventBus.Sample.MainService.Controllers
{
    [Route("main")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        public MainController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        // POST api/values
        [HttpPost("publish-message")]
        public void Post([FromBody] MainServiceModel model)
        {
            _eventBus.Publish(model, nameof(MainServiceModel));
        }
    }
}
