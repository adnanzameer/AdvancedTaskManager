using EPiServer.Core;

namespace AdvancedTask.Features.AdvancedTask
{
    public class MovingPayLoad
    {
        public ContentReference Source { get; set; }

        public ContentReference Destination { get; set; }
    }
}
