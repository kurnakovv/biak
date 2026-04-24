// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// ^biak^ import "..." helper.
/// </summary>
public static class ImportHelper
{
    private const long MAX_SIZE = 5_000_000; // 5 MB

    private static readonly HttpClient s_httpClient = new(
        new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            CheckCertificateRevocationList = true,
        }
    )
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    private static readonly Regex s_importRegex = new(
        @"^(?!\s*#)[ \t]*\^biak\^\s*import\s*(?:""([^""]+)""|(\S+))",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    /// <summary>
    /// Replace imports with file content.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="onImportFailure">What to do on import failure.</param>
    /// <returns>Replaced content with imports.</returns>
    public static async Task<string> ReplaceAsync(string content, FailureBehaviorType onImportFailure)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        MatchCollection matches = s_importRegex.Matches(content);
        if (matches.Count == 0)
        {
            return content;
        }

        string biakDir = Path.Join(Directory.GetCurrentDirectory(), ".biak");
        string biakFullPath = Path.GetFullPath(biakDir);

        string newline = content.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";

        StringBuilder sb = new(content);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];

            string value = match.Groups[1].Success
                ? match.Groups[1].Value
                : match.Groups[2].Value;

            string? replacement = await ResolveImportAsync(value, biakFullPath, newline, onImportFailure);

            if (replacement != null)
            {
                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
            }
        }

        return sb.ToString();
    }

    private static async Task<string?> ResolveImportAsync(
        string value,
        string biakFullPath,
        string newline,
        FailureBehaviorType onImportFailure
    )
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Scheme != Uri.UriSchemeHttps || !uri.Host.Equals("gist.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
            {
                await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.WHITE_LIST_ALLOWED} {value}");
                return null;
            }

            try
            {
                using HttpResponseMessage response = await s_httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (HTTP {(int)response.StatusCode} {response.ReasonPhrase})");
                    return null;
                }

                if (response.Content.Headers.ContentLength > MAX_SIZE)
                {
                    await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.RESPONSE_TOO_LARGE} {value}");
                    return null;
                }

                if (!response.Content.Headers.ContentType?.MediaType?.StartsWith("text/") ?? true)
                {
                    await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.INVALID_CONTENT_TYPE} {value}");
                    return null;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                return NormalizeLineEndings(responseContent, newline);
            }
            catch (Exception ex)
            {
                await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (Reason: {ex.Message})");
                return null;
            }
        }

        string fullPath = Path.GetFullPath(
            Path.Join(biakFullPath, value)
        );

        if (!fullPath.StartsWith(biakFullPath, StringComparison.OrdinalIgnoreCase))
        {
            await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.FORBIDDEN_OUTSIDE} {value}");
            return null;
        }

        if (!File.Exists(fullPath))
        {
            await HandleFailureBehaviorAsync(onImportFailure, $"{ImportConstant.FILE_NOT_FOUND} {value}");
            return null;
        }

        return await File.ReadAllTextAsync(fullPath);
    }

    private static string NormalizeLineEndings(string content, string newline)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", newline, StringComparison.Ordinal);
    }

    private static async Task HandleFailureBehaviorAsync(FailureBehaviorType onImportFailure, string message)
    {
        switch (onImportFailure)
        {
            case FailureBehaviorType.Nothing:
            {
                return;
            }
            case FailureBehaviorType.Warning:
            {
                Console.WriteLine(message);
                return;
            }
            case FailureBehaviorType.Error:
            {
                await Console.Error.WriteLineAsync(message);
                Environment.Exit(1);
                return;
            }
            default:
            {
                throw new NotImplementedException(ImportConstant.FAILURE_BEHAVIOR_TYPE_NOT_IMPLEMENTED);
            }
        }
    }
}
