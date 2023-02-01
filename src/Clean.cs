/*
 * Clean.cs
 *
 *   Created: 2023-01-29-08:23:35
 *   Modified: 2023-01-31-01:54:52
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustinWritesCode.PowerShell;

using System.Management.Automation;

[Cmdlet(VerbsLifecycle.Invoke, "Clean", DefaultParameterSetName = "WithoutCommand")]
[Alias("clean", "cln", "limpiar", "invokeclean")]
public class InvokeClean : InvokeDotnet
{
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true, Mandatory = true, Position = 0,
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.")]
    [ValidatePattern(@"^(?:(?:.*\.*proj)|(?:.*\.*props)|(?:.*\.*targets)|(?:.*\.*usings)|(?:.*\.*tasks)|(?:.*\.*items))$")]
    [Alias("proj", "project", "path", "projpath")]
    public override string? ProjectPath { get; set; } = "./*.*proj";

    public override DotnetCommand Command { get => DotnetCommand.clean.Instance; set { } }

    protected override void BeginProcessing() => base.BeginProcessing();

    protected override void ProcessRecord()
    {
        throw new NotSupportedException();
        // var project = Create(ProjectPath).DeepClone();
        // var objDir = project.Properties.FirstOrDefault(p => p.Name == IntermediateOutputPath);
        // var binDir = project.Properties.FirstOrDefault(p => p.Name == OutputPath);
        // Delete(objDir?.Value ?? "obj", true);
        // Delete(binDir?.Value ?? "bin", true);

        // WriteVerbose(
        //     $"""""
        //         BeginProcessing() called
        //             With arguments:
        //                 ProjectPath: {ProjectPath}
        //                 Configuration: {Configuration}
        //                 Verbosity: {Verbosity}
        //                 Target: {Join(", ", Target)}
        //                 Version: {Version}
        //                 AssemblyVersion: {AssemblyVersion}
        //                 PrintTargets: {PrintTargets}
        //                 Property: {Join(", ", Property)}
        //                 BinaryLogger: {BinaryLogger}
        //                 Help: {Help}
        // """""
        // );
    }

    protected override void EndProcessing() => base.EndProcessing();

    protected override void StopProcessing() => base.StopProcessing();
}
