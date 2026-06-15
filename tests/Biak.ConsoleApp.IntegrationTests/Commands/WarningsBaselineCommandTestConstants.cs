// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

internal static class WarningsBaselineCommandTestConstants
{
    public const string BASELINE_FILTERS = $$"""
        [{VisualBasicProject/Module1.vb}]
        dotnet_diagnostic.BC40000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0108.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{ProgramCS0168Warning.cs}]
        dotnet_diagnostic.CS0168.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{MyClassCS0169.cs}]
        dotnet_diagnostic.CS0169.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{ProgramCS0219Warning.cs}]
        dotnet_diagnostic.CS0219.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{ProgramCS0612.cs}]
        dotnet_diagnostic.CS0612.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0649.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]
        dotnet_diagnostic.CS8618.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
        """;

    public const string BASELINE_EDITORCONFIG = """
        root = true

        """
        + BASELINE_FILTERS;
}
