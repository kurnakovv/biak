// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

internal static class InspectCodeBaselineCommandTestConstants
{
    public const string BASELINE_FILTERS = $$"""
        [{ServiceE.cs}]
        resharper_arrange_accessor_owner_body_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceC.cs}]
        resharper_convert_if_statement_to_conditional_ternary_expression_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceE.cs}]
        resharper_convert_to_auto_property_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs}]
        resharper_convert_to_constant_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceC.cs,ServiceE.cs}]
        resharper_convert_to_primary_constructor_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs,ServiceC.cs}]
        resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceB.cs}]
        resharper_member_can_be_private_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceB.cs}]
        resharper_negative_equality_expression_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceE.cs}]
        resharper_replace_with_single_call_to_first_or_default_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceD.cs}]
        resharper_replace_with_string_is_null_or_empty_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs,ServiceB.cs,ServiceC.cs,ServiceD.cs,ServiceE.cs}]
        resharper_unused_member_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceC.cs}]
        resharper_unused_member_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceA.cs,ServiceB.cs,ServiceC.cs,ServiceD.cs,ServiceE.cs}]
        resharper_unused_type_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

        [{ServiceD.cs}]
        resharper_use_collection_expression_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
        """;
}
