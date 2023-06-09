using AdvancedTask.Business.AdvancedTask.Interface;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class ContentChangeDetails : IContentChangeDetails
    {
        public string Name { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }
    }
}