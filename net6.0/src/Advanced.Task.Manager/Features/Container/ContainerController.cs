using Advanced.Task.Manager.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Advanced.Task.Manager.Features.Container
{
    public class ContainerController : Controller
    {
        [Authorize(Policy = Constants.PolicyName)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
