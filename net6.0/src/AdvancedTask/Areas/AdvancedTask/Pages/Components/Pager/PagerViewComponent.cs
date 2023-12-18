using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace AdvancedTask.Pages.AdvancedTask.Components.Pager
{
    public class PagerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IPagedList items)
        {
            return View(new PagerViewModel
            {
                HasPreviousPage = items.HasPreviousPage,
                HasNextPage = items.HasNextPage,
                PageNumber = items.PageNumber,
                PageCount = items.PageCount,
                QueryString = HttpContext.Request.QueryString.ToString() ?? string.Empty
            });
        }
    }
}
