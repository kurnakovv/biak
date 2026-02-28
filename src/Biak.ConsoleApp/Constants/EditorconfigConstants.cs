// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

#pragma warning disable RCS1266 // Use raw string literal
/// <summary>
/// All .editorconfig constants.
/// </summary>
public static class EditorconfigConstants
{
    /// <summary>
    /// The text at the beginning of the .editorconfig file, with the biak configured.
    /// </summary>
    public const string UP_TEXT = @"# ⚠️ Attention ⚠️
#
# This project is configured with the ""biak"" tool, and all work is now managed through the "".biak"" folder.
# As a result, this file is set to read-only.
# Please avoid modifying it, as any changes you make will be lost.
#
# To learn more about biak, visit: https://github.com/kurnakovv/biak

";

    /// <summary>
    /// The text at the bottom of the .editorconfig file, with the biak configured.
    /// </summary>
    public const string BOTTOM_TEXT = @"
# ⚠️ Attention ⚠️
#
# This file is read-only.
# Please refer to the note at the beginning ☝️ of the file for more information.
";
}

#pragma warning restore RCS1266 // Use raw string literal
