using AdvancedTask.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedTask.Features.Container
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
