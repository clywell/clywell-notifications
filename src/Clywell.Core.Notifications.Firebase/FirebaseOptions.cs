namespace Clywell.Core.Notifications.Firebase;

/// <summary>
/// Configuration options for the Firebase Push provider.
/// </summary>
public sealed class FirebaseOptions
{
    internal string? CredentialFilePath { get; private set; }
    internal string? CredentialJson { get; private set; }
    internal string? ProjectId { get; private set; }

    /// <summary>
    /// Loads the Google service account credential from a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the JSON key file.</param>
    public FirebaseOptions UseCredentialFile(string filePath)
    {
        CredentialFilePath = filePath;
        return this;
    }

    /// <summary>
    /// Loads the Google service account credential from a JSON string.
    /// </summary>
    /// <param name="json">The raw JSON string of the credentials.</param>
    public FirebaseOptions UseCredentialJson(string json)
    {
        CredentialJson = json;
        return this;
    }

    /// <summary>
    /// Sets an explicit Google Cloud project ID.
    /// </summary>
    public FirebaseOptions UseProjectId(string projectId)
    {
        ProjectId = projectId;
        return this;
    }
}
