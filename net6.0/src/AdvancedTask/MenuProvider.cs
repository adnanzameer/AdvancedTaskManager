using System.Collections.Generic;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;

namespace AdvancedTask
{
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var url = Paths.ToResource(GetType(), "container");

            var link = new UrlMenuItem(
                "Advanced Task Manager",
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
