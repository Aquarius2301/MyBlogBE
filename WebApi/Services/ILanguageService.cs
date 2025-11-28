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

    /// <summary>
    /// Get localized strings for an array of keys.
    /// </summary>
    /// <param name="keys">The array of keys to retrieve localized strings for.</param>
    /// <returns>
    /// A dictionary containing the localized strings for the specified keys.
    /// </returns>
    IDictionary<string, string> GetArray(string[] keys);
}
