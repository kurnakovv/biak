// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// Shared constants for warnings-baseline build step.
/// </summary>
public static class WarningsBaselineBuildConstant
{
    /// <summary>
    /// Failed to start dotnet build process.
    /// </summary>
    public const string FAILED_TO_START_DOTNET_BUILD = "Failed to start 'dotnet build' process.";

    /// <summary>
    /// dotnet build failed.
    /// </summary>
    public const string DOTNET_BUILD_FAILED = "'dotnet build' failed.";

    /// <summary>
    /// dotnet build timed out.
    /// </summary>
    public const string DOTNET_BUILD_TIMED_OUT = "'dotnet build' timed out after 30 minutes.";

    /// <summary>
    /// Build target path is not allowed.
    /// </summary>
    public const string INVALID_BUILD_TARGET_PATH =
        "The specified build target path is not allowed. Use a path within the current working directory.";

    /// <summary>
    /// Build target file was not found.
    /// </summary>
    public const string BUILD_TARGET_NOT_FOUND = "The specified build target file was not found.";

    /// <summary>
    /// Auto-detection of build target is ambiguous.
    /// </summary>
    public const string AMBIGUOUS_BUILD_TARGET =
        "Build target auto-detection is ambiguous. Specify '--target <path>' to choose a solution or project.";

    /// <summary>
    /// Build binary log was not generated.
    /// </summary>
    public const string BUILD_BINLOG_NOT_FOUND = "Build binary log was not generated.";

    /// <summary>
    /// Build contains errors.
    /// </summary>
    public const string BUILD_CONTAINS_ERRORS =
        "Build contains errors. Fix them before managing warnings baseline.";
}
