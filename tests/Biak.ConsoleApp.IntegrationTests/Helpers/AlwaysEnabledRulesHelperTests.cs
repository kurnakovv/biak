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
        string input = @"
dotnet_diagnostic.CA9999.severity = error
dotnet_diagnostic.CA1001.severity = warning
";
        string expected = @"
dotnet_diagnostic.CA9999.severity = none
dotnet_diagnostic.CA1001.severity = none
";

        string result = SeverityHelper.Disable(input, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void DisableWhenStartMarkerHasNoEndMarkerReplacesAllSeverities()
    {
        string input = @"
^biak^ always-enabled start
dotnet_diagnostic.CA9999.severity = error
dotnet_diagnostic.CA1001.severity = warning
";
        string expected = @"
^biak^ always-enabled start
dotnet_diagnostic.CA9999.severity = none
dotnet_diagnostic.CA1001.severity = none
";

        string result = SeverityHelper.Disable(input, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void DisableWhenBlockIsEmptyReplacesOutsideSeverities()
    {
        string input =
            """

            ^biak^ always-enabled start
            ^biak^ always-enabled end
            dotnet_diagnostic.CA9999.severity = error

            """;
        string expected =
            """

            ^biak^ always-enabled start
            ^biak^ always-enabled end
            dotnet_diagnostic.CA9999.severity = none

            """;

        string result = SeverityHelper.Disable(input, BiakConfig.DefaultSeveritiesToDisable, SeverityLevelType.None);

        Assert.Equal(expected, result);
    }
}
