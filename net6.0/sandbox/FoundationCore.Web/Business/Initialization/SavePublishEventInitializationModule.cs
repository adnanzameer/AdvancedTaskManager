using System.Globalization;
using EPiServer.DataAccess;
using EPiServer.Find.Cms;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using FoundationCore.Web.Business.Caching;
using FoundationCore.Web.Models.Interface;

namespace FoundationCore.Web.Business.Initialization
{
    [InitializableModule]
    public class SavePublishEventInitializationModule : IInitializableModule
    {
        private readonly Lazy<ICacheManager<string>> _cacheService = new(() => ServiceLocator.Current.GetInstance<ICacheManager<string>>());

        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();

            contentEvents.PublishingContent += Instance_PublishingPage;
            contentEvents.MovingContent += ContentEvents_MovingContent;
            contentEvents.CreatedContent += CreatedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishingContent -= Instance_PublishingPage;
            contentEvents.MovingContent -= ContentEvents_MovingContent;
            contentEvents.CreatedContent -= CreatedContent;
        }

        private void Instance_PublishingPage(object sender, ContentEventArgs e)
        {
            if (e.Content is ISearchableContent)
            {
                _cacheService.Value.Get("searchable", () => DateTime.Now.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void ContentEvents_MovingContent(object sender, ContentEventArgs e)
        {
            if (e.Content != null && e.TargetLink.ID == 2)
            {
                ContentIndexer.Instance.Delete(e.Content);
            }
        }

        private void CreatedContent(object sender, ContentEventArgs e)
        {
            if (e.Content is ImageData imageData)
            {
                SetImagePublished(imageData);
            }

            if (e.Content is MediaData mediaData)
            {
                SetMediaPublished(mediaData);
            }
        }

        private static void SetImagePublished(ImageData content)
        {
            if (content.IsPendingPublish)
            {
                var clone = content.CreateWritableClone() as ImageData;
                var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                contentRepository.Save(clone, SaveAction.Publish, AccessLevel.NoAccess);
            }
        }

        private static void SetMediaPublished(MediaData content)
        {
            if (content.IsPendingPublish)
            {
                var clone = content.CreateWritableClone() as MediaData;
                var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                contentRepository.Save(clone, SaveAction.Publish, AccessLevel.NoAccess);
            }
        }
    }
}