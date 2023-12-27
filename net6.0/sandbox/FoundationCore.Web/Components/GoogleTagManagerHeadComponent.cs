using FoundationCore.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FoundationCore.Web.Components;

public class GoogleTagManagerHeadComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        var settings = PageHelper.SiteSettingsPage;
        if (settings == null)
        {
            return Task.FromResult<IViewComponentResult>(Content(""));
        }
        return Task.FromResult<IViewComponentResult>(View("~/Views/Shared/Partial/_GoogleTagManagerHeadScript.cshtml", settings.GoogleTagManagerId));
    }
}