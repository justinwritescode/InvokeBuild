/*
 * Build.cs
 *
 *   Created: 2023-01-28-11:19:07
 *   Modified: 2023-01-31-01:55:05
 *
 *   Author: Justin Chase <justin@justinwritescode.com>
 *
 *   Copyright © 2022-2023 Justin Chase, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustinWritesCode.PowerShell;

using System.Management.Automation;

[Cmdlet(VerbsLifecycle.Invoke, "Build", DefaultParameterSetName = "WithoutCommand")]
[Alias("ib", "build", "invokebuild")]
public class InvokeBuild : InvokeDotnet
{
    public InvokeBuild()
    {

    }

    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true, Mandatory = true, Position = 0,
        HelpMessage = "The path to the project file to build. Defaults to the first .*proj file in the current directory.")]
    [ValidatePattern(@"^(?:(?:.*\.*proj)|(?:.*\.*props)|(?:.*\.*targets)|(?:.*\.*usings)|(?:.*\.*tasks)|(?:.*\.*items))$")]
    [Alias("proj", "project", "path", "projpath")]
    public override string? ProjectPath { get; set; } = "./*.*proj";

    /// <summary>True if you do NOT want to clean the output directory before building. False otherwise.</summary>
    [Parameter(ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "True if you do NOT want to clean the output directory before building. False otherwise.")]
    [Alias("no-clean", "nc")]
    public bool NoClean { get; set; } = false;

    public override DotnetCommand Command { get => DotnetCommand.build.Instance; set { } }

    protected override void BeginProcessing()
    {
        WriteVerbose("BeginProcessing() called");
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

            var errors = new List<string>();
            var warnings = new List<string>();
            var informations = new List<string>();
            var verboses = new List<string>();
            var debugs = new List<string>();
            var @outs = new List<string>();

            WriteInformation(new InformationRecord($"Starting process: {dotnet} {Command} {args}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
            var ps = PowerShell.Create();
            ps = ps.AddCommand("Start-Process");
            ps = ps.AddParameter("FilePath", dotnet);
            ps = ps.AddParameter("ArgumentList", args);
            ps = ps.AddParameter("Wait", true);
            ps = ps.AddParameter("PassThru", true);
            ps = ps.AddParameter("RedirectStandardOutput", false);
            ps = ps.AddParameter("RedirectStandardError", false);
            ps = ps.AddParameter("NoNewWindow", true);
            ps = ps.AddParameter("UseNewEnvironment", true);

            WriteInformation(new InformationRecord($"Invoking process: {dotnet} {Command} with arguentd {args}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });

            var p = ps.Invoke(null, new PSInvocationSettings { Host = Host, AddToHistory = true });
            WriteInformation(new InformationRecord($"Finished process: {dotnet} {Command} {args}", "invokeBase.cs") { TimeGenerated = Now, User = Environment.UserName, Tags = { "Build" } });
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

    protected override void EndProcessing() => WriteVerbose("EndProcessing() called");

    protected override void StopProcessing() => WriteVerbose("StopProcessing() called");
}
