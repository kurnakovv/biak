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
        FindActivityInputConfigModel? findActivity = config.FindActivity;
        if (findActivity == null ||
            findActivity.DefaultBranch == null ||
            findActivity.ExpirationPeriod == null ||
            findActivity.FileTypes == null ||
            findActivity.FileExtensions == null ||
            findActivity.ExcludeBranches == null ||
            findActivity.IncludedFilePaths == null ||
            findActivity.SaveOutput == null
        )
        {
            Console.WriteLine(FindActivityCommandConstant.ENTER_CRITERIA);
        }

        string? defaultBranch = findActivity?.DefaultBranch;
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
        string? expirationPeriodInput = findActivity?.ExpirationPeriod;
        bool isEmptyConfigExpirationPeriod = findActivity?.ExpirationPeriod == null;
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

        string? fileTypes = findActivity?.FileTypes;
        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            Console.WriteLine(FindActivityCommandConstant.ABOUT_FILE_TYPES);
            Console.Write(FindActivityCommandConstant.FILE_TYPES_INPUT);
            fileTypes = Console.ReadLine();
            Console.WriteLine();
        }

        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            fileTypes = "MDR";
        }
        else if (fileTypes == "*")
        {
            fileTypes = null;
        }

        IEnumerable<string> fileExtensions = new List<string>() { ".cs" };

        string? fileExtensionsInput = findActivity?.FileExtensions;
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

        string? excludeBranchesInput = findActivity?.ExcludeBranches;
        if (excludeBranchesInput == null)
        {
            Console.WriteLine(FindActivityCommandConstant.EXCLUDE_BRANCHES_EXAMPLE);
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

        string? includedFilePathsInput = findActivity?.IncludedFilePaths;
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

        bool? saveOutput = findActivity?.SaveOutput;
        if (saveOutput == null)
        {
            while (true)
            {
                Console.Write(FindActivityCommandConstant.SAVE_OUTPUT_INPUT);
                string? saveOutputInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(saveOutputInput))
                {
                    saveOutput = false;
                    break;
                }

                if (bool.TryParse(saveOutputInput, out bool saveOutputResult))
                {
                    saveOutput = saveOutputResult;
                    break;
                }

                Console.WriteLine();
                Console.WriteLine(FindActivityCommandConstant.INVALID_SAVE_OUTPUT_FORMAT);
            }
            Console.WriteLine();
        }

        return new FindActivityInputModel(
            defaultBranch: defaultBranch,
            expirationPeriod: expirationPeriod,
            fileTypes: fileTypes,
            fileExtensions: fileExtensions,
            excludeBranches: excludeBranches,
            includedFilePaths: includedFilePaths,
            saveOutput: saveOutput.Value
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
        IEnumerable<string>? includedFilePaths,
        bool saveOutput
    )
    {
        DefaultBranch = defaultBranch;
        ExpirationPeriod = expirationPeriod;
        FileTypes = fileTypes;
        FileExtensions = fileExtensions;
        ExcludeBranches = excludeBranches;
        IncludedFilePaths = includedFilePaths;
        SaveOutput = saveOutput;
    }

    internal string DefaultBranch { get; } = null!;
    internal int? ExpirationPeriod { get; }
    internal string? FileTypes { get; }
    internal IEnumerable<string> FileExtensions { get; }
    internal IEnumerable<string>? ExcludeBranches { get; }
    internal IEnumerable<string>? IncludedFilePaths { get; }
    internal bool SaveOutput { get; }
}
