using EPiServer.Core;

namespace Advanced.Task.Manager.Features.AdvancedTask
{
    public class MovingPayLoad
    {
        public ContentReference Source { get; set; }

        public ContentReference Destination { get; set; }
    }
}
