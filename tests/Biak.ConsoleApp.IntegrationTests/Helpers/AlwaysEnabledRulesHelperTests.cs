// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class AlwaysEnabledRulesHelperTests
{
    [Fact]
    public void DisableWhenNoStartMarkerReplacesAllSeverities()
    {
        const string INPUT = """

            dotnet_diagnostic.CA9999.severity = error
            dotnet_diagnostic.CA1001.severity = warning

            """;
        const string EXPECTED = """

            dotnet_diagnostic.CA9999.severity = none
            dotnet_diagnostic.CA1001.severity = none

            """;

        string result = SeverityHelper.Disable(INPUT, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(EXPECTED, result);
    }

    [Fact]
    public void DisableWhenStartMarkerHasNoEndMarkerReplacesAllSeverities()
    {
        const string INPUT = """

            ^biak^ always-enabled start
            dotnet_diagnostic.CA9999.severity = error
            dotnet_diagnostic.CA1001.severity = warning

            """;
        const string EXPECTED = """

            ^biak^ always-enabled start
            dotnet_diagnostic.CA9999.severity = none
            dotnet_diagnostic.CA1001.severity = none

            """;

        string result = SeverityHelper.Disable(INPUT, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(EXPECTED, result);
    }

    [Fact]
    public void DisableWhenBlockIsEmptyReplacesOutsideSeverities()
    {
        const string INPUT =
            """

            ^biak^ always-enabled start
            ^biak^ always-enabled end
            dotnet_diagnostic.CA9999.severity = error

            """;
        const string EXPECTED =
            """

            ^biak^ always-enabled start
            ^biak^ always-enabled end
            dotnet_diagnostic.CA9999.severity = none

            """;

        string result = SeverityHelper.Disable(INPUT, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(EXPECTED, result);
    }
}
