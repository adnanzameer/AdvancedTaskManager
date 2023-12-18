using Microsoft.AspNetCore.Mvc;

namespace AdvancedTask.Pages.AdvancedTask.Pages.Models
{
    public class Paging
    {
        public const int DefaultPageSize = 50;

        [FromQuery(Name = "page")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "page-size")]
        public int PageSize { get; set; } = DefaultPageSize;
    }
}
