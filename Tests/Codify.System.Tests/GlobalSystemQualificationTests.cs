namespace Codify.System.Tests;

public class GlobalSystemQualificationTests
{
    private static readonly string[] BclIdentifiers =
    [
        "ArgumentException",
        "ArgumentNullException",
        "ArgumentOutOfRangeException",
        "Action",
        "Activator",
        "CallerMemberName",
        "ConcurrentDictionary",
        "CultureInfo",
        "Dictionary",
        "EqualityComparer",
        "EventArgs",
        "EventHandler",
        "Exception",
        "Func",
        "ICollection",
        "IDictionary",
        "IDisposable",
        "IEnumerable",
        "IEnumerator",
        "INotifyPropertyChanged",
        "IList",
        "KeyValuePair",
        "NotifyCollectionChangedAction",
        "NotifyCollectionChangedEventArgs",
        "ObservableCollection",
        "PropertyChangedEventArgs",
        "PropertyChangedEventHandler",
        "StringComparison",
        "Task",
        "Type"
    ];

    [Fact]
    public void ProductionSourcesUnderCodifyNamespacesQualifyBclSystemReferences()
    {
        var repositoryRoot = FindRepositoryRoot();
        var offenders = new global::System.Collections.Generic.List<string>();

        foreach (var projectDirectory in new[] { "System", "System.Windows" })
        {
            var sourceDirectory = global::System.IO.Path.Combine(repositoryRoot, projectDirectory);
            foreach (var file in global::System.IO.Directory.EnumerateFiles(
                         sourceDirectory,
                         "*.cs",
                         global::System.IO.SearchOption.AllDirectories))
            {
                if (IsGeneratedOutput(file))
                {
                    continue;
                }

                InspectFile(repositoryRoot, file, offenders);
            }
        }

        Assert.Empty(offenders);
    }

    private static void InspectFile(
        string repositoryRoot,
        string file,
        global::System.Collections.Generic.List<string> offenders)
    {
        var lineNumber = 0;
        foreach (var line in global::System.IO.File.ReadLines(file))
        {
            lineNumber++;
            var code = StripTrailingLineComment(line);
            var trimmed = code.TrimStart();

            if (trimmed.Length == 0 || trimmed.StartsWith("///", global::System.StringComparison.Ordinal))
            {
                continue;
            }

            if (trimmed.StartsWith("using System", global::System.StringComparison.Ordinal))
            {
                AddOffender(repositoryRoot, file, lineNumber, 1, trimmed, offenders);
            }

            foreach (var identifier in BclIdentifiers)
            {
                AddUnqualifiedIdentifierOffenders(repositoryRoot, file, lineNumber, code, identifier, offenders);
            }
        }
    }

    private static void AddUnqualifiedIdentifierOffenders(
        string repositoryRoot,
        string file,
        int lineNumber,
        string code,
        string identifier,
        global::System.Collections.Generic.List<string> offenders)
    {
        var searchIndex = 0;
        while (searchIndex < code.Length)
        {
            var matchIndex = code.IndexOf(identifier, searchIndex, global::System.StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return;
            }

            searchIndex = matchIndex + identifier.Length;
            if (!HasIdentifierBoundary(code, matchIndex - 1) ||
                !HasIdentifierBoundary(code, matchIndex + identifier.Length))
            {
                continue;
            }

            if (IsMemberAccess(code, matchIndex))
            {
                continue;
            }

            if (IsGlobalSystemQualified(code, matchIndex))
            {
                continue;
            }

            AddOffender(repositoryRoot, file, lineNumber, matchIndex + 1, identifier, offenders);
        }
    }

    private static void AddOffender(
        string repositoryRoot,
        string file,
        int lineNumber,
        int column,
        string value,
        global::System.Collections.Generic.List<string> offenders)
    {
        var relativePath = global::System.IO.Path.GetRelativePath(repositoryRoot, file);
        offenders.Add($"{relativePath}:{lineNumber}:{column}: {value}");
    }

    private static bool IsGeneratedOutput(string file)
    {
        var separator = global::System.IO.Path.DirectorySeparatorChar;
        var binSegment = $"{separator}bin{separator}";
        var objSegment = $"{separator}obj{separator}";

        return file.Contains(binSegment,
                   global::System.StringComparison.OrdinalIgnoreCase) ||
               file.Contains(objSegment,
                   global::System.StringComparison.OrdinalIgnoreCase);
    }

    private static string StripTrailingLineComment(string line)
    {
        var commentIndex = line.IndexOf("//", global::System.StringComparison.Ordinal);
        return commentIndex < 0 ? line : line[..commentIndex];
    }

    private static bool HasIdentifierBoundary(string text, int index)
    {
        return index < 0 ||
               index >= text.Length ||
               (!char.IsLetterOrDigit(text[index]) && text[index] != '_');
    }

    private static bool IsMemberAccess(string code, int matchIndex)
    {
        var previousIndex = matchIndex - 1;
        while (previousIndex >= 0 && char.IsWhiteSpace(code[previousIndex]))
        {
            previousIndex--;
        }

        return previousIndex >= 0 && code[previousIndex] == '.';
    }

    private static bool IsGlobalSystemQualified(string code, int matchIndex)
    {
        const string qualifierRoot = "global::System.";

        var qualifierEnd = matchIndex - 1;
        while (qualifierEnd >= 0 && char.IsWhiteSpace(code[qualifierEnd]))
        {
            qualifierEnd--;
        }

        var qualifierStart = qualifierEnd;
        while (qualifierStart >= 0)
        {
            var character = code[qualifierStart];
            if (!char.IsLetterOrDigit(character) && character != '_' && character != ':' && character != '.')
            {
                break;
            }

            qualifierStart--;
        }

        var qualifier = code[(qualifierStart + 1)..(qualifierEnd + 1)];
        return qualifier.StartsWith(qualifierRoot, global::System.StringComparison.Ordinal);
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
