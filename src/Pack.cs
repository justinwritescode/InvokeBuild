using System.Globalization;
/*
 * Pack.cs
 *
 *   Created: 2023-01-29-01:11:59
 *   Modified: 2023-01-31-01:55:25
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustinWritesCode.PowerShell;

using System.Management.Automation;

[Cmdlet(VerbsLifecycle.Invoke, "Pack", DefaultParameterSetName = "WithoutCommand")]
[Alias("pk", "pack", "invokepack")]
public class InvokePack : InvokeDotnet
{
    public InvokePack()
    {
    }

    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true, Mandatory = true, Position = 0,
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.")]
    [ValidatePattern(@"^(?:(?:.*\.*proj)|(?:.*\.*props)|(?:.*\.*targets)|(?:.*\.*usings)|(?:.*\.*tasks)|(?:.*\.*items))$")]
    [Alias("proj", "project", "path", "projpath")]
    public override string? ProjectPath { get; set; } = "./*.*proj";

    public override DotnetCommand Command { get => DotnetCommand.pack.Instance; set { } }
}
