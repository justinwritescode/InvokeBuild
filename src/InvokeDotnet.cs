using System.Collections.Generic;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;
/*
 * InvokeBase.cs
 *
 *   Created: 2023-01-26-04:55:59
 *   Modified: 2023-01-31-01:54:28
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */


namespace JustinWritesCode.PowerShell;

using System.Collections.Concurrent;
using System.Management.Automation;           // Windows PowerShell namespace.
using System.Text;
using Microsoft.PowerShell.Commands;
using static JustinWritesCode.PowerShell.PowerShellExtensions;

[Cmdlet(VerbsLifecycle.Invoke, "Dotnet", DefaultParameterSetName = "Build")]
public class InvokeDotnet : PSCmdlet
{
    public InvokeDotnet()
    {
    }

    protected const string dotnet = nameof(dotnet);

    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true, Mandatory = true, Position = 1, ParameterSetName = "Build",
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.")]
    [ValidatePattern(@"^(?:(?:.*\.*proj)|(?:.*\.*props)|(?:.*\.*targets)|(?:.*\.*usings)|(?:.*\.*tasks)|(?:.*\.*items))$")]
    [Alias("proj", "project", "path", "projpath")]
    public virtual string? ProjectPath { get; set; } = "./*.*proj";

    [Parameter(ValueFromPipeline = true,
        Position = 0,
        ValueFromPipelineByPropertyName = true, Mandatory = false,
        HelpMessage = "The command to run. Defaults to 'build'.",
        ParameterSetName = "Build")]
    [ValidateSet("build", "test", "pack", "publish", "clean", "restore", "run", "help")]
    [Alias("c", "cmd", "command")]
    public virtual DotnetCommand Command { get; set; } = DotnetCommand.build.Instance;

