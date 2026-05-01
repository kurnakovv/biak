// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.FindActivity;

internal static class FindActivityInputHelper
{
    internal static FindActivityInputModel Request(BiakConfig config)
    {
        if (config.FindActivity?.DefaultBranch == null ||
            config.FindActivity?.ExpirationPeriod == null ||
            config.FindActivity?.FileTypes == null ||
            config.FindActivity?.FileExtensions == null ||
            config.FindActivity?.ExcludeBranches == null ||
            config.FindActivity?.IncludedFilePaths == null
        )
        {
            Console.WriteLine(FindActivityCommandConstant.ENTER_CRITERIA);
        }

        string? defaultBranch = config.FindActivity?.DefaultBranch;
        if (string.IsNullOrWhiteSpace(defaultBranch))
        {
            Console.Write(FindActivityCommandConstant.DEFAULT_BRANCH_INPUT);
            defaultBranch = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(defaultBranch))
            {
                defaultBranch = "main";
            }
            Console.WriteLine();
        }

        int? expirationPeriod = 30;
        string? expirationPeriodInput = config.FindActivity?.ExpirationPeriod;
        bool isEmptyConfigExpirationPeriod = config.FindActivity?.ExpirationPeriod == null;
        bool isInvalidConfigExpirationPeriod = false;
        while (true)
        {
            if (isEmptyConfigExpirationPeriod || isInvalidConfigExpirationPeriod)
            {
                Console.Write(FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT);
                expirationPeriodInput = Console.ReadLine();
            }
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
            isInvalidConfigExpirationPeriod = true;

            string invalidFormatMessage;

            if (isEmptyConfigExpirationPeriod)
            {
                Console.WriteLine();
                invalidFormatMessage = FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT;
            }
            else
            {
                invalidFormatMessage = FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT_IN_CONFIG;
            }

            Console.WriteLine(invalidFormatMessage);
        }

        if (isEmptyConfigExpirationPeriod || isInvalidConfigExpirationPeriod)
        {
            Console.WriteLine();
        }

        string? fileTypes = config.FindActivity?.FileTypes;
        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            Console.WriteLine(FindActivityCommandConstant.ABOUT_FILE_TYPES);
            Console.Write(FindActivityCommandConstant.FILE_TYPES_INPUT);
            fileTypes = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileTypes))
            {
                fileTypes = "MDR";
            }
            else if (fileTypes == "*")
            {
                fileTypes = null;
            }
            Console.WriteLine();
        }

        IEnumerable<string> fileExtensions = new List<string>() { ".cs" };

        string? fileExtensionsInput = config.FindActivity?.FileExtensions;
        if (string.IsNullOrWhiteSpace(fileExtensionsInput))
        {
            Console.Write(FindActivityCommandConstant.FILE_EXTENSIONS_INPUT);
            fileExtensionsInput = Console.ReadLine();
            Console.WriteLine();
        }

        if (fileExtensionsInput == "*")
        {
            fileExtensions = new List<string>();
        }
        else if (!string.IsNullOrWhiteSpace(fileExtensionsInput))
        {
            fileExtensions = fileExtensionsInput.Trim().Split(",");
        }

        string? excludeBranchesInput = config.FindActivity?.ExcludeBranches;
        if (excludeBranchesInput == null)
        {
            Console.WriteLine(FindActivityCommandConstant.EXCLUDE_BRANCES_EXAMPLE);
            Console.WriteLine(FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE);
            Console.WriteLine(FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR);
            Console.Write(FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT);
            excludeBranchesInput = Console.ReadLine();
            Console.WriteLine();
        }
        IEnumerable<string>? excludeBranches = null;
        if (!string.IsNullOrWhiteSpace(excludeBranchesInput))
        {
            excludeBranches = excludeBranchesInput.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        string? includedFilePathsInput = config.FindActivity?.IncludedFilePaths;
        if (includedFilePathsInput == null)
        {
            Console.Write(FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT);
            includedFilePathsInput = Console.ReadLine();
            Console.WriteLine();
        }
        IEnumerable<string>? includedFilePaths = null;
        if (!string.IsNullOrWhiteSpace(includedFilePathsInput))
        {
            includedFilePaths = includedFilePathsInput.Trim().Split(",");
        }

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
