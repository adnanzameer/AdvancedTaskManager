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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web.Mvc;
using Task = System.Threading.Tasks.Task;

namespace AdvancedTask.Controllers
{
    [EPiServer.Shell.Web.ScriptResource("Scripts/jquery.blockUI.js")]
    [Gadget(ResourceType = typeof(AdvancedTaskController),
           NameResourceKey = "GadgetName", DescriptionResourceKey = "GadgetDescription")]
    [EPiServer.Shell.Web.CssResource("Content/AdvancedTaskGadget.css")]
    [EPiServer.Shell.Web.ScriptResource("Scripts/jquery.form.js")]
    [Authorize]
    public class AdvancedTaskController : Controller
    {
        IApprovalRepository _approvalRepository;
        IContentRepository _contentRepository;
        IContentTypeRepository _contentTypeRepository;
        IUserNotificationRepository _userNotificationRepository;
        IApprovalEngine _approvalEngine;
        public AdvancedTaskController(IApprovalRepository approvalRepository, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, IUserNotificationRepository userNotificationRepository, IApprovalEngine approvalEngine)
        {
            _approvalRepository = approvalRepository;
            _contentRepository = contentRepository;
            _contentTypeRepository = contentTypeRepository;
            _userNotificationRepository = userNotificationRepository;
            _approvalEngine = approvalEngine;
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

            int pageNr = pageNumber ?? 1;
            int pageSz = pageSize ?? 30;

            AdvancedTaskIndexViewData viewModel = new AdvancedTaskIndexViewData
            {
                PageNumber = pageNr,
                PagerSize = 4,
                PageSize = pageSz,
                Sorting = sorting
            };

            Task<List<ContentTask>> task = Task.Run(async () => await GetData(pageNr, pageSz, sorting));
            {
                List<ContentTask> contentTaskList = task.Result;
                viewModel.TotalItemsCount = contentTaskList.Count;
                viewModel.ContentTaskList = contentTaskList;

                int count = contentTaskList.Count(x => x.CanUserPublish);
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
                                    if (block != null)
                                    {
                                        if ((block as IContent).CanUserPublish())
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
        }

        private async Task<List<ContentTask>> GetData(int pageNumber, int pageSize, string sorting)
        {
            //List of All task for the user 
            ApprovalQuery query = new ApprovalQuery
            {
                Status = ApprovalStatus.InReview,
                Username = PrincipalInfo.CurrentPrincipal.Identity.Name
            };

            PagedApprovalResult list = await _approvalRepository.ListAsync(query, (pageNumber - 1) * pageSize, pageSize);

            List<ContentTask> taskList = new List<ContentTask>();
            foreach (Approval task in list.PagedResult)
            {
                ContentApproval approval = task as ContentApproval;

                if (approval != null)
                {
                    _contentRepository.TryGet(approval.ContentLink, out IContent content);


                    if (content != null)
                    {
                        //Create Task Object
                        ContentTask customTask = new ContentTask
                        {
                            ApprovalId = approval.ID,
                            CanUserPublish = content.CanUserPublish(),
                            ContentReference = approval.ContentLink,
                            ContentName = content.Name,
                            ContentType = _contentTypeRepository.Load(content.GetType().BaseType).DisplayName,
                            DateTime = approval.ActiveStepStarted.ToString("dd MMMM HH:mm"),
                            StartedBy = approval.StartedBy
                        };

                        //Get Notifications
                        PagedInternalNotificationMessageResult notifications = await GetNotifications(PrincipalInfo.CurrentPrincipal.Identity.Name, approval.ContentLink.ID.ToString(), "7");
                        if (notifications != null && notifications.PagedResult != null && notifications.PagedResult.Any())
                        {
                            //Mark Notification Read
                            foreach (InternalNotificationMessage notification in notifications.PagedResult)
                            {
                                await _userNotificationRepository.MarkUserNotificationAsReadAsync(new NotificationUser(PrincipalInfo.CurrentPrincipal.Identity.Name), notification.ID);
                            }

                            customTask.NotificationUnread = true;
                        }
                        else
                        {
                            customTask.NotificationUnread = false;
                        }

                        ////Deadline Property of The Content
                        //if (content is BaseContentData pageData)
                        //{
                        //    customTask.Type = "Page";
                        //    if (pageData.ContentApprovalDeadline != null)
                        //    {
                        //        customTask.Deadline = pageData.ContentApprovalDeadline.Value.ToString("dd MMMM HH:mm");
                        //        int days = DateTime.Now.CountDaysInRange(pageData.ContentApprovalDeadline.Value);

                        //        if (days == 0)
                        //        {
                        //            customTask.WarningColor = "red";
                        //        }
                        //        else if (days > 0 && days < 4)
                        //        {
                        //            customTask.WarningColor = "green";
                        //        }
                        //    }
                        //    else
                        //    {
                        //        customTask.Deadline = " - ";
                        //    }
                        //}
                        //else if (content is SiteBlockData blockData)
                        //{
                        //    customTask.Type = "Block";
                        //    if (blockData.ContentApprovalDeadline != null)
                        //    {
                        //        customTask.Deadline = blockData.ContentApprovalDeadline.Value.ToString("dd MMMM HH:mm");
                        //        int days = DateTime.Now.CountDaysInRange(blockData.ContentApprovalDeadline.Value);

                        //        if (days == 0)
                        //        {
                        //            customTask.WarningColor = "red";
                        //        }
                        //        else if (days > 0 && days < 4)
                        //        {
                        //            customTask.WarningColor = "green";
                        //        }
                        //    }
                        //    else
                        //    {
                        //        customTask.Deadline = " - ";
                        //    }
                        //}

                        taskList.Add(customTask);
                    }
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
            }

            return taskList;
        }

        private async Task<PagedInternalNotificationMessageResult> GetNotifications(string user, string contentId, string step)
        {
            IAsyncDatabaseExecutor db = ServiceLocator.Current.GetInstance<IAsyncDatabaseExecutor>();
            return new PagedInternalNotificationMessageResult(await db.ExecuteAsync(async () =>
            {
                List<InternalNotificationMessage> entries = new List<InternalNotificationMessage>();
                string query =
                    "SELECT pkID AS ID, Recipient, Sender, Channel, [Type], [Subject], Content, Sent, SendAt, Saved, [Read], Category FROM [tblNotificationMessage] " +
                    $"WHERE Recipient = '{user}' " +
                    $"AND Content like '%\"contentLink\":\"{contentId}_%' " +
                    $"AND Content like '%status\":{step}%' " +
                    "AND Channel = 'epi-approval' " +
                    "AND[Read] is NULL " +
                    "order by Saved desc";

                DbCommand command = db.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                using (DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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