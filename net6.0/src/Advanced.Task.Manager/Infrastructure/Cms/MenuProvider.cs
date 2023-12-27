using System.Collections.Generic;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;

namespace Advanced.Task.Manager.Infrastructure.Cms
{
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            //var url = Paths.ToResource(GetType(), "container");
            var url = Paths.ToResource("Advanced Task", "container");

            var link = new UrlMenuItem(
                "Adv. Task Manager",
                MenuPaths.Global + "/cms/advancedtask",
                url)
            {
                SortIndex = 100,
                AuthorizationPolicy = Constants.PolicyName
            };

            return new List<MenuItem>
            {
                link
            };
        }
    }
}
