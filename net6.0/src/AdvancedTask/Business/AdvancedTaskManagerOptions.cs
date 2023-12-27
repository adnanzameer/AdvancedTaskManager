namespace AdvancedTask.Business
{
	public class AdvancedTaskManagerOptions
	{
		public int WarningDays { get; set; } = 4;
		public bool DeleteChangeApprovalTasks { get; set; } = true;
		public bool DeleteContentApprovalDeadlineProperty { get; set; } = false;
		public bool AddContentApprovalDeadlineProperty { get; set; } = false;
	}
}
