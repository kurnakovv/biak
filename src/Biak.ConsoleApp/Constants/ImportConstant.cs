// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// ^biak^ import "..." constants.
/// </summary>
public static class ImportConstant
{
    /// <summary>
    /// Warning for non white list URL's.
    /// </summary>
    public const string WHITE_LIST_ALLOWED = $"{ATTENTION} Only https://gist.githubusercontent.com is allowed to use for URL imports:";

    /// <summary>
    /// Unable to retrieve content from link.
    /// </summary>
    public const string UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK = $"{ATTENTION} Unable to retrieve content from link:";

    /// <summary>
    /// Forbidden outside .biak folder.
    /// </summary>
    public const string FORBIDDEN_OUTSIDE = $"{ATTENTION} It is forbidden to go beyond the .biak folder:";

    /// <summary>
    /// Response too large.
    /// </summary>
    public const string RESPONSE_TOO_LARGE = $"{ATTENTION} Import response exceeds maximum allowed size (5 MB):";

    /// <summary>
    /// Invalid content type.
    /// </summary>
    public const string INVALID_CONTENT_TYPE = $"{ATTENTION} Import response has unsupported content type:";

    /// <summary>
    /// File not found.
    /// </summary>
    public const string FILE_NOT_FOUND = $"{ATTENTION} Import file not found:";

    /// <summary>
    /// Failure behavior type not implemented.
    /// </summary>
    public const string FAILURE_BEHAVIOR_TYPE_NOT_IMPLEMENTED = $"{ATTENTION} Failure behavior type not implemented, please contact support";

    private const string ATTENTION = "⚠️ Attention:";
}
