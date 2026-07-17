// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

internal static class InspectCodeBaselineCommandTestConstants
{
    public const string BASELINE_FILTERS = $$"""
        [{ServiceC.cs}]
        # 'if' statement can be rewritten as '?:' expression [ConvertIfStatementToConditionalTernaryExpression] | https://www.jetbrains.com/help/resharper/ConvertIfStatementToConditionalTernaryExpression.html
        resharper_convert_if_statement_to_conditional_ternary_expression_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceE.cs}]
        # Convert property into auto-property [ConvertToAutoProperty] | https://www.jetbrains.com/help/resharper/ConvertToAutoProperty.html
        resharper_convert_to_auto_property_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs}]
        # Convert local variable or field into constant (private accessibility) [ConvertToConstant.Local] | https://www.jetbrains.com/help/resharper/ConvertToConstant.Local.html
        resharper_convert_to_constant_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs,ServiceC.cs}]
        # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
        resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceB.cs}]
        # Member can be made private (non-private accessibility) [MemberCanBePrivate.Global] | https://www.jetbrains.com/help/resharper/MemberCanBePrivate.Global.html
        resharper_member_can_be_private_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceE.cs}]
        # Replace with single call to FirstOrDefault(..) [ReplaceWithSingleCallToFirstOrDefault] | https://www.jetbrains.com/help/resharper/ReplaceWithSingleCallToFirstOrDefault.html
        resharper_replace_with_single_call_to_first_or_default_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceD.cs}]
        # Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
        resharper_replace_with_string_is_null_or_empty_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceC.cs}]
        # Type member is never used (private accessibility) [UnusedMember.Local] | https://www.jetbrains.com/help/resharper/UnusedMember.Local.html
        resharper_unused_member_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
        """;
}
