// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// .biak/config.json constants.
/// </summary>
public static class BiakConfigConstant
{
    /// <summary>
    /// .biak/config.json file not found message.
    /// </summary>
    public const string FILE_NOT_FOUND = $"{WARNING} '.biak/config.json' file not found. {DEFAULT_SETTINGS}";

    /// <summary>
    /// Message when .biak/config.json is null.
    /// </summary>
    public const string IS_NULL = $"{WARNING} '.biak/config.json' is null. {DEFAULT_SETTINGS}";

    /// <summary>
    /// Invalid JSON format message.
    /// </summary>
    public const string INVALID_FORMAT = $"{WARNING} Invalid JSON format in '.biak/config.json' file. {DEFAULT_SETTINGS}";

    /// <summary>
    /// Warning.
    /// </summary>
    private const string WARNING = "⚠️ Warning:";

    /// <summary>
    /// Default settings message on warning.
    /// </summary>
    private const string DEFAULT_SETTINGS = "Default settings will be applied.";
}
