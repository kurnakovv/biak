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
    /// Build binary log file path.
    /// </summary>
    public const string BUILD_BINLOG_PATH = "biak-build.binlog";

    /// <summary>
    /// Warnings-baseline initialization started.
    /// </summary>
    public const string INIT_STARTED = "warnings-baseline init started...";

    /// <summary>
    /// Note to add TreatWarningsAsErrors.
    /// </summary>
    public const string TREAT_WARNINGS_AS_ERRORS_NOTE = "Choose one: add this full snippet to Directory.Build.props," +
        " or if you configure in a .csproj file(s), add only <TreatWarningsAsErrors>true</TreatWarningsAsErrors> inside an existing <PropertyGroup>.";

    /// <summary>
    /// Insert filters into .editorconfig note.
    /// </summary>
    public const string INSERT_FILTERS_TO_EDITORCONFIG_NOTE = "Insert these filters into your .editorconfig file";

    /// <summary>
    /// No warnings found in the build.
    /// </summary>
    public const string NO_WARNINGS_FOUND = "No warnings found. Nothing to generate.";

    /// <summary>
    /// Failed to initialize warnings baseline.
    /// </summary>
    public const string INIT_FAILED = "Failed to initialize warnings baseline.";

    /// <summary>
    /// Treat warnings as errors configuration.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    public static readonly string TREAT_WARNINGS_AS_ERRORS_CONFIGURATION = string.Join(
#pragma warning restore IDE1006 // Naming Styles
        Environment.NewLine,
        "<Project>",
        "\t<PropertyGroup>",
        "\t\t<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
        "\t</PropertyGroup>",
        "</Project>"
    );
}
