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
    /// Build binary log was not generated.
    /// </summary>
    public const string BUILD_BINLOG_NOT_FOUND = "Build binary log was not generated.";

    /// <summary>
    /// Build contains errors.
    /// </summary>
    public const string BUILD_CONTAINS_ERRORS =
        "Build contains errors. Fix them before managing warnings baseline.";
}
