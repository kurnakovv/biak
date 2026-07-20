// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Helpers.Baseline;

/// <summary>
/// Shared helper for synchronizing editorconfig baseline sections.
/// </summary>
public static class BaselineSyncEditorconfigHelper
{
    private static readonly StringComparer s_pathComparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Returns all unique identifiers that appear in baseline blocks.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="tryGetIdentifier">Function that extracts baseline identifier from a baseline setting line.</param>
    /// <returns>Unique baseline identifiers.</returns>
    public static HashSet<string> GetIdentifiers(
        string content,
        Func<string, string?> tryGetIdentifier)
    {
        string newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(new[] { newline }, StringSplitOptions.None);

        return EnumerateBlocks(lines, tryGetIdentifier)
            .Select(x => x.Identifier)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes baseline blocks whose identifiers are not in <paramref name="identifiersToKeep"/>,
    /// and trims files per block to still-active files when <paramref name="activeFilesByIdentifier"/> is provided.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="identifiersToKeep">Identifiers to keep in baseline.</param>
    /// <param name="tryGetIdentifier">Function that extracts baseline identifier from a baseline setting line.</param>
    /// <param name="activeFilesByIdentifier">Optional map of active files per identifier.</param>
    /// <returns>Updated .editorconfig content.</returns>
    public static string RemoveFilters(
        string content,
        IReadOnlySet<string> identifiersToKeep,
        Func<string, string?> tryGetIdentifier,
        IReadOnlyDictionary<string, IReadOnlySet<string>>? activeFilesByIdentifier = null)
    {
        string newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(new[] { newline }, StringSplitOptions.None);

        Dictionary<int, BaselineBlock> blocksByHeaderIndex = EnumerateBlocks(lines, tryGetIdentifier)
            .ToDictionary(x => x.HeaderIndex);

        List<string> result = new(lines.Length);
        int i = 0;

        while (i < lines.Length)
        {
            string line = lines[i];

            if (blocksByHeaderIndex.TryGetValue(i, out BaselineBlock block))
            {
                if (!identifiersToKeep.Contains(block.Identifier))
                {
                    i = block.BlockEndIndex;
                    continue;
                }

                if (activeFilesByIdentifier is not null)
                {
                    IReadOnlySet<string> activeFilesForIdentifier = GetActiveFilesForIdentifier(activeFilesByIdentifier, block.Identifier);
                    string[] filesToKeep = block.SectionFiles
                        .Where(x => activeFilesForIdentifier.Contains(NormalizeEditorConfigPath(x)))
                        .ToArray();

                    if (filesToKeep.Length == 0)
                    {
                        i = block.BlockEndIndex;
                        continue;
                    }

                    if (filesToKeep.Length != block.SectionFiles.Length)
                    {
                        line = "[{" + string.Join(",", filesToKeep) + "}]";
                    }
                }
            }

            result.Add(line);
            i++;
        }

        return string.Join(newline, result);
    }

    /// <summary>
    /// Returns files and identifiers removed by synchronization.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="identifiersToKeep">Identifiers to keep in baseline.</param>
    /// <param name="tryGetIdentifier">Function that extracts baseline identifier from a baseline setting line.</param>
    /// <param name="activeFilesByIdentifier">Optional map of active files per identifier.</param>
    /// <returns>Map where key is section file path and value is removed identifiers.</returns>
    public static IReadOnlyDictionary<string, IReadOnlySet<string>> GetSynchronizedFiles(
        string content,
        IReadOnlySet<string> identifiersToKeep,
        Func<string, string?> tryGetIdentifier,
        IReadOnlyDictionary<string, IReadOnlySet<string>>? activeFilesByIdentifier = null)
    {
        string newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(new[] { newline }, StringSplitOptions.None);

        Dictionary<string, HashSet<string>> synchronizedFiles = new(StringComparer.OrdinalIgnoreCase);

        foreach (BaselineBlock block in EnumerateBlocks(lines, tryGetIdentifier))
        {
            if (!identifiersToKeep.Contains(block.Identifier))
            {
                foreach (string sectionFile in block.SectionFiles)
                {
                    AddSynchronizedFile(synchronizedFiles, sectionFile, block.Identifier);
                }
            }
            else if (activeFilesByIdentifier is not null)
            {
                IReadOnlySet<string> activeFilesForIdentifier = GetActiveFilesForIdentifier(activeFilesByIdentifier, block.Identifier);

                foreach (string sectionFile in block.SectionFiles.Where(
                    sectionFile => !activeFilesForIdentifier.Contains(NormalizeEditorConfigPath(sectionFile))
                ))
                {
                    AddSynchronizedFile(synchronizedFiles, sectionFile, block.Identifier);
                }
            }
        }

        return synchronizedFiles.ToDictionary(
            x => x.Key,
            x => (IReadOnlySet<string>)x.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }

    private static IEnumerable<BaselineBlock> EnumerateBlocks(string[] lines, Func<string, string?> tryGetIdentifier)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (!TryGetSectionFiles(lines[i], out string[] sectionFiles))
            {
                continue;
            }

            int diagIndex = i + 1;
            while (diagIndex < lines.Length && string.IsNullOrWhiteSpace(lines[diagIndex]))
            {
                diagIndex++;
            }

            if (diagIndex >= lines.Length)
            {
                continue;
            }

            string? identifier = tryGetIdentifier(lines[diagIndex]);
            if (string.IsNullOrWhiteSpace(identifier))
            {
                continue;
            }

            int blockEnd = diagIndex + 1;
            while (blockEnd < lines.Length && string.IsNullOrWhiteSpace(lines[blockEnd]))
            {
                blockEnd++;
            }

            int nextSectionIndex = blockEnd;
            while (nextSectionIndex < lines.Length && (string.IsNullOrWhiteSpace(lines[nextSectionIndex]) || IsCommentLine(lines[nextSectionIndex])))
            {
                nextSectionIndex++;
            }

            if (nextSectionIndex < lines.Length && !lines[nextSectionIndex].TrimStart().StartsWith('['))
            {
                continue;
            }

            yield return new BaselineBlock(
                HeaderIndex: i,
                BlockEndIndex: blockEnd,
                Identifier: identifier,
                SectionFiles: sectionFiles
            );
        }
    }

