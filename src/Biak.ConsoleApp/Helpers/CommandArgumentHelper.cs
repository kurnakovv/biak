// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Command argument helper.
/// </summary>
public static class CommandArgumentHelper
{
    /// <summary>
    /// Parse command options from CLI arguments.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <param name="options">Parsed options.</param>
    /// <param name="allowedOptions">Allowed option names.</param>
    /// <returns>Can be parsed or not.</returns>
    public static bool TryParseOptions(string[] args, out Dictionary<string, string> options, HashSet<string> allowedOptions)
    {
        options = new Dictionary<string, string>(StringComparer.Ordinal);

        if (args.Length == 2)
        {
            return true;
        }

        if ((args.Length - 2) % 2 != 0)
        {
            return false;
        }

        HashSet<string> allowedOptionSet = allowedOptions;

        for (int i = 2; i < args.Length; i += 2)
        {
            string option = args[i];
            string value = args[i + 1];

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!allowedOptionSet.Contains(option))
            {
                return false;
            }

            if (!options.TryAdd(option, value))
            {
                return false;
            }
        }

        return true;
    }
}
