// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

if (args.Length == 0)
{
    Console.WriteLine(DocsConstant.GREETING);
}
else if (SetupCommand.IsRunnable(args))
{
    await SetupCommand.RunAsync();
}
else
{
    Console.WriteLine(DocsConstant.NO_COMMAND);
}