    private static bool TryGetSectionFiles(string line, out string[] sectionFiles)
    {
        sectionFiles = Array.Empty<string>();

        string trimmed = line.Trim();
        if (!trimmed.StartsWith("[{", StringComparison.Ordinal)
            || !trimmed.EndsWith("}]", StringComparison.Ordinal)
            || trimmed.Length < 4)
        {
            return false;
        }

        string filesContent = trimmed[2..^2];
        sectionFiles = filesContent
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return sectionFiles.Length > 0;
    }

    private static IReadOnlySet<string> GetActiveFilesForIdentifier(
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByIdentifier,
        string identifier)
    {
        return activeFilesByIdentifier.TryGetValue(identifier, out IReadOnlySet<string>? files)
            ? files
            : new HashSet<string>(s_pathComparer);
    }

    private static bool IsCommentLine(string line)
    {
        string trimmed = line.TrimStart();
        return trimmed.StartsWith('#') || trimmed.StartsWith(';');
    }

    private static void AddSynchronizedFile(Dictionary<string, HashSet<string>> synchronizedFiles, string filePath, string identifier)
    {
        if (!synchronizedFiles.TryGetValue(filePath, out HashSet<string>? identifiers))
        {
            identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            synchronizedFiles[filePath] = identifiers;
        }

        identifiers.Add(identifier);
    }

    private static string NormalizeEditorConfigPath(string path)
    {
        return path.Replace('\\', '/');
    }

    private readonly record struct BaselineBlock(
        int HeaderIndex,
        int BlockEndIndex,
        string Identifier,
        string[] SectionFiles
    );
}
