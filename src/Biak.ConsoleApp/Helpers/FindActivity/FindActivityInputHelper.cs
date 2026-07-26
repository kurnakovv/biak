// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.FindActivity;

internal static class FindActivityInputHelper
{
    internal static async Task<FindActivityInputModel> RequestAsync(BiakConfig config, AppExecutionContext executionContext)
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
            await executionContext.Out.WriteLineAsync(SharedFindCommandConstant.ENTER_CRITERIA);
        }

        string? defaultBranch = findActivity?.DefaultBranch;
        if (string.IsNullOrWhiteSpace(defaultBranch))
        {
            await executionContext.Out.WriteAsync(SharedFindCommandConstant.DEFAULT_BRANCH_INPUT);
            defaultBranch = await executionContext.In.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(defaultBranch))
            {
                defaultBranch = "main";
            }
            await executionContext.Out.WriteLineAsync();
        }

        int? expirationPeriod = 30;
        string? expirationPeriodInput = findActivity?.ExpirationPeriod;
        bool isEmptyConfigExpirationPeriod = findActivity?.ExpirationPeriod == null;
        bool isInvalidConfigExpirationPeriod = false;
        while (true)
        {
            if (isEmptyConfigExpirationPeriod || isInvalidConfigExpirationPeriod)
            {
                await executionContext.Out.WriteAsync(FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT);
                expirationPeriodInput = await executionContext.In.ReadLineAsync();
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
                await executionContext.Out.WriteLineAsync();
                invalidFormatMessage = FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT;
            }
            else
            {
                invalidFormatMessage = FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT_IN_CONFIG;
            }

            await executionContext.Out.WriteLineAsync(invalidFormatMessage);
        }

        if (isEmptyConfigExpirationPeriod || isInvalidConfigExpirationPeriod)
        {
            await executionContext.Out.WriteLineAsync();
        }

        string? fileTypes = findActivity?.FileTypes;
        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            await executionContext.Out.WriteLineAsync(FindActivityCommandConstant.ABOUT_FILE_TYPES);
            await executionContext.Out.WriteAsync(FindActivityCommandConstant.FILE_TYPES_INPUT);
            fileTypes = await executionContext.In.ReadLineAsync();
            await executionContext.Out.WriteLineAsync();
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
            await executionContext.Out.WriteAsync(FindActivityCommandConstant.FILE_EXTENSIONS_INPUT);
            fileExtensionsInput = await executionContext.In.ReadLineAsync();
            await executionContext.Out.WriteLineAsync();
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
            await executionContext.Out.WriteLineAsync(FindActivityCommandConstant.EXCLUDE_BRANCHES_EXAMPLE);
            await executionContext.Out.WriteLineAsync(FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE);
            await executionContext.Out.WriteLineAsync(FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR);
            await executionContext.Out.WriteAsync(FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT);
            excludeBranchesInput = await executionContext.In.ReadLineAsync();
            await executionContext.Out.WriteLineAsync();
        }
        IEnumerable<string>? excludeBranches = null;
        if (!string.IsNullOrWhiteSpace(excludeBranchesInput))
        {
            excludeBranches = excludeBranchesInput.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        string? includedFilePathsInput = findActivity?.IncludedFilePaths;
        if (includedFilePathsInput == null)
        {
            await executionContext.Out.WriteAsync(FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT);
            includedFilePathsInput = await executionContext.In.ReadLineAsync();
            await executionContext.Out.WriteLineAsync();
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
                await executionContext.Out.WriteAsync(FindActivityCommandConstant.SAVE_OUTPUT_INPUT);
                string? saveOutputInput = await executionContext.In.ReadLineAsync();

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

                await executionContext.Out.WriteLineAsync();
                await executionContext.Out.WriteLineAsync(FindActivityCommandConstant.INVALID_SAVE_OUTPUT_FORMAT);
            }
            await executionContext.Out.WriteLineAsync();
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
