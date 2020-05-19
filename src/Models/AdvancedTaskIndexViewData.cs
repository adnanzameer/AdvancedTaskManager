using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask;

namespace AdvancedTask.Models
{
    public class AdvancedTaskIndexViewData
    {
        public AdvancedTaskIndexViewData()
        {
            Sorting = "timestamp_desc";
            PageSize = 30;
            PageNumber = 1;
            HasPublishAccess = false;
            ContentTaskList= new List<ContentTask>();
            ChangeApproval = false;
            ShowChangeApprovalTab = false;
        }

        public List<ContentTask> ContentTaskList { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerSize { get; set; }
        public int TotalItemsCount { get; set; }
        public bool HasPublishAccess { get; set; }
        public bool ChangeApproval { get; set; }
        public bool ShowChangeApprovalTab{ get; set; }

        public IEnumerable<int> Pages
        {
            get
            {
                List<int> list2 = new List<int> { 1 };
                List<int> list = list2;
                if (PageNumber - PagerSize - 1 > 1)
                {
                    list.Add(0);
                }
                for (int i = PageNumber - PagerSize; i <= PageNumber + PagerSize; i++)
                {
                    if (i > 1 && i < TotalPagesCount)
                    {
                        list.Add(i);
                    }
                }
                if (PageNumber + PagerSize + 1 < TotalPagesCount)
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

        public string Sorting { get; set; }
        public string TaskValues { get; set; }
    }
}