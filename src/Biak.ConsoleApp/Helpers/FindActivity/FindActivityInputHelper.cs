// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Helpers.FindActivity;

internal static class FindActivityInputHelper
{
    internal static FindActivityInputModel Request()
    {
        Console.WriteLine("Please enter the desired criteria");

        Console.Write("Default branch ('main' by default): ");
        string? defaultBranch = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(defaultBranch))
        {
            defaultBranch = "main";
        }
        Console.WriteLine();

        int? expirationPeriod = 30;
        while (true)
        {
            Console.Write("Expiration period in days (default: 30, '*' for unlimited): ");
            string? expirationPeriodInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(expirationPeriodInput))
            {
                break;
            }
            if (expirationPeriodInput == "*")
            {
                expirationPeriod = null;
                break;
            }

            if (int.TryParse(expirationPeriodInput, out int value) && value > 0)
            {
                expirationPeriod = value;
                break;
            }

            Console.WriteLine("Invalid format");
        }
        Console.WriteLine();

        Console.WriteLine("About file types https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB");
        Console.Write("File types (MDR by default, '*' all files): ");
        string? fileTypes = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            fileTypes = "MDR";
        }
        else if (fileTypes == "*")
        {
            fileTypes = null;
        }
        Console.WriteLine();

        Console.Write("File extensions separated by commas ('.cs' by default, '*' all files): ");
        string? fileExtensionsInput = Console.ReadLine();
        IEnumerable<string> fileExtensions = new List<string>() { ".cs" };
        if (fileExtensionsInput == "*")
        {
            fileExtensions = new List<string>();
        }
        else if (!string.IsNullOrWhiteSpace(fileExtensionsInput))
        {
            fileExtensions = fileExtensionsInput.Trim().Split(",");
        }
        Console.WriteLine();

        Console.WriteLine("Exclude specific branches separated by space (e.g., f-1 f-2).");
        Console.WriteLine("You can use '*' to select multiple similar branches (e.g., f-*).");
        Console.WriteLine("By default, no additional branches are excluded.");
        Console.Write("Exclude branches: ");
        string? excludeBranchesInput = Console.ReadLine();
        IEnumerable<string>? excludeBranches = null;
        if (!string.IsNullOrWhiteSpace(excludeBranchesInput))
        {
            excludeBranches = excludeBranchesInput.Trim().Split(" ");
        }
        Console.WriteLine();

        Console.Write("Enter file paths (comma-separated) to process only these files, others will be skipped (default: all files): ");
        string? includedFilePathsInput = Console.ReadLine();
        IEnumerable<string>? includedFilePaths = null;
        if (!string.IsNullOrWhiteSpace(includedFilePathsInput))
        {
            includedFilePaths = includedFilePathsInput.Trim().Split(",");
        }
        Console.WriteLine();

        return new FindActivityInputModel(
            defaultBranch: defaultBranch,
            expirationPeriod: expirationPeriod,
            fileTypes: fileTypes,
            fileExtensions: fileExtensions,
            excludeBranches: excludeBranches,
            includedFilePaths: includedFilePaths
        );
    }
}

internal class FindActivityInputModel
{
    internal FindActivityInputModel(
        string defaultBranch,
        int? expirationPeriod,
        string? fileTypes,
        IEnumerable<string> fileExtensions,
        IEnumerable<string>? excludeBranches,
        IEnumerable<string>? includedFilePaths
    )
    {
        DefaultBranch = defaultBranch;
        ExpirationPeriod = expirationPeriod;
        FileTypes = fileTypes;
        FileExtensions = fileExtensions;
        ExcludeBranches = excludeBranches;
        IncludedFilePaths = includedFilePaths;
    }

    internal string DefaultBranch { get; } = null!;
    internal int? ExpirationPeriod { get; }
    internal string? FileTypes { get; }
    internal IEnumerable<string> FileExtensions { get; }
    internal IEnumerable<string>? ExcludeBranches { get; }
    internal IEnumerable<string>? IncludedFilePaths { get; }
}
