using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;

namespace AdvancedTask.Models
{
    public class AdvancedTaskIndexViewData
    {
        public AdvancedTaskIndexViewData(List<LanguageBranchOption> languageBranchList)
        {
            LanguageBranchList = languageBranchList;
            SelectedLanguageText = "Select";

            if (languageBranchList != null && languageBranchList.Any())
            {
                var selectedLanguage = languageBranchList.FirstOrDefault(x => x.Selected);

                if (selectedLanguage?.Language != null)
                {
                    SelectedLanguageText = selectedLanguage.Language.Name;
                }
            }

            HasPublishAccess = false;
            ContentTaskList = new List<ContentTask>();
            PageNumber = 1;
            EnableContentApprovalDeadline = false; // bool.Parse(ConfigurationManager<>.AppSettings["ATM:EnableContentApprovalDeadline"] ?? "false");
        }

        public readonly string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public readonly string DateTimeFormatUserFriendly = "MMM dd, yyyy, h:mm:ss tt";
        
        public const int DefaultPageSize = 50;

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

        
        
        public int PageSize { get; set; } = DefaultPageSize;
        
        public int PageNumber { get; set; }
       
        public List<ContentTask> ContentTaskList { get; set; }
       
        public int TotalItemsCount { get; set; }
        
        public bool HasPublishAccess { get; set; }
        
        public string TaskValues { get; set; }
        
        public string QueryString { get; set; }

        public string PageUrl(int page)
        {
            var qs = HttpUtility.ParseQueryString(QueryString);
            qs["page"] = page.ToString();
            return $"?{qs}";
        }

        public string LanguageUrl(string language)
        {
            var qs = HttpUtility.ParseQueryString(QueryString);
            qs["language"] = language;
            return $"?{qs}";
        }

        public bool EnableContentApprovalDeadline { get; set; }



        public List<LanguageBranchOption> LanguageBranchList { get; set; }
        
        public string  SelectedLanguageText { get; set; }
    }

    public class LanguageBranchOption
    {
        public LanguageBranch Language { get; set; }
        public bool Selected { get; set; }
    }
}