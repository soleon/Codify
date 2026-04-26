namespace Codify.System.Tests;

public class SdkPolicyTests
{
    [Fact]
    public void RepositoryDocumentsFloatingSdkPolicy()
    {
        var repositoryRoot = FindRepositoryRoot();
        var globalJsonPath = global::System.IO.Path.Combine(repositoryRoot, "global.json");
        var readmePath = global::System.IO.Path.Combine(repositoryRoot, "README.md");
        var readme = global::System.IO.File.ReadAllText(readmePath);

        Assert.False(global::System.IO.File.Exists(globalJsonPath));
        Assert.Contains(
            "The repository intentionally does not include a `global.json` SDK pin.",
            readme,
            global::System.StringComparison.Ordinal);
        Assert.Contains(
            "floats to the latest installed stable .NET SDK",
            readme,
            global::System.StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new global::System.IO.DirectoryInfo(global::System.AppContext.BaseDirectory);
        while (directory != null)
        {
            if (global::System.IO.File.Exists(global::System.IO.Path.Combine(directory.FullName, "Codify.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new global::System.InvalidOperationException("Could not locate the repository root.");
    }
}
