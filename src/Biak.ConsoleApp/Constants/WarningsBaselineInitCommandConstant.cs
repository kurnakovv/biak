// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak warnings-baseline init` command constants.
/// </summary>
public static class WarningsBaselineInitCommandConstant
{
    /// <summary>
    /// Note to add TreatWarningsAsErrors.
    /// </summary>
    public const string TREAT_WARNINGS_AS_ERRORS_NOTE = "Add this configuration to Directory.Build.props or to all .csproj files";

    /// <summary>
    /// Failed to start dotnet build process.
    /// </summary>
    public const string FAILED_TO_START_DOTNET_BUILD = "Failed to start dotnet build process.";

    /// <summary>
    /// dotnet build failed.
    /// </summary>
    public const string DOTNET_BUILD_FAILED = "dotnet build failed.";

    /// <summary>
    /// build.binlog was not generated.
    /// </summary>
    public const string BUILD_BINLOG_NOT_FOUND = "build.binlog was not generated.";

    /// <summary>
    /// Failed to initialize warnings baseline.
    /// </summary>
    public const string INIT_FAILED = "Failed to initialize warnings baseline.";
}
