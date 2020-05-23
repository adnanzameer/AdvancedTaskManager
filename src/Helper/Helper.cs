using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace AdvancedTask.Helper
{
    public static class Helper
    {
        public static bool CanUserPublish<T>(this T content) where T : IContent
        {
            IList<string> roles;
            using (var store = new UserStore<ApplicationUser>(new ApplicationDbContext<ApplicationUser>("EPiServerDB")))
            {
                var user = store.FindByNameAsync(PrincipalInfo.CurrentPrincipal.Identity.Name).GetAwaiter().GetResult();
                roles = GetUserToRoles(store, user);
            }
            return content.RoleHasAccess(roles.ToArray(), AccessLevel.Publish);
        }

        private static bool RoleHasAccess<T>(this T content, string[] roles, AccessLevel accessLevel) where T : IContent
        {
            var securedContent = content as ISecurable;
            if (securedContent != null)
            {
                var descriptor = securedContent.GetSecurityDescriptor();
                var identity = new GenericIdentity("doesn't matter");
                var principal = new GenericPrincipal(identity, roles);
                return descriptor.HasAccess(principal, accessLevel);
            }
            return false;
        }

        private static IList<string> GetUserToRoles(UserStore<ApplicationUser> store, ApplicationUser user)
        {
            IUserRoleStore<ApplicationUser, string> userRoleStore = store;
            using (new RoleStore<IdentityRole>(new ApplicationDbContext<ApplicationUser>("EPiServerDB")))
            {
                return userRoleStore.GetRolesAsync(user).GetAwaiter().GetResult();
            }
        }

        public static IEnumerable<DateTime> GetDaysInRange(this DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return Enumerable.Empty<DateTime>();

            return Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                .Select(offset => startDate.AddDays(offset))
                .ToArray();
        }


        public static int CountDaysInRange(this DateTime startDate, DateTime endDate)
        {
            var dates = startDate.GetDaysInRange(endDate);

            return dates.Count();
        }

        public static T ToObject<T>(this string value) where T : class
        {
            if (string.IsNullOrEmpty(value))
                return default(T);
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }
    }
}
