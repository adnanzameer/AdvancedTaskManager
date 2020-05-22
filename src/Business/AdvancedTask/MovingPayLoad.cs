using EPiServer.Core;

namespace AdvancedTask.Business.AdvancedTask
{
    public class MovingPayLoad
    {
        public ContentReference Source { get; set; }

        public ContentReference Destination { get; set; }
    }
}
