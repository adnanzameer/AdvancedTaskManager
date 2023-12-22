using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdvancedTask.Business;
using AdvancedTask.Helper;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.Authorization;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Notification.Internal;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;

namespace AdvancedTask.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Policy = Constants.PolicyName)]
    public class AdvancedTaskController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IApprovalRepository _approvalRepository;
        private readonly IUIHelper _helper;
        private readonly IContentRepository _contentRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IApprovalEngine _approvalEngine;
        private readonly LocalizationService _localizationService;
        private readonly IChangeTaskHelper _changeTaskHelper;
        private readonly IConfiguration _configuration;
        private readonly IPrincipalAccessor _principalAccessor;

        private const string ContentApprovalDeadlinePropertyName = "ATM_ContentApprovalDeadline";

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string DateTimeFormatUserFriendly = "MMM dd, yyyy, h:mm:ss tt";
        public AdvancedTaskController(IApprovalRepository approvalRepository, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, IUserNotificationRepository userNotificationRepository, IApprovalEngine approvalEngine, LocalizationService localizationService, IChangeTaskHelper changeTaskHelper, IUIHelper helper, IConfiguration configuration, IPrincipalAccessor principalAccessor)
        {
            _approvalRepository = approvalRepository;
            _contentRepository = contentRepository;
            _contentTypeRepository = contentTypeRepository;
            _userNotificationRepository = userNotificationRepository;
            _approvalEngine = approvalEngine;
            _localizationService = localizationService;
            _changeTaskHelper = changeTaskHelper;
            _helper = helper;
            _configuration = configuration;
            _principalAccessor = principalAccessor;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !string.IsNullOrEmpty(a.FullName) && a.FullName.Contains("EPiServer.ChangeApproval"));

            if (assemblies.Any())
            {
                ViewBag.ChangeApproval = true;
            }

            var viewModel = new AdvancedTaskIndexViewData
            {
                QueryString = HttpContext.Request.QueryString.ToString(),
                PageNumber = page ?? 1
            };

            await ChangeApprovalModel(viewModel);

            return View(viewModel);
        }

        private async Task ChangeApprovalModel(AdvancedTaskIndexViewData viewModel)
        {
            ViewBag.Page = "contentapproval";

            var contentTaskList = await GetContentTasks(viewModel);
            viewModel.ContentTaskList = contentTaskList;

            var count = contentTaskList.Count(x => x.CanUserPublish);
            if (count != 0)
            {
                viewModel.HasPublishAccess = true;
            }
        }

        [HttpGet]
        [Route("/advancedtask/changeapproval")]

        public async Task<IActionResult> ChangeApproval(int? page)
        {
            var viewModel = new AdvancedTaskIndexViewData
            {
                QueryString = HttpContext.Request.QueryString.ToString(),
                PageNumber = page ?? 1
            };

            ViewBag.Page = "changeapproval";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !string.IsNullOrEmpty(a.FullName) && a.FullName.Contains("EPiServer.ChangeApproval"));

            if (!assemblies.Any())
            {
                //delete all change approval tasks
                var deleteChangeApprovalTasks = _configuration.GetValue<bool>("ATM:DeleteChangeApprovalTasks:True");
                if (deleteChangeApprovalTasks)
                {
                    var changeTasks = await GetChangeApprovalTasks(viewModel);
                    var ids = changeTasks.Select(contentTask => contentTask.ApprovalId).ToList();
                    await AbortTasks(ids);
                }

                await ChangeApprovalModel(viewModel);
                return View(viewModel);
            }
            
            ViewBag.ChangeApproval = true;
            var approvalTasks = await GetChangeApprovalTasks(viewModel);
            viewModel.ContentTaskList = approvalTasks;

            return View(viewModel);
        }

        [HttpPost]
        [Route("/advancedtask/approvecontent")]
        public async Task<IActionResult> ApproveContentTasks([FromBody] ApprovalData approvalData)
        {
            ViewBag.Page = "contentapproval";
            if (approvalData != null && !string.IsNullOrEmpty(approvalData.TaskValues) && !string.IsNullOrEmpty(approvalData.ApprovalComment))
            {
                //await ApproveContent(approvalData.TaskValues, approvalData.ApprovalComment, approvalData.PublishContent);
            }
            var viewModel = new AdvancedTaskIndexViewData();
            //var contentTaskList = await GetContentTasks( viewModel);
            //viewModel.ContentTaskList = contentTaskList;

            return Json(viewModel);
        }

        private async Task<List<ContentTask>> GetContentTasks(AdvancedTaskIndexViewData model)
        {
            var roles = await _helper.GetUserRoles();

            var query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                Language = new CultureInfo("en-US"),
                Reference = new Uri("content:"),
            };


            if (!roles.Contains(Roles.Administrators) && !roles.Contains(Roles.WebAdmins) &&
                !roles.Contains(Roles.CmsAdmins))
            {
                query.Username = _principalAccessor.Principal.Identity?.Name;
            }

            var list = await _approvalRepository.ListAsync(query, (model.PageNumber - 1) * model.PageSize, model.PageSize);
            model.TotalItemsCount = Convert.ToInt32(list.TotalCount);

            var taskList = new List<ContentTask>();

            foreach (var task in list.PagedResult)
            {
                var id = task.ID.ToString();

                var customTask = new ContentTask
                {
                    ApprovalId = task.ID,
                    DateTime = task.ActiveStepStarted.ToString(DateTimeFormat),
                    DateTimeUserFriendly = task.ActiveStepStarted.ToString(DateTimeFormatUserFriendly),
                    StartedBy = task.StartedBy
                };

                if (task is ContentApproval approval)
                {
                    _contentRepository.TryGet(approval.ContentLink, out IContent content);

                    if (content != null)
                    {
                        customTask.URL = UIHelper.GetEditUrl(approval.ContentLink, "en-US");
                        id = content.ContentLink.ID.ToString();
                        var canUserPublish = _helper.CanUserPublish(content);

                        customTask.CanUserPublish = canUserPublish;
                        customTask.ContentReference = content.ContentLink;
                        customTask.ContentName = content.Name;

                        customTask.ContentType = GetTypeContent(content);

                        if (content is PageData)
                        {
                            customTask.Type = "Page";
                            customTask.ContentIcon = "file-text";
                        }
                        else if (content is BlockData)
                        {
                            customTask.Type = "Block";
                            customTask.ContentIcon = "file-text";

                            if (!string.IsNullOrWhiteSpace(customTask.ContentType) && customTask.ContentType.Equals("Form container"))
                            {
                                customTask.Type = "Form";
                                customTask.ContentIcon = "file";
                            }
                        }
                        else if (content is ImageData)
                        {
                            customTask.Type = "Image";
                            customTask.ContentIcon = "image";
                        }
                        else if (content is MediaData)
                        {
                            customTask.Type = "Media";
                            customTask.ContentIcon = "youtube";

                            if (!string.IsNullOrWhiteSpace(customTask.ContentType) && customTask.ContentType.Equals("Video"))
                            {
                                customTask.Type = "Video";
                                customTask.ContentIcon = "film";
                            }
                        }

                        var enableContentApprovalDeadline = model.EnableContentApprovalDeadline;
                        var warningDays = 4;//int.Parse(ConfigurationManager.AppSettings["ATM:WarningDays"] ?? "4");

                        if (enableContentApprovalDeadline)
                        {
                            //Deadline Property of The Content
                            var propertyData = content.Property.Get(ContentApprovalDeadlinePropertyName) ?? content.Property[ContentApprovalDeadlinePropertyName];
                            if (propertyData != null)
                            {
                                DateTime.TryParse(propertyData.ToString(), out var dateValue);
                                if (dateValue != DateTime.MinValue && !string.IsNullOrEmpty(customTask.Type))
                                {
                                    customTask.Deadline = dateValue.ToString(DateTimeFormat);
                                    customTask.DeadlineUserFriendly = dateValue.ToString(DateTimeFormatUserFriendly);
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
                            }
                            else
                            {
                                customTask.Deadline = DateTime.Now.AddDays(-7).ToString(DateTimeFormat);
                                customTask.DeadlineUserFriendly = DateTime.Now.AddDays(-7).ToString(DateTimeFormatUserFriendly);
                                //customTask.WarningColor = "red";
                                customTask.WarningColor = "green";

                            }
                        }
                    }

                    //Get Notifications
                    customTask = await GetNotifications(id, customTask, true);

                    taskList.Add(customTask);
                }
            }

            return taskList;
        }

        private async Task ApproveContent(string values, string approvalComment, bool publishContent)
        {
            if (!string.IsNullOrEmpty(values))
            {
                var ids = values.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

                foreach (var id in ids)
                {
                    int.TryParse(id, out var approvalId);
                    if (approvalId != 0)
                    {
                        var approval = await _approvalRepository.GetAsync(approvalId);
                        await _approvalEngine.ForceApproveAsync(approvalId, _principalAccessor.Principal.Identity?.Name, approvalComment);

                        if (approval is ContentApproval contentApproval)
                        {
                            _contentRepository.TryGet(contentApproval.ContentLink, out IContent content);

                            var canUserPublish = publishContent && _helper.CanUserPublish(content);
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

        private async Task<List<ContentTask>> GetChangeApprovalTasks(AdvancedTaskIndexViewData model)
        {
            var roles = await _helper.GetUserRoles();

            var query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                //Language = new CultureInfo("en-US"),
                Reference = new Uri("changeapproval:")
            };

            if (!roles.Contains(Roles.Administrators) && !roles.Contains(Roles.WebAdmins) &&
                !roles.Contains(Roles.CmsAdmins))
            {
                query.Username = _principalAccessor.Principal.Identity?.Name;
            }

            var list = await _approvalRepository.ListAsync(query, (model.PageNumber - 1) * model.PageSize, model.PageSize);
            model.TotalItemsCount = Convert.ToInt32(list.TotalCount);

            var taskList = new List<ContentTask>();

            foreach (var task in list.PagedResult)
            {
                IContent content = null;
                var id = task.ID.ToString();

                var customTask = new ContentTask
                {
                    ApprovalId = task.ID,
                    DateTime = task.ActiveStepStarted.ToString(DateTimeFormat),
                    DateTimeUserFriendly = task.ActiveStepStarted.ToString(DateTimeFormatUserFriendly),
                    StartedBy = task.StartedBy,
                    URL = UIHelper.GetEditUrl(new ContentReference(task.ID)).Replace(".contentdata:", ".changeapproval:")
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

            return taskList;
        }

        private async Task AbortTasks(List<int> ids)
        {
            await _approvalEngine.AbortAsync(ids, _principalAccessor.Principal.Identity?.Name);
        }

        private async Task<ContentTask> GetNotifications(string id, ContentTask customTask, bool isContentQuery)
        {
            if (_principalAccessor.Principal.Identity != null && !string.IsNullOrEmpty(_principalAccessor.Principal.Identity.Name))
            {
                var notifications = await GetNotifications(_principalAccessor.Principal.Identity.Name, id, isContentQuery);

                if (notifications?.PagedResult != null && notifications.PagedResult.Any())
                {
                    //Mark Notification Read
                    foreach (var notification in notifications.PagedResult)
                    {
                        await _userNotificationRepository.MarkUserNotificationAsReadAsync(new NotificationUser(_principalAccessor.Principal.Identity.Name), notification.ID);
                    }

                    customTask.NotificationUnread = true;
                }
                else
                {
                    customTask.NotificationUnread = false;
                }
            }

            return customTask;
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

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(NotificationMessageFromReader.Create(reader));
                    }
                }
                return (IList<InternalNotificationMessage>)entries;
            }).ConfigureAwait(false), 0L);
        }
    }

    public class ApprovalData
    {
        public string TaskValues { get; set; }
        public string ApprovalComment { get; set; }
        public bool PublishContent { get; set; }
    }
}