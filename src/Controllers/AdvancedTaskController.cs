using AdvancedTask.Business;
using AdvancedTask.Business.AdvancedTask;
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
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Approvals.FallbackApprovals.Internal;
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
        private readonly IContentRepository _contentRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IApprovalEngine _approvalEngine;
        private readonly LocalizationService _localizationService;

        private const string ContentApprovalDeadlinePropertyName = "ATM_ContentApprovalDeadline";

        public AdvancedTaskController(IApprovalRepository approvalRepository, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, IUserNotificationRepository userNotificationRepository, IApprovalEngine approvalEngine, LocalizationService localizationService)
        {
            _approvalRepository = approvalRepository;
            _contentRepository = contentRepository;
            _contentTypeRepository = contentTypeRepository;
            _userNotificationRepository = userNotificationRepository;
            _approvalEngine = approvalEngine;
            _localizationService = localizationService;
        }
        private void CheckAccess()
        {
            if (!PrincipalInfo.HasEditAccess)
            {
                throw new SecurityException("Access denied");
            }
        }

        public ActionResult Index(int? pageNumber, int? pageSize, string sorting, string taskValues, string approvalComment, string publishContent)
        {
            CheckAccess();

            if (!string.IsNullOrEmpty(taskValues))
            {
                ApproveContent(taskValues, approvalComment, !string.IsNullOrEmpty(publishContent) && publishContent == "true");
            }

            var pageNr = pageNumber ?? 1;
            var pageSz = pageSize ?? 30;

            var viewModel = new AdvancedTaskIndexViewData
            {
                PageNumber = pageNr,
                PagerSize = 4,
                PageSize = pageSz,
                Sorting = sorting
            };

            var task = Task.Run(async () => await GetData(pageNr, pageSz, sorting, viewModel));
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

        private void ApproveContent(string values, string approvalComment, bool publishContent)
        {
            if (!string.IsNullOrEmpty(values))
            {
                string[] ids = values.Split(',');
                foreach (string id in ids)
                {
                    int.TryParse(id, out int approvalId);
                    if (approvalId != 0)
                    {
                        Task<Approval> approval = Task.Run(async () => await _approvalRepository.GetAsync(approvalId));
                        ContentApproval contentApproval = approval.Result as ContentApproval;
                        if (contentApproval != null)
                        {
                            Task.Run(async () => await _approvalEngine.ApproveAsync(approvalId, PrincipalInfo.CurrentPrincipal.Identity.Name, 1, ApprovalDecisionScope.Force, approvalComment));
                            if (publishContent)
                            {
                                _contentRepository.TryGet(contentApproval.ContentLink, out PageData content);
                                if (content != null)
                                {
                                    if (content.CanUserPublish())
                                    {
                                        IContent clone = content.CreateWritableClone();
                                        _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                    }
                                }
                                else
                                {
                                    _contentRepository.TryGet(contentApproval.ContentLink, out BlockData block);
                                    if (block != null && (block as IContent).CanUserPublish())
                                    {
                                        IContent clone = block.CreateWritableClone() as IContent;
                                        _contentRepository.Save(clone, SaveAction.Publish, AccessLevel.Publish);
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<List<ContentTask>> GetData(int pageNumber, int pageSize, string sorting, AdvancedTaskIndexViewData model)
        {
            //List of All task for the user 
            var query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                Username = PrincipalInfo.CurrentPrincipal.Identity.Name
            };

            var list = await _approvalRepository.ListAsync(query, (pageNumber - 1) * pageSize, pageSize);
            model.TotalItemsCount = Convert.ToInt32(list.TotalCount);

            var showApprovalTypeColumn = false;
            var taskList = new List<ContentTask>();

            foreach (var task in list.PagedResult)
            {
                var approvalType = "";
                IContent content = null;
                var contentReference = ContentReference.EmptyReference;
                if (task is ContentApproval approval)
                {
                    approvalType = "Content";
                    contentReference = approval.ContentLink;
                    _contentRepository.TryGet(contentReference, out content);
                }
                else if (task is FallbackApproval fallBack)
                {
                    showApprovalTypeColumn = true;
                    approvalType = "Change";
                    var pageId = fallBack.Reference.AbsolutePath.Replace("/", "");

                    int.TryParse(pageId, out int contentId);
                    if (contentId != 0)
                    {
                        _contentRepository.TryGet(new ContentReference(contentId), out content);
                    }
                }

                if (content != null)
                {
                    //Create Task Object
                    var customTask = new ContentTask
                    {
                        ApprovalId = task.ID,
                        CanUserPublish = content.CanUserPublish(),
                        ContentReference = content.ContentLink,
                        ContentName = content.Name,
                        DateTime = task.ActiveStepStarted.ToString("dd MMMM HH:mm"),
                        StartedBy = task.StartedBy,
                        ApprovalType = approvalType
                    };

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
                            contentName = contentType.DisplayName;
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

                    customTask.ContentType = contentName;

                    //Get Notifications
                    var notifications = await GetNotifications(PrincipalInfo.CurrentPrincipal.Identity.Name, content.ContentLink.ID.ToString(), "7");
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

                    if (content is PageData)
                        customTask.Type = "Page";
                    else if (content is BlockData)
                    {
                        customTask.Type = "Block";

                        if (!string.IsNullOrWhiteSpace(contentName) && contentName.Equals("Form container"))
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
                        if (!string.IsNullOrWhiteSpace(contentName) && contentName.Equals("Video"))
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
                    taskList.Add(customTask);
                }

            }

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
                case "atype_aes":
                    taskList = taskList.OrderBy(x => x.Type).ToList();
                    break;
                case "atype_desc":
                    taskList = taskList.OrderByDescending(x => x.Type).ToList();
                    break;
            }

            return taskList;
        }

        private async Task<PagedInternalNotificationMessageResult> GetNotifications(string user, string contentId, string step)
        {
            IAsyncDatabaseExecutor db = ServiceLocator.Current.GetInstance<IAsyncDatabaseExecutor>();
            return new PagedInternalNotificationMessageResult(await db.ExecuteAsync(async () =>
            {
                var entries = new List<InternalNotificationMessage>();
                var query =
                    "SELECT pkID AS ID, Recipient, Sender, Channel, [Type], [Subject], Content, Sent, SendAt, Saved, [Read], Category FROM [tblNotificationMessage] " +
                    $"WHERE Recipient = '{user}' " +
                    $"AND Content like '%\"contentLink\":\"{contentId}_%' " +
                    $"AND Content like '%status\":{step}%' " +
                    "AND Channel = 'epi-approval' " +
                    "AND [Read] is NULL " +
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