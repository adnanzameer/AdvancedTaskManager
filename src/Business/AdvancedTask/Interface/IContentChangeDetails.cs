namespace AdvancedTask.Business.AdvancedTask.Interface
{
    public interface IContentChangeDetails
    {
        string Name { get; set; }

        object OldValue { get; set; }

        object NewValue { get; set; }
    }
}