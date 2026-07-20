// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.IntegrationTests.Commands.Baseline.InspectCode;

internal static class InspectCodeBaselineCommandTestConstants
{
    public const string BASELINE_FILTERS = $$"""
        # 'if' statement can be rewritten as '?:' expression [ConvertIfStatementToConditionalTernaryExpression] | https://www.jetbrains.com/help/resharper/ConvertIfStatementToConditionalTernaryExpression.html
        [{ServiceC.cs}]
        resharper_convert_if_statement_to_conditional_ternary_expression_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Convert property into auto-property [ConvertToAutoProperty] | https://www.jetbrains.com/help/resharper/ConvertToAutoProperty.html
        [{ServiceE.cs}]
        resharper_convert_to_auto_property_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Convert local variable or field into constant (private accessibility) [ConvertToConstant.Local] | https://www.jetbrains.com/help/resharper/ConvertToConstant.Local.html
        [{ServiceA.cs}]
        resharper_convert_to_constant_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
        [{ServiceA.cs,ServiceC.cs}]
        resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Member can be made private (non-private accessibility) [MemberCanBePrivate.Global] | https://www.jetbrains.com/help/resharper/MemberCanBePrivate.Global.html
        [{ServiceB.cs}]
        resharper_member_can_be_private_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Replace with single call to FirstOrDefault(..) [ReplaceWithSingleCallToFirstOrDefault] | https://www.jetbrains.com/help/resharper/ReplaceWithSingleCallToFirstOrDefault.html
        [{ServiceE.cs}]
        resharper_replace_with_single_call_to_first_or_default_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
        [{ServiceD.cs}]
        resharper_replace_with_string_is_null_or_empty_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        # Type member is never used (private accessibility) [UnusedMember.Local] | https://www.jetbrains.com/help/resharper/UnusedMember.Local.html
        [{ServiceC.cs}]
        resharper_unused_member_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
        """;
}
