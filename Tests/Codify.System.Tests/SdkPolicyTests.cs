namespace Codify.System.Tests;

public class SdkPolicyTests
{
    private static readonly string[] TestOnlyTransitivePackages =
    [
        "Microsoft.ApplicationInsights",
        "Microsoft.Bcl.AsyncInterfaces",
        "Microsoft.Testing.Extensions.Telemetry",
        "Microsoft.Testing.Extensions.TrxReport.Abstractions",
        "Microsoft.Testing.Platform",
        "Microsoft.Testing.Platform.MSBuild",
        "Newtonsoft.Json"
    ];

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

    [Fact]
    public void RepositoryDocumentsTestOnlyTransitivePackageFreshnessPolicy()
    {
        var repositoryRoot = FindRepositoryRoot();
        var readmePath = global::System.IO.Path.Combine(repositoryRoot, "README.md");
        var buildPropsPath = global::System.IO.Path.Combine(repositoryRoot, "Directory.Build.props");
        var readme = global::System.IO.File.ReadAllText(readmePath);
        var buildProps = global::System.Xml.Linq.XDocument.Load(buildPropsPath);

        var directPackageReferences = buildProps
            .Descendants("PackageReference")
            .Select(static element => (string?)element.Attribute("Include"))
            .Where(static package => !global::System.String.IsNullOrWhiteSpace(package))
            .ToHashSet(global::System.StringComparer.OrdinalIgnoreCase);

        foreach (var package in TestOnlyTransitivePackages)
        {
            Assert.DoesNotContain(package, directPackageReferences);
        }

        Assert.Contains(
            "Test-only transitive package freshness",
            readme,
            global::System.StringComparison.Ordinal);
        Assert.Contains(
            "Direct test package references are kept on their latest stable versions.",
            readme,
            global::System.StringComparison.Ordinal);
        Assert.Contains(
            "Outdated test-only transitive packages are left to the direct test packages that own them when those direct packages are current, non-vulnerable, and non-deprecated.",
            readme,
            global::System.StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("System", "net10.0")]
    [InlineData("System.Windows", "net10.0-windows")]
    public void LibraryProjectsTargetLatestLtsOnlyWithoutCompatibilityBaselines(
        string projectName,
        string targetFramework)
    {
        var repositoryRoot = FindRepositoryRoot();
        var buildPropsPath = global::System.IO.Path.Combine(repositoryRoot, "Directory.Build.props");
        var buildProps = global::System.Xml.Linq.XDocument.Load(buildPropsPath);

        var propertyGroup = Assert.Single(
            buildProps.Root!.Elements("PropertyGroup"),
            element =>
                ((string?)element.Attribute("Condition"))?.Contains(
                    $"$(MSBuildProjectName)' == '{projectName}'",
                    global::System.StringComparison.Ordinal) == true &&
                element.Element("TargetFramework") != null);

        Assert.Equal(targetFramework, (string?)propertyGroup.Element("TargetFramework"));
        Assert.Empty(buildProps.Descendants("TargetFrameworks"));
        Assert.Empty(buildProps.Descendants("EnablePackageValidation"));
        Assert.Empty(buildProps.Descendants("PackageValidationBaselineVersion"));
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
