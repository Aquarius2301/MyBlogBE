using Microsoft.Extensions.Localization;
using WebApi.Resources;

namespace WebApi.Services.Implementations
{
    public class LanguageService : ILanguageService
    {
        private readonly IStringLocalizer _localizer;

        public LanguageService(IStringLocalizerFactory factory, ILogger<LanguageService> logger)
        {
            var type = typeof(SharedResources);
            _localizer = factory.Create(type);
        }

        public string Get(string key)
        {
            var localizedString = _localizer[key];

            return localizedString.Value;
        }

        public IDictionary<string, string> GetArray(string[] keys)
        {
            var result = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                result[key] = Get(key);
            }
            return result;
        }
    }
}
