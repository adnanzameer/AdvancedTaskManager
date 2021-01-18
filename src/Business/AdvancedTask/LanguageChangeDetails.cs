using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Helper;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Shell.Web;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class LanguageChangeDetails
    {
        private readonly LocalizationService _localizationService;

        private readonly string ArrowSeparator = " > ";
        private readonly string CommaSeparator = ", ";
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly IContentLanguageSettingsHandler _contentLanguageSettingsHandler;
        private readonly IContentRepository _contentRepository;

        public LanguageChangeDetails(ILanguageBranchRepository languageBranchRepository, LocalizationService localizationService, IContentLanguageSettingsHandler contentLanguageSettingsHandler, IContentRepository contentRepository)
        {
            _languageBranchRepository = languageBranchRepository;
            _localizationService = localizationService;
            _contentLanguageSettingsHandler = contentLanguageSettingsHandler;
            _contentRepository = contentRepository;
        }


        #region Language

        public IEnumerable<IContentChangeDetails> GetLanguageChangeDetails(ChangeTaskViewModel command)
        {
            var contentChangeDetailsList = new List<IContentChangeDetails>();
            var parent = GetParent(command.AppliedOnContentLink);
            var settingsFromJson1 = GetContentLanguageSettingsFromJson(command.CurrentSettingsJson);
            var settingsFromJson2 = GetContentLanguageSettingsFromJson(command.NewSettingsJson);
            var flag = !ContentReference.IsNullOrEmpty(parent);
            var dictionary = ((settingsFromJson2 == null ? 1 : settingsFromJson2.Count == 0 ? 1 : 0) & (flag ? 1 : 0)) != 0 ? GetLanguageSettings(parent) : settingsFromJson2;
            if (flag)
            {
                var str1 = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/yes");
                var str2 = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/no");
                contentChangeDetailsList.Add((IContentChangeDetails)new ContentChangeDetails()
                {
                    Name = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/inheritsettingsfromtheparentpage"),
                    OldValue = settingsFromJson1.Values.Any((Func<ContentLanguageSetting, bool>)(st => st.DefinedOnContent.CompareToIgnoreWorkID(command.AppliedOnContentLink))) ? str2 : str1,
                    NewValue = dictionary != null && dictionary.Values.Any((Func<ContentLanguageSetting, bool>)(st => st.DefinedOnContent.CompareToIgnoreWorkID(command.AppliedOnContentLink))) ? str2 : str1
                });
            }
            var currentLangs = settingsFromJson1.Where<KeyValuePair<string, ContentLanguageSetting>>((Func<KeyValuePair<string, ContentLanguageSetting>, bool>)(s => s.Value.IsActive)).Select<KeyValuePair<string, ContentLanguageSetting>, string>((Func<KeyValuePair<string, ContentLanguageSetting>, string>)(s => s.Value.LanguageBranch));
            var newLangs = dictionary.Where((Func<KeyValuePair<string, ContentLanguageSetting>, bool>)(s => s.Value.IsActive)).Select<KeyValuePair<string, ContentLanguageSetting>, string>((Func<KeyValuePair<string, ContentLanguageSetting>, string>)(s => s.Value.LanguageBranch));
            contentChangeDetailsList.Add(new ContentChangeDetails()
            {
                Name = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/availableLanguages"),
                OldValue = FormatLanguageSettingsChange(currentLangs, newLangs, true),
                NewValue = FormatLanguageSettingsChange(currentLangs, newLangs, false)
            });
            var str3 = OldFallbackLanguagesToString(settingsFromJson1, dictionary);
            var str4 = NewFallbackLanguagesToString(settingsFromJson1, dictionary);
            contentChangeDetailsList.Add((IContentChangeDetails)new ContentChangeDetails()
            {
                Name = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/fallbacklanguages"),
                OldValue = str3,
                NewValue = str4
            });
            var str5 = OldReplacementLanguagesToString(settingsFromJson1, dictionary);
            var str6 = NewReplacementLanguagesToString(settingsFromJson1, dictionary);
            contentChangeDetailsList.Add(new ContentChangeDetails()
            {
                Name = _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/replacementlanguages"),
                OldValue = str5,
                NewValue = str6
            });
            return contentChangeDetailsList;
        }

        private ContentReference GetParent(ContentReference contentLink)
        {
            var content = _contentRepository.Get<IContent>(contentLink, LanguageSelector.AutoDetect(true));
            if (content == null)
                return null;
            var source = _contentLanguageSettingsHandler.Get(content.ParentLink);
            return source?.FirstOrDefault()?.DefinedOnContent;
        }

        private string GetLanguageName(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return string.Empty;
            var languageBranch = _languageBranchRepository.Load(languageCode);
            return languageBranch == null ? string.Empty : languageBranch.Name;
        }

        private string StringifyLanguages(IEnumerable<string> languages, string separator)
        {
            return languages.Aggregate<string, string>(string.Empty, (Func<string, string, string>)((workingStr, next) => string.Format("{0}{1}{2}", workingStr, separator, GetLanguageName(next)))).TrimStart(separator.ToCharArray());
        }

        private string FormatLanguageSettingsChange(
          IEnumerable<string> currentLangs,
          IEnumerable<string> newLangs,
          bool original)
        {
            var str = string.Empty;
            if (!currentLangs.Except<string>(newLangs).Any<string>() && !newLangs.Except<string>(currentLangs).Any<string>())
                str = StringifyLanguages(currentLangs, CommaSeparator).Fade();
            else if (original)
            {
                str = StringifyLanguages(currentLangs, CommaSeparator);
            }
            else
            {
                foreach (var currentLang in currentLangs)
                {
                    var item = currentLang;
                    var languageName = GetLanguageName(item);
                    str = newLangs.Any<string>((Func<string, bool>)(lang => lang.Equals(item, StringComparison.InvariantCultureIgnoreCase))) ? str + string.Format("{0}{1}", languageName, CommaSeparator) : str + string.Format("{0}{1}", languageName.Strikethrough(), CommaSeparator);
                }
                foreach (var newLang in newLangs)
                {
                    var item = newLang;
                    var languageName = GetLanguageName(item);
                    if (!string.IsNullOrWhiteSpace(languageName) && !currentLangs.Any<string>((Func<string, bool>)(lang => lang.Equals(item, StringComparison.InvariantCultureIgnoreCase))))
                        str += string.Format("{0}{1}", languageName.Bold(), CommaSeparator);
                }
            }
            return str.TrimEnd(CommaSeparator);
        }

        private static List<string> GetFallbackLanguages(
          string visitorLanguage,
          IDictionary<string, ContentLanguageSetting> settings)
        {
            return settings == null || !settings.ContainsKey(visitorLanguage) || settings[visitorLanguage].LanguageBranchFallback == null ? ((IEnumerable<string>)new string[0]).ToList<string>() : ((IEnumerable<string>)settings[visitorLanguage].LanguageBranchFallback).ToList<string>();
        }

        private string OldFallbackLanguagesToString(
          IDictionary<string, ContentLanguageSetting> oldSettings,
          IDictionary<string, ContentLanguageSetting> newSettings)
        {
            var stringBuilder = new StringBuilder();
            foreach (var key in (IEnumerable<string>)oldSettings.Keys)
            {
                var fallbackLanguages1 = GetFallbackLanguages(key, oldSettings);
                var fallbackLanguages2 = GetFallbackLanguages(key, newSettings);
                if ((fallbackLanguages1.Count != 0 || fallbackLanguages2.Count != 0) && (fallbackLanguages1.Any<string>((Func<string, bool>)(item => !string.IsNullOrWhiteSpace(GetLanguageName(item)))) || fallbackLanguages2.Any<string>((Func<string, bool>)(item => !string.IsNullOrWhiteSpace(GetLanguageName(item))))))
                {
                    var languageName = GetLanguageName(key);
                    if (fallbackLanguages1.Count > 0)
                    {
                        var str1 = StringifyLanguages((IEnumerable<string>)fallbackLanguages1, ArrowSeparator);
                        var str2 = fallbackLanguages1.SequenceEqual<string>((IEnumerable<string>)fallbackLanguages2) ? string.Format("{0}{1}{2}", languageName, ArrowSeparator, str1).Fade() : string.Format("{0}{1}{2}", languageName, ArrowSeparator, str1);
                        stringBuilder.Append(str2);
                    }
                    else
                        stringBuilder.Append(string.Format("{0}{1}{2}", languageName, ArrowSeparator, _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/none")));
                    stringBuilder.Append("</br>");
                }
            }
            return stringBuilder.ToString().TrimEnd("</br>");
        }

        private string NewFallbackLanguagesToString(
          IDictionary<string, ContentLanguageSetting> oldSettings,
          IDictionary<string, ContentLanguageSetting> newSettings)
        {
            var stringBuilder = new StringBuilder();
            foreach (var key in (IEnumerable<string>)newSettings.Keys)
            {
                var fallbackLanguages1 = GetFallbackLanguages(key, newSettings);
                var fallbackLanguages2 = GetFallbackLanguages(key, oldSettings);
                if (fallbackLanguages2.Count > 0 || fallbackLanguages1.Count > 0)
                {
                    var languageName1 = GetLanguageName(key);
                    if (!string.IsNullOrWhiteSpace(languageName1))
                    {
                        if (fallbackLanguages1.SequenceEqual<string>((IEnumerable<string>)fallbackLanguages2))
                        {
                            var str = StringifyLanguages((IEnumerable<string>)fallbackLanguages1, ArrowSeparator);
                            stringBuilder.Append(string.Format("{0}{1}{2}", languageName1, ArrowSeparator, str).Fade());
                        }
                        else if (fallbackLanguages1.Count == 0 && fallbackLanguages2.Count > 0)
                        {
                            var str = StringifyLanguages((IEnumerable<string>)fallbackLanguages2, ArrowSeparator);
                            stringBuilder.Append(string.Format("{0}{1}{2}", languageName1, ArrowSeparator, str).Strikethrough());
                        }
                        else if (fallbackLanguages1.Count > 0)
                        {
                            if ((fallbackLanguages2.Count != 0 || fallbackLanguages1.Count != 0) && (fallbackLanguages2.Any<string>((Func<string, bool>)(item => !string.IsNullOrWhiteSpace(GetLanguageName(item)))) || fallbackLanguages1.Any<string>((Func<string, bool>)(item => !string.IsNullOrWhiteSpace(GetLanguageName(item))))))
                            {
                                stringBuilder.Append(languageName1);
                                foreach (var languageCode in fallbackLanguages2)
                                {
                                    var str = fallbackLanguages1.Contains(languageCode) ? string.Format("{0}{1}", ArrowSeparator, GetLanguageName(languageCode)) : string.Format("{0}{1}", ArrowSeparator, GetLanguageName(languageCode).Strikethrough());
                                    stringBuilder.Append(str);
                                }
                                foreach (var languageCode in fallbackLanguages1)
                                {
                                    var languageName2 = GetLanguageName(languageCode);
                                    if (!string.IsNullOrWhiteSpace(languageName2) && !fallbackLanguages2.Contains(languageCode))
                                        stringBuilder.Append(string.Format("{0}{1}", ArrowSeparator, languageName2.Bold()));
                                }
                            }
                            else
                                continue;
                        }
                        stringBuilder.Append("</br>");
                    }
                }
            }
            return stringBuilder.ToString().TrimEnd("</br>");
        }

        private string GetReplacementLanguage(
          string visitorLanguage,
          IDictionary<string, ContentLanguageSetting> settings)
        {
            return settings == null || !settings.ContainsKey(visitorLanguage) || settings[visitorLanguage].ReplacementLanguageBranch == null ? string.Empty : GetLanguageName(settings[visitorLanguage].ReplacementLanguageBranch);
        }

        private string OldReplacementLanguagesToString(
          IDictionary<string, ContentLanguageSetting> oldSettings,
          IDictionary<string, ContentLanguageSetting> newSettings)
        {
            var stringBuilder = new StringBuilder();
            foreach (var key in (IEnumerable<string>)oldSettings.Keys)
            {
                var replacementLanguage1 = GetReplacementLanguage(key, oldSettings);
                var replacementLanguage2 = GetReplacementLanguage(key, newSettings);
                if (!string.IsNullOrEmpty(replacementLanguage1) || !string.IsNullOrEmpty(replacementLanguage2))
                {
                    var languageName = GetLanguageName(key);
                    var str = replacementLanguage1 != null && replacementLanguage1.Equals(replacementLanguage2) ? string.Format("{0}{1}{2}", languageName, ArrowSeparator, replacementLanguage1).Fade() : string.Format("{0}{1}{2}", languageName, ArrowSeparator, !string.IsNullOrEmpty(replacementLanguage1) ? replacementLanguage1 : _localizationService.GetString("/gadget/changeapproval/languagesettingcommand/none"));
                    stringBuilder.Append(str);
                    stringBuilder.Append("</br>");
                }
            }
            return stringBuilder.ToString().TrimEnd("</br>");
        }

        private string NewReplacementLanguagesToString(
          IDictionary<string, ContentLanguageSetting> oldSettings,
          IDictionary<string, ContentLanguageSetting> newSettings)
        {
            var stringBuilder = new StringBuilder();
            foreach (var key in (IEnumerable<string>)newSettings.Keys)
            {
                var replacementLanguage1 = GetReplacementLanguage(key, newSettings);
                var replacementLanguage2 = GetReplacementLanguage(key, oldSettings);
                if (!string.IsNullOrEmpty(replacementLanguage1) || !string.IsNullOrEmpty(replacementLanguage2))
                {
                    var languageName = GetLanguageName(key);
                    if (!string.IsNullOrWhiteSpace(languageName))
                    {
                        if (replacementLanguage1 != null && replacementLanguage1.Equals(replacementLanguage2))
                            stringBuilder.Append(string.Format("{0}{1}{2}", languageName, ArrowSeparator, replacementLanguage1).Fade());
                        else if (string.IsNullOrEmpty(replacementLanguage1))
                            stringBuilder.Append(string.Format("{0}{1}{2}", languageName, ArrowSeparator, replacementLanguage2).Strikethrough());
                        else
                            stringBuilder.Append(string.Format("{0}{1}{2}", languageName, ArrowSeparator, replacementLanguage1.Bold()));
                    }
                    stringBuilder.Append("</br>");
                }
            }
            return stringBuilder.ToString().TrimEnd("</br>");
        }

        private IDictionary<string, ContentLanguageSetting> GetContentLanguageSettingsFromJson(
          string json)
        {
            return string.IsNullOrEmpty(json) ? (IDictionary<string, ContentLanguageSetting>)new Dictionary<string, ContentLanguageSetting>() : json.ToObject<IDictionary<string, ContentLanguageSetting>>();
        }

        private IDictionary<string, ContentLanguageSetting> GetLanguageSettings(
            ContentReference contentLink)
        {
            var dictionary = new Dictionary<string, ContentLanguageSetting>();
            var languageBranchList = _languageBranchRepository.ListEnabled();
            var source = _contentLanguageSettingsHandler.Get(contentLink);
            if (source.Any<ContentLanguageSetting>())
            {
                foreach (var contentLanguageSetting in source)
                {
                    foreach (var languageBranch in (IEnumerable<LanguageBranch>)languageBranchList)
                    {
                        if (string.Compare(contentLanguageSetting.LanguageBranch, languageBranch.LanguageID, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            dictionary.Add(languageBranch.LanguageID, contentLanguageSetting);
                            break;
                        }
                    }
                }
            }
            return dictionary;
        }
        #endregion
    }
}
