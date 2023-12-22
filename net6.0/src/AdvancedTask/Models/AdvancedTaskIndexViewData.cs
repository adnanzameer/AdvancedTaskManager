using System.Collections.Generic;
using System.Web;

namespace AdvancedTask.Models
{
    public class AdvancedTaskIndexViewData
    {
        public AdvancedTaskIndexViewData()
        {
            HasPublishAccess = false;
            ContentTaskList = new List<ContentTask>();
            //ChangeApproval = false;
            //ShowChangeApprovalTab = false;
            PageNumber = 1;
            EnableContentApprovalDeadline = true; // bool.Parse(ConfigurationManager<>.AppSettings["ATM:EnableContentApprovalDeadline"] ?? "false");
        }

        public IEnumerable<int> Pages
        {
            get
            {
                var list2 = new List<int> { 1 };
                var list = list2;
                if (PageNumber - PageSize - 1 > 1)
                {
                    list.Add(0);
                }
                for (int i = PageNumber - PageSize; i <= PageNumber + PageSize; i++)
                {
                    if (i > 1 && i < TotalPagesCount)
                    {
                        list.Add(i);
                    }
                }
                if (PageNumber + PageSize + 1 < TotalPagesCount)
                {
                    list.Add(0);
                }
                if (TotalPagesCount > 1)
                {
                    list.Add(TotalPagesCount);
                }
                return list;
            }
        }
        public int TotalPagesCount => (TotalItemsCount - 1) / PageSize + 1;
        public int MaxIndexOfItem
        {
            get
            {
                if (PageNumber * PageSize <= TotalItemsCount)
                {
                    return PageNumber * PageSize;
                }

                return TotalItemsCount;
            }
        }

        public int MinIndexOfItem
        {
            get
            {
                if (TotalItemsCount <= 0)
                {
                    return 0;
                }

                return (PageNumber - 1) * PageSize + 1;
            }
        }

        public readonly int PageSize = 30;
        public int PageNumber { get; set; }
        public List<ContentTask> ContentTaskList { get; set; }
        public int TotalItemsCount { get; set; }
        public bool HasPublishAccess { get; set; }
        //public bool ChangeApproval { get; set; }
        //public bool ShowChangeApprovalTab { get; set; }
        public string TaskValues { get; set; }
        public string QueryString { get; set; }

        public string PageUrl(int page)
        {
            var qs = HttpUtility.ParseQueryString(QueryString);
            qs["page"] = page.ToString();
            return $"?{qs}";
        }

        public bool EnableContentApprovalDeadline { get; set; }
    }
}