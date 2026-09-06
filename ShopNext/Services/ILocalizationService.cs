using System.Collections.Generic;

namespace ShopNext.Services
{
    public class LanguageOption
    {
        public string Code { get; set; } = "en";
        public string Name { get; set; } = "English";
        public string NativeName { get; set; } = "English";
        public string Flag { get; set; } = "🇬🇧";
        public bool IsRtl { get; set; } = false;
    }

    public interface ILocalizationService
    {
        List<LanguageOption> GetSupportedLanguages();
        string GetString(string key, string? culture = null);
        string GetString(string key, string culture, params object[] args);
        string GetCurrentLanguage(string? cookieLanguage = null);
        Dictionary<string, string> GetAllStringsForCulture(string culture);
    }
}
