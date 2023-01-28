/*
 * Invoke-Build.cs
 *
 *   Created: 2023-01-26-04:55:59
 *   Modified: 2023-01-26-04:55:59
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustinWritesCode.PowerShell;
using System.Management.Automation;           // Windows PowerShell namespace.
using System.Runtime.InteropServices;

[Cmdlet(VerbsLifecycle.Invoke, "Build", DefaultParameterSetName = "WithoutCommand")]
[Alias("ib", "build", "invokebuild")]
public class InvokeBuild : Cmdlet
{

    [Parameter(Mandatory = false, Position = 1, ParameterSetName = "WithCommand",
        HelpMessage = "The command to run. Defaults to 'build'.")]
    [ValidateSet("build", "test", "pack", "publish", "clean", "restore", "run", "help")]
    public string? Command { get; set; } = "build";

    [Parameter(Mandatory = false, Position = 2, ParameterSetName = "WithoutCommand",
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.")]
    [Parameter(Mandatory = false, Position = 2, ParameterSetName = "WithCommand",
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.",
        AcceptWildcards = true)]
    [ValidatePattern(@"^.*\.*proj$")]
    public string? ProjectPath { get; set; } = "./*.*proj";

    // /// <summary>True if you want to push the build package to the Local feed; false otherwise. Defaults to true.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the Local feed; false otherwise. Defaults to true.")]
    [Alias("pl", "pshloc", "pushlocl", "pushloc", "plocal")]
    public bool PushLocal { get; set; } = true;

    /// <summary>True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.")]
    [Alias("pgh", "pshgh", "pushgh")]
    public bool PushGitHub { get; set; } = false;

    /// <summary>True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.")]
    [Alias("paz", "pshaz", "pushaz")]
    public bool PushAzure { get; set; } = false;

    /// <summary>True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.")]
    [Alias("pn", "png", "pnu", "pushng")]
    public bool PushNuGet { get; set; } = false;

    /// <summary>True if you want to restore the build package; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "True if you want to restore the build package; false otherwise. Defaults to false.")]
    [Alias("no-restore")]
    public bool NoRestore { get; set; } = false;

    /// <summary>True if you do NOT want to clean the output directory before building. False otherwise.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you do NOT want to clean the output directory before building. False otherwise.")]
    [Alias("no-clean", "nc")]
    public bool NoClean { get; set; } = false;

    /// <summary>The configuration to build with. Defaults to "Local"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("c")]
    public string Configuration { get; set; } = "Local";

    /// <summary>The verbosity of the build. Defaults to "minimal"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [ValidateSet("q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic")]
    public string Verbosity { get; set; } = "minimal";

    /// <summary>The targets to build. Defaults to "Build"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("t")]
    public string[] Target { get; set; } = new[] { "Build" };

    /// <summary>The version of the built package. Defaults to "0.0.1-Local"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("v")]
    public string? Version { get; set; } = null;

    /// <summary>The version of the built assembly file. Defaults to "0.0.1"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("av", "asmv")]
    public string? AssemblyVersion { get; set; } = null;

    /// <summmary> Prints a list of available targets without executing the actual build process.
    /// By default, the output is written to the console window.
    ///    If the path to an output file is provided that will be used instead.
    /// </summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("targets", "show-targets", "ts")]
    public string? PrintTargets { get; set; } = null;

    /// <summary>Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("p", "prop", "properties")]
    public string[] Property { get; set; } = Array.Empty<string>();

    /** </summary>
          Serializes all build events to a compressed binary file.
          By default the file is in the current directory and named
          "msbuild.binlog". The binary log is a detailed description
          of the build process that can later be used to reconstruct
          text logs and used by other analysis tools.A binary log
          is usually 10-20x smaller than the most detailed text
          diagnostic-level log, but it contains more information
      </summary>
      */
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("bl", "binlog")]
    public bool BinaryLogger { get; set; } = false;

    /// <summary>The path to the binary log file to create. If not specified, the default is msbuild.binlog in the current directory.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("blf", "binlog-file")]
    public string? BinaryLogFile { get; set; } = null;

    /// <summary>Prints the help text</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "Help")]
    [Alias("?")]
    public bool Help { get; set; } = false;

    protected override void BeginProcessing()
    {
        WriteVerbose("BeginProcessing() called");
        WriteVerbose("With arguments: ");
        WriteVerbose($"    Configuration: {Configuration}");
        WriteVerbose($"    Verbosity: {Verbosity}");
        WriteVerbose($"    Target: {string.Join(", ", Target)}");
        WriteVerbose($"    Version: {Version}");
        WriteVerbose($"    AssemblyVersion: {AssemblyVersion}");
        WriteVerbose($"    PrintTargets: {PrintTargets}");
        WriteVerbose($"    Property: {string.Join(", ", Property)}");
        WriteVerbose($"    BinaryLogger: {BinaryLogger}");
        WriteVerbose($"    BinaryLogFile: {BinaryLogFile}");
        WriteVerbose($"    Help: {Help}");
    }

    protected override void ProcessRecord()
    {
        this.WriteVerbose("ProcessRecord() called");
    }

    protected override void EndProcessing()
    {
        this.WriteVerbose("EndProcessing() called");
    }

    protected override void StopProcessing()
    {
        this.WriteVerbose("StopProcessing() called");
    }
}
