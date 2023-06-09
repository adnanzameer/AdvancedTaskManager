using EPiServer.Core;

namespace AdvancedTask.Models
{
    internal class MovingPayLoad
    {
        public ContentReference Source { get; set; }

        public ContentReference Destination { get; set; }
    }
}
