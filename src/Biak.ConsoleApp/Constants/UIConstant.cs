// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// UI constants.
/// </summary>
public static class UIConstant
{
    /// <summary>
    /// Message when user enters an invalid command.
    /// </summary>
    public const string NO_COMMAND = "Command not found";

    /// <summary>
    /// Message when .editorconfig not found.
    /// </summary>
    public const string EDITORCONFIG_NOT_FOUND = ".editorconfig not found: ";

    /// <summary>
    /// The text that the user must enter when agreeing to perform an action.
    /// </summary>
    public const string CONFIRM = "y";

    /// <summary>
    /// Message when user call `dotnet biak setup` command with biak folder.
    /// </summary>
    public const string BIAK_FOLDER_ALREADY_EXISTS = $"Folder .biak already exists. Recreate it? Type '{CONFIRM}' to confirm, or press Enter to cancel:";

    /// <summary>
    /// Start setup message.
    /// </summary>
    public const string START_SETUP = "Setup .biak folder...";

    /// <summary>
    /// End setup message.
    /// </summary>
    public const string END_SETUP = "Folder .biak was created successfully.";
}
