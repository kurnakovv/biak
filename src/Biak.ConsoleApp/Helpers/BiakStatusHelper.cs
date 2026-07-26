// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Helper for resolving current biak status.
/// </summary>
public static class BiakStatusHelper
{
    /// <summary>
    /// Resolves current biak status and a human-readable message.
    /// </summary>
    /// <param name="executionContext">Provides the context required to perform the operation.</param>
    /// <param name="includeDebugDetails">Whether to include detailed broken-status reason.</param>
    /// <returns>Status type with rendered status message.</returns>
    public static async Task<BiakStatusResult> GetAsync(AppExecutionContext executionContext, bool includeDebugDetails = false)
    {
        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(suppressConsoleOutput: true, executionContext: executionContext);

        if (editorconfigPaths.MainValue == null)
        {
            return new BiakStatusResult(
                BiakStatusType.Broken,
                includeDebugDetails
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.BIAK_NOT_INITIALIZED + " " + UIConstant.RUN_BIAK_SETUP
                    : UIConstant.STATUS_BROKEN
            );
        }

        if (editorconfigPaths.Value == null)
        {
            return new BiakStatusResult(
                BiakStatusType.Broken,
                includeDebugDetails
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.EDITORCONFIG_NOT_FOUND + Path.Join(executionContext.WorkingDirectory, ".editorconfig")
                    : UIConstant.STATUS_BROKEN
            );
        }

        (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync(executionContext: executionContext);
        if (message != null)
        {
            return new BiakStatusResult(
                BiakStatusType.Broken,
                includeDebugDetails
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + message
                    : UIConstant.STATUS_BROKEN
            );
        }

        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        string currentContent = NormalizeLineEndings(await File.ReadAllTextAsync(editorconfigPaths.Value));
        string disabledContent = NormalizeLineEndings(
            await EditorconfigHelper.GetDisabledContentAsync(editorconfigMainContent, config, executionContext)
        );

        if (string.Equals(currentContent, disabledContent, StringComparison.Ordinal))
        {
            return new BiakStatusResult(BiakStatusType.Disabled, UIConstant.STATUS_DISABLED);
        }

        string enabledContent = NormalizeLineEndings(
            await EditorconfigHelper.GetEnabledContentAsync(editorconfigMainContent, config, executionContext)
        );

        return string.Equals(currentContent, enabledContent, StringComparison.Ordinal)
            ? new BiakStatusResult(BiakStatusType.Enabled, UIConstant.STATUS_ENABLED)
            : new BiakStatusResult(BiakStatusType.Unsynchronised, UIConstant.STATUS_UNSYNCHRONISED);
    }

    private static string NormalizeLineEndings(string content)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
    }
}