    // /// <summary>True if you want to push the build package to the Local feed; false otherwise. Defaults to true.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the Local feed; false otherwise. Defaults to true.",
        ParameterSetName = "Push")]
    [Alias("pl", "pshloc", "pushlocl", "pushloc", "plocal")]
    public SwitchParameter PushLocal { get; set; } = true;

    /// <summary>True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.",
        ParameterSetName = "Push")]
    [Alias("pgh", "pshgh", "pushgh")]
    public SwitchParameter PushGitHub { get; set; } = false;

    /// <summary>True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "Push")]
    [Alias("paz", "pshaz", "pushaz")]
    public SwitchParameter PushAzure { get; set; } = false;

    /// <summary>True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "Push")]
    [Alias("pn", "png", "pnu", "pushng")]
    public SwitchParameter PushNuGet { get; set; } = false;

    /// <summary>True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "Push")]
    [Alias("nl", "no-logo")]
    public SwitchParameter NoLogo { get; set; } = false;

    /// <summary>True if you want to restore the build package; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "True if you want to restore the build package; false otherwise. Defaults to false.",
            ParameterSetName = "Build")]
    [Alias("no-restore")]
    public SwitchParameter NoRestore { get; set; } = false;

    /// <summary>True if you want to restore the build package; false otherwise. Defaults to false.</summary>
    [Parameter(ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Allows the command to stop and wait for user input or action (for example to complete authentication).",
            ParameterSetName = "Build")]
    [Alias("inter")]
    public SwitchParameter Interactive { get; set; } = false;

    /// <summary>The configuration to build with. Defaults to "Local"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The configuration to build with. Defaults to \"Local.\"",
        ParameterSetName = "Build")]
    [Alias("c")]
    public string Configuration { get; set; } = "Local";

    /// <summary>The verbosity of the build. Defaults to "minimal"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = false,
        HelpMessage = "The verbosity of the build. Defaults to \"minimal.\"",
        ParameterSetName = "Build")]
    [ValidateSet("q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic")]
    public string Verbosity { get; set; } = "minimal";

    /// <summary>The targets to build. Defaults to "Build"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = false,
        HelpMessage = "The targets to build. Defaults to \"Build.\"",
        ParameterSetName = "Build")]
    [Alias("t")]
    public string[] Target { get; set; } = new[] { "Build" };

    /// <summary>The version of the built package. Defaults to "0.0.1-Local"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = false,
        HelpMessage = "The version of the built package. Defaults to \"0.0.1-Local\"",
        ParameterSetName = "Build")]
    [Alias("v")]
    public string? Version { get; set; } = null;

    /// <summary>The version of the built assembly file. Defaults to "0.0.1"</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = false,
        HelpMessage = "The version of the built assembly file. Defaults to \"0.0.1\".",
        ParameterSetName = "Build")]
    [Alias("av", "asmv")]
    public string? AssemblyVersion { get; set; } = null;

    /// <summmary> Prints a list of available targets without executing the actual build process.
    /// By default, the output is written to the console window.
    ///    If the path to an output file is provided that will be used instead.
    /// </summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("targets", "show-targets", "ts")]
    [Parameter(Mandatory = false,
        HelpMessage = "Prints a list of available targets without executing the actual build process. By default, the output is written to the console window. If the path to an output file is provided that will be used instead.",
        ParameterSetName = "Build")]
    public StringSwitch PrintTargets { get; set; } = default;

    /// <summary>Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("prop", "properties")]
    [Parameter(Mandatory = false,
        HelpMessage = "Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately.",
        ParameterSetName = "Build")]
    public string[] Property { get; set; } = Empty<string>();

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
    [Parameter(Mandatory = false,
        HelpMessage = "The command to run. Defaults to 'build'.",
        ParameterSetName = "Build")]
    [Alias("bl", "binlog")]
    public StringSwitch BinaryLogger { get; set; } = false;

    // /// <summary>The path to the binary log file to create. If not specified, the default is msbuild.binlog in the current directory.</summary>
    // [Parameter(ValueFromPipeline = true,
    //     ValueFromPipelineByPropertyName = true)]
    // [Parameter(Mandatory = false,
    //     HelpMessage = "The command to run. Defaults to 'build'.",
    //     ParameterSetName = "Publish")]
    // [Alias("blf", "binlog-file")]
    // public string? BinaryLogFile { get; set; } = null;

    /// <summary>Prints the help text</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("?", "h")]
    public SwitchParameter Help { get; set; } = false;

    /// <summary>Adds tags to the build</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "Build")]
    [Alias("tag")]
    public string[] Tags { get; set; } = new[] { "Build" };

    public int ExitCode { get; private set; } = -1;

    protected IEnumerable<object?> Arguments
    {
        get
        {
            yield return Command;

            yield return ProjectPath;

            if (PrintTargets.IsPresent)
            {
                if (PrintTargets == "")
                {
                    yield return "-targets";
                }
                else
                {
                    yield return "-targets";
                    yield return PrintTargets;
                }
            }

            if (Target != null)
            {
                foreach (var target in Target)
                {
                    yield return "-t";
                    yield return target;
                }
            }

            if (Version != null)
            {
                yield return $"-p:Version={Version}";
            }

            if (AssemblyVersion != null)
            {
                yield return $"-p:AsssemblyVersion={AssemblyVersion}";
            }

            if (Property != null)
            {
                foreach (var property in Property)
                {
                    yield return "-p";
                    yield return property;
                }
            }

            if (NoRestore)
            {
                yield return "-no-restore";
            }

            if (NoLogo)
            {
                yield return "-norestore";
            }

            if (BinaryLogger)
            {
                yield return "-bl";
                if (BinaryLogger != "")
                {
                    yield return BinaryLogger;
                }
            }

            if (Verbosity != null)
            {
                yield return "-v";
                yield return Verbosity;
            }
        }
    }

    protected virtual void GetHelp()
    {
        var help = new StringBuilder();
        help.AppendLine("Builds a .NET project.");
        help.AppendLine("Usage: dotnet build [options] [project]");
        help.AppendLine("Options:");
        help.AppendLine("  -h, --help, -?, /?  Prints the help text");
        help.AppendLine("  -t, --target        The targets to build. Defaults to \"Build.\"");
        help.AppendLine("  -v, --version       The version of the built package. Defaults to \"0.0.1-Local\"");
        help.AppendLine("  -av, --asmv         The version of the built assembly file. Defaults to \"0.0.1\"");
        help.AppendLine("  -ts, --targets      Prints a list of available targets without executing the actual build process. By default, the output is written to the console window. If the path to an output file is provided that will be used instead.");
        help.AppendLine("  -prop, --properties Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately.");
        help.AppendLine("  -bl, --binlog       Serializes all build events to a compressed binary file. By default the file is in the current directory and named \"msbuild.binlog\". The binary log is a detailed description of the build process that can later be used to reconstruct text logs and used by other analysis tools. A binary log is usually 10-20x smaller than the most detailed text diagnostic-level log, but it contains more information");
        help.AppendLine("  -tag                Adds tags to the build");
        help.AppendLine("  -c, --configuration The configuration to build. Defaults to \"Release\".");
        help.AppendLine("  -o, --output        The output directory. Defaults to \"./bin/Release\".");
        help.AppendLine("  -f, --framework     The target framework to build. Defaults to \"netstandard2.0\".");
        help.AppendLine("  -r, --runtime       The target runtime to build. Defaults to \"win-x64\".");
        help.AppendLine("  -p, --project       The project to build. Defaults to the current directory.");
        help.AppendLine("  -n, --no-restore    Do not restore the project before building.");
        help.AppendLine("  -nr, --no-restore   Do not restore the project before building.");
        help.AppendLine("  -d, --diagnostics   Enable diagnostic output.");
        help.AppendLine("  -l, --log           The log file to write to. Defaults to \"msbuild.log\" in the current directory.");
        help.AppendLine("  -w, --warnaserror   Treat warnings as errors.");
        help.AppendLine("  -q, --quiet         Do not log anything to the console.");
        help.AppendLine("  -vq, --verbosity    Set the verbosity level. Defaults to \"minimal\". Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].");

    }

    protected override void ProcessRecord()
    {
        WriteVerbose("ProcessRecord() started");

        if (Help)
        {
            GetHelp();
            return;
        }

        try
        {
            var properties = new StringDictionary();
            if (Version != null)
            {
                properties.Add("Version", Version);
                properties.Add("PackageVersionOverride", Version);
            }

            if (AssemblyVersion != null)
            {
                properties.Add("AssemblyVersion", AssemblyVersion);
            }

            if (Property != null)
            {
                foreach (var prop in Property)
                {
                    var split = prop.Split('=');
                    properties.Add(split.First(), split.Skip(1).FirstOrDefault() ?? "");
                }
            }

            var targets = new List<string>();
            if (Target != null)
            {
                targets.AddRange(Target);
            }

            var args = new List<string>
            {
                "build",
                ProjectPath ?? "*.*proj",
                $"--configuration:{Configuration}",
                $"--verbosity:{Verbosity}"
            };
            if (NoRestore)
            {
                args.Add("--no-restore");
            }
            if (BinaryLogger)
            {
                args.Add("--binaryLogger" + (BinaryLogger.HasValue ? "" : $":{BinaryLogger}"));
            }
            if (PrintTargets)
            {
                args.Add("--targets");
            }

            if (Target != null)
            {
                args.Add(Join(" ", Target.Select(t => $"-t:{t}")));
            }

            WriteVerbose($"Args: {Join(" ", args)}");


            var errors = new List<string>();
            var warnings = new List<string>();
            var informations = new List<string>();
            var verboses = new List<string>();
            var debugs = new List<string>();
            var @outs = new List<string>();

            WriteInformation(new InformationRecord($"Starting process: {dotnet} {Command} {args}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
            var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps = ps.AddCommand("Start-Process");
            ps = ps.AddParameter("FilePath", dotnet);
            ps = ps.AddParameter("ArgumentList", args);
            ps = ps.AddParameter("Wait", true);
            ps = ps.AddParameter("PassThru", true);
            // ps = ps.AddParameter("RedirectStandardOutput", "msbuild.out.log");
            // ps = ps.AddParameter("RedirectStandardError", false);
            ps = ps.AddParameter("NoNewWindow", true);
            ps = ps.AddParameter("UseNewEnvironment", false);
            ps.Streams.Error.DataAdded += (sender, e) =>
            {
                var error = ps.Streams.Error[e.Index];
                WriteObject(error);
            };
            ps.Streams.Warning.DataAdded += (sender, e) =>
            {
                var warning = ps.Streams.Warning[e.Index];
                WriteObject(warning.ToString());
            };
            ps.Streams.Information.DataAdded += (sender, e) =>
            {
                var information = ps.Streams.Information[e.Index];
                WriteObject(information);
            };
            ps.Streams.Verbose.DataAdded += (sender, e) =>
            {
                var verbose = ps.Streams.Verbose[e.Index];
                WriteObject(verbose.ToString());
            };
            ps.Streams.Debug.DataAdded += (sender, e) =>
            {
                var debug = ps.Streams.Debug[e.Index];
                WriteObject(debug.ToString());
            };



            WriteObject(new InformationRecord($"Invoking process: {dotnet} {Command} with arguents {Join(" ", args)}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });

            var p = ps.Invoke();
            WriteObject(new InformationRecord($"Finished process: {dotnet} {Command} {Join(" ", args)}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
            ForEach(p.ToArray(), p => WriteObject(p));
            // p[0]. += (sender, e) =>
            // {
            //     if (e.Data != null)
            //     {
            //         errors.Add(e.Data);
            //     }
            // };
            // p[0].OutputDataReceived += (sender, e) =>
            // {
            //     if (e.Data != null)
            //     {
            //         @outs.Add(e.Data);
            //     }
            // };
            // if (p.Count > 0)
            // {
            //     ExitCode = p[0].ExitCode;
            // }
            // else
            // {
            //     ExitCode = -1;
            // }

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    WriteError(new ErrorRecord(new Exception(error), "Error", ErrorCategory.NotSpecified, null));
                }
            }

            if (warnings != null)
            {
                foreach (var warning in warnings)
                {
                    WriteWarning(warning);
                }
            }

            if (@outs != null)
            {
                foreach (var information in @outs)
                {
                    WriteInformation(new InformationRecord(information, "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
                }
            }

            // var sjc = new StartJobCommand();
            // sjc.Name = dotnet;
            // sjc.ArgumentList = args.ToArray();
            // sjc.AllowRedirection = true;
            // sjc.ApplicationName = dotnet;
            // sjc.ConfigurationName = Configuration;
            // sjc.FilePath = dotnet;

            // WriteInformation(new InformationRecord($"Starting process: {sjc.Name} {sjc.ArgumentList}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
            // sjc.Invoke();


            // var psi = new ProcessStartInfo
            // {
            //     FileName = "dotnet",
            //     Arguments = Join(" ", args),
            //     UseShellExecute = false,
            //     RedirectStandardOutput = true,
            //     RedirectStandardError = true,
            //     CreateNoWindow = true,
            // };

            // WriteInformation(new InformationRecord($"Starting process: {psi.FileName} {psi.Arguments}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
            // var outputQueue = new ConcurrentQueue<InformationRecord>();
            // var errorQ = new ConcurrentQueue<ErrorRecord>();
            // var p = Process.Start(psi);
            // p.OutputDataReceived += InformationReceived;// /WriteInformation(new InformationRecord(e.Data, "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" }, ProcessId = (uint)p.Id });
            // p.ErrorDataReceived += ErrorReceived;
            // //WriteError(new ErrorRecord(new BuildException(e.Data), "BuildError: " + e.Data, ErrorCategory.NotSpecified, ProjectPath));
            // // {
            // //     errorQ.Enqueue(new ErrorRecord(new BuildException("e.Data.GetType(): " + e.Data?.GetType() + e.Data), "BuildError: " + e.Data, ErrorCategory.NotSpecified, ProjectPath));
            // // };
            // p.BeginOutputReadLine();
            // for (outputQueue.TryDequeue(out var e); outputQueue.TryDequeue(out e) && !p.HasExited;)
            //     if (e != null)
            //     {
            //         WriteInformation(e);
            //     }
            // p.BeginErrorReadLine();
            // for (errorQ.TryDequeue(out var e); errorQ.TryDequeue(out e) && !p.HasExited;)
            //     if (e != null)
            //     {
            //         WriteError(e);
            //     }
            // for (var line = await p.StandardOutput.ReadLineAsync(); line != null && p.HasExited; line = await p.StandardOutput.ReadLineAsync())
            // {
            //     WriteInformation(new InformationRecord(line, "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" }, ProcessId = (uint)p.Id });
            // }
            // for (var line = await p.StandardOutput.ReadLineAsync(); line != null && p.HasExited; line = await p.StandardError.ReadLineAsync())
            // {
            //     WriteError(new ErrorRecord(new BuildException(line), "BuildError: " + line, ErrorCategory.NotSpecified, ProjectPath) { ErrorDetails = new ErrorDetails(line) });
            // }
            // p.WaitForExit();
            // ExitCode = p.ExitCode;
            // WriteVerbose("Std Output:" + p.StandardOutput.ReadToEnd());

            // if (ExitCode != 0)
            // {
            //     WriteError(new ErrorRecord(new BuildException("Process exited with code: " + ExitCode), "BuildError: " + ExitCode, ErrorCategory.NotSpecified, ProjectPath) { ErrorDetails = new ErrorDetails("Process exited with code: " + ExitCode) });
            // }
            // else
            // {
            //     this.WriteProgress(new ProgressRecord(ExitCode, "Process exited", "Process exited with code: " + ExitCode));
            // }

            // while (outputQueue.TryDequeue(out var line))
            // {
            //     WriteObject(line);
            // }

            // while (errorQ.TryDequeue(out var error))
            // {
            //     WriteError(error);
            // }
            // }).Wait();
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "BuildError: " + ex.Message, ErrorCategory.NotSpecified, ProjectPath) { ErrorDetails = new ErrorDetails(ex.Message) });
        }
    }

    private ConcurrentQueue<ErrorRecord> Errors { get; } = new ConcurrentQueue<ErrorRecord>();
    private ConcurrentQueue<InformationRecord> InformationMessages { get; } = new ConcurrentQueue<InformationRecord>();

    private void InformationReceived(object sender, DataReceivedEventArgs e)
    {
        InformationMessages.Enqueue(new InformationRecord(e.Data, "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, ProcessId = (uint)((Process)sender).Id });
    }

    private void ErrorReceived(object sender, DataReceivedEventArgs e)
    {
        Errors.Enqueue(new ErrorRecord(new BuildException(e.Data), "BuildError: " + e.Data, ErrorCategory.NotSpecified, ProjectPath) { ErrorDetails = new ErrorDetails(e.Data) });
    }

    protected override void EndProcessing()
    {
        WriteVerbose("EndProcessing() called");
    }

    protected override void StopProcessing()
    {
        WriteVerbose("StopProcessing() called");
    }
}
