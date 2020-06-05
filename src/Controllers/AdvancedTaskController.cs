using AdvancedTask.Business;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Notification.Internal;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Gadgets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web.Mvc;
using AdvancedTask.Business.AdvancedTask;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Helper;
using EPiServer.Editor;
using Task = System.Threading.Tasks.Task;

namespace AdvancedTask.Controllers
{
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.blockUI.js")]
    [Gadget(ResourceType = typeof(AdvancedTaskController), NameResourceKey = "GadgetName", DescriptionResourceKey = "GadgetDescription")]
    [EPiServer.Shell.Web.CssResource("ClientResources/Content/AdvancedTaskGadget.css")]
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.form.js")]
    [Authorize]
    public class AdvancedTaskController : Controller
    {
        private readonly IApprovalRepository _approvalRepository;
        private readonly UIHelper _helper;
        private readonly IContentRepository _contentRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IApprovalEngine _approvalEngine;
        private readonly LocalizationService _localizationService;
        private readonly ChangeTaskHelper _changeTaskHelper;
        private readonly ChangeApprovalActions _changeApprovalService;

        private const string ContentApprovalDeadlinePropertyName = "ATM_ContentApprovalDeadline";

        public AdvancedTaskController(IApprovalRepository approvalRepository, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, IUserNotificationRepository userNotificationRepository, IApprovalEngine approvalEngine, LocalizationService localizationService, IAsyncDatabaseExecutor asyncDatabaseExecutor, ChangeTaskHelper changeTaskHelper, UIHelper helper, ChangeApprovalActions changeApprovalService)
        {
            _approvalRepository = approvalRepository;
            _contentRepository = contentRepository;
            _contentTypeRepository = contentTypeRepository;
            _userNotificationRepository = userNotificationRepository;
            _approvalEngine = approvalEngine;
            _localizationService = localizationService;
            _changeTaskHelper = changeTaskHelper;
            _helper = helper;
            _changeApprovalService = changeApprovalService;
        }
        private void CheckAccess()
        {
            if (!PrincipalInfo.HasEditAccess)
            {
                throw new SecurityException("Access denied");
            }
        }

        public ActionResult Index(int? pageNumber, int? pageSize, string sorting, string taskValues, string approvalComment, string publishContent, bool? isChange)
        {
            CheckAccess();

            var pageNr = pageNumber ?? 1;
            var pageSz = pageSize ?? 30;

            var viewModel = new AdvancedTaskIndexViewData
            {
                PageNumber = pageNr,
                PagerSize = 4,
                PageSize = pageSz,
                Sorting = sorting
            };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("EPiServer.ChangeApproval"));

            if (assemblies.Any())
            {
                viewModel.ShowChangeApprovalTab = true;

                if (isChange.HasValue && isChange.Value)
                {
                    viewModel.ChangeApproval = true;

                    var changeTasks = Task.Run(async () => await ProcessChangeData(pageNr, pageSz, sorting, viewModel, taskValues, approvalComment));
                    {
                        var changeTaskList = changeTasks.Result;
                        viewModel.ContentTaskList = changeTaskList;
                    }

                    return View("Index", viewModel);
                }
            }
            else
            {
                //delete all change approval tasks
                var deleteChangeApprovalTasks = bool.Parse(ConfigurationManager.AppSettings["ATM:DeleteChangeApprovalTasks"] ?? "false");
                if (deleteChangeApprovalTasks)
                {
                    var changeTasks = Task.Run(async () => await ProcessChangeData(pageNr, pageSz, sorting, viewModel, taskValues, approvalComment));
                    {
                        var changeTaskList = changeTasks.Result;

                        var ids = changeTaskList.Select(contentTask => contentTask.ApprovalId).ToList();
                        Task.Run(async () => await AbortTasks(ids));
                    }
                }
            }

            var task = Task.Run(async () => await ProcessContentData(pageNr, pageSz, sorting, viewModel, taskValues, approvalComment));
            {
                var contentTaskList = task.Result;
                viewModel.ContentTaskList = contentTaskList;

                var count = contentTaskList.Count(x => x.CanUserPublish);
                if (count != 0)
                {
                    viewModel.HasPublishAccess = true;
                }
            }

            return View("Index", viewModel);
        }


