/*
 * PowerShellCommand.cs
 *
 *   Created: 2023-01-31-09:10:38
 *   Modified: 2023-01-31-09:10:38
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustinWritesCode.PowerShell;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

public static class PowerShellExtensions
{
    public const RunspaceMode DefaultRunspaceMode = RunspaceMode.CurrentRunspace;
    public static readonly InitialSessionState InitialSessionState = InitialSessionState.CreateDefault();

    public static PowerShell CreateCommand(string command, string[] arguments, RunspaceMode runspaceMode = RunspaceMode.CurrentRunspace)
        => PowerShell.Create(runspaceMode).AddCommand(command).AddParameters(arguments.ToList());


    public static PowerShell CreateCommand(string command, IDictionary<string, object> arguments, InitialSessionState initialSessionState)
        => PowerShell.Create(initialSessionState).AddCommand(command).AddParameters(arguments as IDictionary);

    public static PowerShell CreateCommand(string command, string[] arguments)
        => CreateCommand(command, arguments);


    public static PowerShell CreateCommand(string command, IDictionary<string, object> arguments)
        => CreateCommand(command, arguments);
}
