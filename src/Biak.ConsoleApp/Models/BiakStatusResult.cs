// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Enums;

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Result of biak status resolution.
/// </summary>
public sealed record BiakStatusResult(BiakStatusType StatusType, string Message);