        private async Task ApproveContent(string values, string approvalComment)
        {
            if (!string.IsNullOrEmpty(values))
            {
                var ids = values.Split(',');
                foreach (var id in ids)
                {
                    int.TryParse(id, out var approvalId);
                    if (approvalId != 0)
                    {
                        var approval = await _approvalRepository.GetAsync(approvalId);
                        await _approvalEngine.ForceApproveAsync(approvalId, PrincipalInfo.CurrentPrincipal.Identity.Name, approvalComment);

                        if (approval is ContentApproval contentApproval)
                        {
                            _contentRepository.TryGet(contentApproval.ContentLink, out IContent content);

                            var canUserPublish = await _helper.CanUserPublish(content);
                            if (content != null && canUserPublish)
                            {
                                switch (content)
                                {
                                    case PageData page:
                                        {
                                            var clone = page.CreateWritableClone();
                                            _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                            break;
                                        }
                                    case BlockData block:
                                        {
                                            var clone = block.CreateWritableClone() as IContent;
                                            _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                            break;
                                        }
                                    case ImageData image:
                                        {
                                            var clone = image.CreateWritableClone() as IContent;
                                            _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                            break;
                                        }
                                    case MediaData media:
                                        {
                                            var clone = media.CreateWritableClone() as IContent;
                                            _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task ApproveChangeTask(string values, string approvalComment)
        {
            if (!string.IsNullOrEmpty(values))
            {
                var ids = values.Split(',');
                foreach (var id in ids)
                {
                    int.TryParse(id, out var approvalId);
                    if (approvalId != 0)
                    {
                        await _changeApprovalService.ForceComplete(approvalId, approvalComment);
                    }
                }
            }
        }

        private async Task AbortTasks(List<int> ids)
        {
            await _approvalEngine.AbortAsync(ids, PrincipalInfo.CurrentPrincipal.Identity.Name);
        }


        private async Task<List<ContentTask>> ProcessContentData(int pageNumber, int pageSize, string sorting, AdvancedTaskIndexViewData model, string taskValues, string approvalComment)
        {
            if (!string.IsNullOrEmpty(taskValues))
            {
                await ApproveContent(taskValues, approvalComment);
            }

            //List of All task for the user 
            var query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                Username = PrincipalInfo.CurrentPrincipal.Identity.Name,
                Reference = new Uri("content:")
            };

            var list = await _approvalRepository.ListAsync(query, (pageNumber - 1) * pageSize, pageSize);
            model.TotalItemsCount = Convert.ToInt32(list.TotalCount);

            var taskList = new List<ContentTask>();

            foreach (var task in list.PagedResult)
            {
                var id = task.ID.ToString();

                var customTask = new ContentTask
                {
                    ApprovalId = task.ID,
                    DateTime = task.ActiveStepStarted.ToString("dd MMMM HH:mm"),
                    StartedBy = task.StartedBy
                };

                if (task is ContentApproval approval)
                {
                    _contentRepository.TryGet(approval.ContentLink, out IContent content);

                    if (content != null)
                    {
                        customTask.URL = PageEditing.GetEditUrl(approval.ContentLink);
                        id = content.ContentLink.ID.ToString();
                        var canUserPublish = await _helper.CanUserPublish(content);

                        customTask.CanUserPublish = canUserPublish;
                        customTask.ContentReference = content.ContentLink;
                        customTask.ContentName = content.Name;

                        customTask.ContentType = GetTypeContent(content);

                        if (content is PageData)
                            customTask.Type = "Page";
                        else if (content is BlockData)
                        {
                            customTask.Type = "Block";

                            if (!string.IsNullOrWhiteSpace(customTask.ContentType) && customTask.ContentType.Equals("Form container"))
                            {
                                customTask.Type = "Form";
                            }
                        }
                        else if (content is ImageData)
                        {
                            customTask.Type = "Image";
                        }
                        else if (content is MediaData)
                        {
                            customTask.Type = "Media";
                            if (!string.IsNullOrWhiteSpace(customTask.ContentType) && customTask.ContentType.Equals("Video"))
                            {
                                customTask.Type = "Video";
                            }
                        }

                        var enableContentApprovalDeadline = bool.Parse(ConfigurationManager.AppSettings["ATM:EnableContentApprovalDeadline"] ?? "false");
                        var warningDays = int.Parse(ConfigurationManager.AppSettings["ATM:WarningDays"] ?? "4");

                        if (enableContentApprovalDeadline)
                        {
                            //Deadline Property of The Content
                            var propertyData = content.Property.Get(ContentApprovalDeadlinePropertyName) ?? content.Property[ContentApprovalDeadlinePropertyName];
                            if (propertyData != null)
                            {
                                DateTime.TryParse(propertyData.ToString(), out DateTime dateValue);
                                if (dateValue != DateTime.MinValue)
                                {
                                    if (!string.IsNullOrEmpty(customTask.Type))
                                    {
                                        customTask.Deadline = dateValue.ToString("dd MMMM HH:mm");
                                        var days = DateTime.Now.CountDaysInRange(dateValue);

                                        if (days == 0)
                                        {
                                            customTask.WarningColor = "red";
                                        }
                                        else if (days > 0 && days < warningDays)
                                        {
                                            customTask.WarningColor = "green";
                                        }
                                    }
                                    else
                                    {
                                        customTask.Deadline = " - ";
                                    }
                                }
                            }
                        }
                    }

                    //Get Notifications
                    customTask = await GetNotifications(id, customTask, true);

                    taskList.Add(customTask);
                }
            }

            taskList = SortColumns(sorting, taskList);

            return taskList;
        }

        private async Task<List<ContentTask>> ProcessChangeData(int pageNumber, int pageSize, string sorting, AdvancedTaskIndexViewData model, string taskValues, string approvalComment)
        {
            if (!string.IsNullOrEmpty(taskValues))
            {
                await ApproveChangeTask(taskValues, approvalComment);
            }

            //List of All task for the user 
            var query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                Username = PrincipalInfo.CurrentPrincipal.Identity.Name,
                Reference = new Uri("changeapproval:")
            };

            var list = await _approvalRepository.ListAsync(query, (pageNumber - 1) * pageSize, pageSize);
            model.TotalItemsCount = Convert.ToInt32(list.TotalCount);

            var taskList = new List<ContentTask>();


            foreach (var task in list.PagedResult)
            {
                IContent content = null;
                var id = task.ID.ToString();

                var customTask = new ContentTask
                {
                    ApprovalId = task.ID,
                    DateTime = task.ActiveStepStarted.ToString("dd MMMM HH:mm"),
                    StartedBy = task.StartedBy,
                    URL = PageEditing.GetEditUrl(new ContentReference(task.ID)).Replace(".contentdata:", ".changeapproval:")
                };

                if (!(task is ContentApproval))
                {

                    var taskDetails = _changeTaskHelper.GetData(task.ID);

                    if (taskDetails != null)
                    {
                        customTask.Type = taskDetails.Type;
                        customTask.ContentName = taskDetails.Name;
                        customTask.Details = taskDetails.Details;
                    }

                    if (task.Reference != null)
                    {
                        if (!string.IsNullOrEmpty(task.Reference.AbsolutePath))
                        {
                            var pageId = task.Reference.AbsolutePath.Replace("/", "");

                            int.TryParse(pageId, out var contentId);
                            if (contentId != 0)
                            {
                                _contentRepository.TryGet(new ContentReference(contentId), out content);
                            }
                        }
                    }

                    if (content != null)
                    {
                        customTask.ContentReference = content.ContentLink;
                        customTask.ContentType = GetTypeContent(content);
                    }

                    customTask = await GetNotifications(id, customTask, false);

                    taskList.Add(customTask);
                }
            }

            taskList = SortColumns(sorting, taskList);

            return taskList;
        }

        private async Task<ContentTask> GetNotifications(string id, ContentTask customTask, bool isContentQuery)
        {
            var notifications = await GetNotifications(PrincipalInfo.CurrentPrincipal.Identity.Name, id, isContentQuery);

            if (notifications != null && notifications.PagedResult != null && notifications.PagedResult.Any())
            {
                //Mark Notification Read
                foreach (var notification in notifications.PagedResult)
                {
                    await _userNotificationRepository.MarkUserNotificationAsReadAsync(new NotificationUser(PrincipalInfo.CurrentPrincipal.Identity.Name), notification.ID);
                }

                customTask.NotificationUnread = true;
            }
            else
            {
                customTask.NotificationUnread = false;
            }

            return customTask;
        }
        private List<ContentTask> SortColumns(string sorting, List<ContentTask> taskList)
        {
            //Sorting of the Columns 
            switch (sorting)
            {
                case "name_aes":
                    taskList = taskList.OrderBy(x => x.ContentName).ToList();
                    break;
                case "name_desc":
                    taskList = taskList.OrderByDescending(x => x.ContentName).ToList();
                    break;
                case "ctype_aes":
                    taskList = taskList.OrderBy(x => x.ContentType).ToList();
                    break;
                case "ctype_desc":
                    taskList = taskList.OrderByDescending(x => x.ContentType).ToList();
                    break;
                case "timestamp_aes":
                    taskList = taskList.OrderBy(x => x.DateTime).ToList();
                    break;
                case "timestamp_desc":
                    taskList = taskList.OrderByDescending(x => x.DateTime).ToList();
                    break;
                case "user_aes":
                    taskList = taskList.OrderBy(x => x.StartedBy).ToList();
                    break;
                case "user_desc":
                    taskList = taskList.OrderByDescending(x => x.StartedBy).ToList();
                    break;
                case "deadline_aes":
                    taskList = taskList.OrderBy(x => x.Deadline).ToList();
                    break;
                case "deadline_desc":
                    taskList = taskList.OrderByDescending(x => x.Deadline).ToList();
                    break;
                case "type_aes":
                    taskList = taskList.OrderBy(x => x.Type).ToList();
                    break;
                case "type_desc":
                    taskList = taskList.OrderByDescending(x => x.Type).ToList();
                    break;
            }

            return taskList;
        }

        private string GetTypeContent(IContent content)
        {
            var contentName = "";

            var contentType = _contentTypeRepository.Load(content.GetType().BaseType);

            if (contentType != null)
            {
                contentName = contentType.DisplayName;
            }
            else
            {
                contentType = _contentTypeRepository.Load(content.GetType());

                if (contentType != null)
                {
                    if (!string.IsNullOrEmpty(contentType.DisplayName))
                        contentName = contentType.DisplayName;
                    else if (!string.IsNullOrEmpty(contentType.Name))
                        contentName = contentType.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(contentName))
            {
                var memberInfo = content.GetType().BaseType;
                if (memberInfo != null)
                {
                    contentName = _localizationService.GetString("/contenttypes/" + memberInfo.Name.ToLower() + "/name", FallbackBehaviors.FallbackCulture);
                }
            }

            if (!string.IsNullOrWhiteSpace(contentName) && contentName.Contains("[Missing text"))
            {
                contentName = "";
            }

            return contentName;
        }

        private async Task<PagedInternalNotificationMessageResult> GetNotifications(string user, string contentId, bool isContentQuery = true)
        {
            var db = ServiceLocator.Current.GetInstance<IAsyncDatabaseExecutor>();
            return new PagedInternalNotificationMessageResult(await db.ExecuteAsync(async () =>
            {
                var entries = new List<InternalNotificationMessage>();
                var query = "SELECT pkID AS ID, Recipient, Sender, Channel, [Type], [Subject], Content, Sent, SendAt, Saved, [Read], Category FROM [tblNotificationMessage] " +
                                   $"WHERE Recipient = '{user}'";

                if (isContentQuery)
                {
                    query = query + $"AND Content like '%\"contentLink\":\"{contentId}_%' " +
                                    $"AND Content like '%status\":7%' " +
                                    "AND Channel = 'epi-approval' ";
                }
                else
                {
                    query = query + $"AND Content like '%\"ApprovalID\": {contentId},%' " +
                                    "AND Channel = 'epi-changeapproval' ";
                }

                query = query + "AND [Read] is NULL " +
                                "order by Saved desc";

                var command = db.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        entries.Add(NotificationMessageFromReader.Create(reader));
                    }
                }
                return (IList<InternalNotificationMessage>)entries;
            }).ConfigureAwait(false), 0L);
        }

        public static string GadgetEditMenuName => LocalizationService.Current.GetString("/gadget/tasks/configure");

        public static string GadgetName => LocalizationService.Current.GetString("/gadget/tasks/name");

        public static string GadgetDescription => LocalizationService.Current.GetString("/gadget/tasks/description");
    }
}