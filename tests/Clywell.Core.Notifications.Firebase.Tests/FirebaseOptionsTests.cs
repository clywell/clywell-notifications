using Clywell.Core.Notifications.Firebase;
using System;
using Xunit;

namespace Clywell.Core.Notifications.Firebase.Tests;

public class FirebaseOptionsTests
{
    [Fact]
    public void UseCredentialFile_SetsCredentialFilePath()
    {
        // Arrange
        var options = new FirebaseOptions();
        var path = "/tmp/fake-key.json";

        // Act
        options.UseCredentialFile(path);

        // Assert
        Assert.Equal(path, options.CredentialFilePath);
        Assert.Null(options.CredentialJson);
    }

    [Fact]
    public void UseCredentialJson_SetsCredentialJson()
    {
        // Arrange
        var options = new FirebaseOptions();
        var json = "{\"type\": \"service_account\"}";

        // Act
        options.UseCredentialJson(json);

        // Assert
        Assert.Equal(json, options.CredentialJson);
        Assert.Null(options.CredentialFilePath);
    }

    [Fact]
    public void UseProjectId_SetsProjectId()
    {
        // Arrange
        var options = new FirebaseOptions();
        var projectId = "my-test-project";

        // Act
        options.UseProjectId(projectId);

        // Assert
        Assert.Equal(projectId, options.ProjectId);
    }
}
