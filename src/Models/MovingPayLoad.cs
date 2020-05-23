using EPiServer.Core;

namespace AdvancedTask.Models
{
    public class MovingPayLoad
    {
        public ContentReference Source { get; set; }

        public ContentReference Destination { get; set; }
    }
}
