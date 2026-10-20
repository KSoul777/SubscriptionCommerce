using System.Xml.Linq;
using AwesomeAssertions;

namespace SubscriptionCommerce.ArchitectureTests.Layers;

public class ProjectReferenceTests
{
    private const string Prefix = "SubscriptionCommerce.";

    private static readonly Dictionary<string, string[]> AllowedByLayer = new()
    {
        ["Common.Domain"] = [],
        ["Common.Application"] = ["Common.Domain"],
        ["Common.Infrastructure"] = ["Common.Application"],
        ["Common.Presentation"] = ["Common.Domain"],
        ["Domain"] = ["Common.Domain"],
        ["Application"] = ["Domain", "Common.Application"],
        ["Infrastructure"] = ["Application", "Common.Infrastructure"],
        ["Presentation"] = ["Application", "Common.Presentation"]
    };

    [Fact]
    public void ProjectReferences_Should_FollowTheLayerGraph()
    {
        SourceProject[] projects = SourceProjects();

        string[] violations = projects
            .SelectMany(p => p.References.Where(r => !IsAllowed(p.Name, r)).Select(r => $"{p.Name} -> {r}"))
            .ToArray();

        projects.Select(p => p.Name).Should().Contain(
            ["SubscriptionCommerce.Api", "SubscriptionCommerce.Modules.Pricing.Infrastructure", "SubscriptionCommerce.Modules.Customers.Infrastructure"]);
        violations.Should().BeEmpty();
    }

    private static bool IsAllowed(string project, string reference)
    {
        if (project == "SubscriptionCommerce.Api")
        {
            return reference.StartsWith(Prefix + "Modules.", StringComparison.Ordinal) &&
                   (reference.EndsWith(".Infrastructure", StringComparison.Ordinal) ||
                    reference.EndsWith(".Presentation", StringComparison.Ordinal));
        }

        (string? module, string layer) = Split(project);
        string key = module is null ? $"Common.{layer}" : layer;

        return AllowedByLayer.TryGetValue(key, out string[]? allowed) &&
               allowed.Any(target => reference == FullName(module, target));
    }

    private static (string? Module, string Layer) Split(string project)
    {
        string[] parts = project[Prefix.Length..].Split('.');

        return parts[0] == "Modules" ? (parts[1], parts[2]) : (null, parts[1]);
    }

    private static string FullName(string? module, string target) =>
        target.StartsWith("Common.", StringComparison.Ordinal)
            ? Prefix + target
            : $"{Prefix}Modules.{module}.{target}";

    private static SourceProject[] SourceProjects()
    {
        DirectoryInfo root = RepositoryRoot();

        return Directory.EnumerateFiles(Path.Combine(root.FullName, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => !Path.GetFileNameWithoutExtension(path).EndsWith("Tests", StringComparison.Ordinal))
            .Select(path => new SourceProject(
                Path.GetFileNameWithoutExtension(path),
                XDocument.Load(path).Descendants("ProjectReference")
                    .Select(r => Path.GetFileNameWithoutExtension(r.Attribute("Include")!.Value.Replace('\\', '/')))
                    .ToArray()))
            .ToArray();
    }

    private static DirectoryInfo RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SubscriptionCommerce.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new InvalidOperationException("SubscriptionCommerce.slnx not found above the test output");
    }

    private sealed record SourceProject(string Name, string[] References);
}
