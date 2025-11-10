namespace WebApi.Services;

/// <summary>
/// Language Service Interface
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Get localized string by key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string Get(string key);
}
