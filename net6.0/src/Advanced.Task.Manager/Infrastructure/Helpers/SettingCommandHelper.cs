namespace Advanced.Task.Manager.Infrastructure.Helpers
{
    public static class SettingCommandHelper
    {
        public static string Fade(this string text)
        {
            return !string.IsNullOrEmpty(text) ? string.Format("<span class='epi-changeapproval-faded'>{0}</span>", text) : "";
        }

        public static string Bold(this string text)
        {
            return !string.IsNullOrEmpty(text) ? string.Format("<strong>{0}</strong>", text) : "";
        }

        public static string Strikethrough(this string text)
        {
            return !string.IsNullOrEmpty(text) ? string.Format("<del>{0}</del>", text) : "";
        }
    }
}