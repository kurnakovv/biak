// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// Constants for <see cref="Helpers.Baseline.InspectCode.InspectCodeBaselineRunHelper"/>.
/// </summary>
public static class InspectCodeBaselineRunHelperConstant
{
    /// <summary>
    /// Failed to start jb inspectcode process.
    /// </summary>
    public const string FAILED_TO_START_INSPECTCODE = "Failed to start 'jb' via 'dotnet tool run jb'. Make sure JetBrains.ReSharper.GlobalTools is in your .config/dotnet-tools.json and 'dotnet tool restore' has been run.";

    /// <summary>
    /// jb inspectcode failed.
    /// </summary>
    public const string INSPECTCODE_FAILED = "'dotnet tool run jb inspectcode' failed.";

    /// <summary>
    /// jb inspectcode timed out.
    /// </summary>
    public const string INSPECTCODE_TIMED_OUT = "'dotnet tool run jb inspectcode' timed out after 30 minutes.";

    /// <summary>
    /// SARIF report file was not produced.
    /// </summary>
    public const string SARIF_REPORT_NOT_FOUND = "InspectCode SARIF report was not produced at the expected path.";

    /// <summary>
    /// Directory for SARIF reports inside .biak folder.
    /// </summary>
    public const string REPORTS_DIRECTORY = ".biak/inspectcode-baseline";

    /// <summary>
    /// No .slnx, .sln or .csproj file found in the current directory.
    /// </summary>
    public const string NO_SOLUTION_OR_PROJECT_FOUND =
        "No .slnx, .sln, or .csproj file found in the current directory. " +
        "Run the command from the directory that contains your solution or project file, " +
        "or specify the path explicitly in .biak/config.json:\n" +
        "  \"inspectCodeBaseline\": {\n" +
        "    \"target\": \"path/to/YourSolution.sln\"\n" +
        "  }";

    /// <summary>
    /// Explicit target file was not found — prefix (before the resolved path).
    /// </summary>
    public const string TARGET_NOT_FOUND_PREFIX = "InspectCode target file not found: ";

    /// <summary>
    /// Explicit target file was not found — suffix (after the resolved path).
    /// </summary>
    public const string TARGET_NOT_FOUND_SUFFIX =
        ".\nCheck the \"target\" value in .biak/config.json:\n" +
        "  \"inspectCodeBaseline\": {\n" +
        "    \"target\": \"path/to/YourSolution.sln\"\n" +
        "  }";
}
