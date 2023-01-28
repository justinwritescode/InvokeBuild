using namespace System.Management.Automation;

New-Variable -Name ProjectFilenameRegex "(?:.*\.*proj)|(?:.*\.sln)|(?:*.usings)|(?:*.tasks)|(?:*.props)|(?:*.targets)" -Option Constant
New-Variable -Name BuildCommandRegex "(?:build)|(?:pack)|(?:publish)|(?:restore)|(?:run)|(?:test)" -Option Constant
# New-Variable -Name BuildCommandOrProjectFilenameRegex = "^(?:$ProjectFilenameRegex)|(?:$BuildCommandRegex)$" -Option Constant

[CmdletAttribute("Do", "Foo")]
class DoFoo : System.Management.Automation.Cmdlet {
    [Parameter(ValueFromPipeline = $true,
        ValueFromPipelineByPropertyName = $true,
        HelpMessage = "The command to run.",
        Mandatory = $false,
        Position = 0)]
    [Alias("cmd")]
    [ValidateSet("build", "pack", "publish", "restore", "run", "test")]
    [string]$Command = "build"

    [object]GetDynamicParameters() {
        return $null;
    }

    [void]Process() {
        $this.WriteObject("Hello, World!");
    }
}

<#
    .SYNOPSIS
        Builds a project using the dotnet cli

    .DESCRIPTION
        Builds a project using the dotnet cli
            
    .EXAMPLE
        # Build the project and push the package to all four feeds
        Invoke-Build -Project "C:\Projects\MyProject\MyProject.csproj" -PushLocal -PushGitHub -PushAzure -PushNuGet
    .EXAMPLE
        # Build the project and push the package to the Local feed
        Invoke-Build -Project "C:\Projects\MyProject\MyProject.csproj" -PushLocal
    .EXAMPLE
        # Don't build the project; output the project's targets to the console
        Invoke-Build -Project "C:\Projects\MyProject\MyProject.csproj" -Targets 
    .EXAMPLE
        # Don't build the project; output the project's targets to a file instead
        Invoke-Build -Project "C:\Projects\MyProject\MyProject.csproj" -Targets:MyProject.targets

    .INPUTS
        None

    .OUTPUTS
        None
    
    .COMPONENT
        MSBuild, dotnet cli
#>
function Invoke-Build {
    [cmdletbinding()]
    [Alias("Build", "Dotnet-Build", "ib")]
    param(
        # The command to run or the project file to compile
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The command to run.",
            Mandatory = $false,
            Position = 0)]
        [Alias("cmd")]
        [ValidatePattern("^((?:build)|(?:pack)|(?:publish)|(?:restore)|(?:run)|(?:test)|(?:(?:.*\.*proj)|(?:.*\.sln)|(?:.*.usings)|(?:.*\.tasks)|(?:.*\.props)|(?:.*\.targets))$")]
        [string]$Command = "build",

        # The project file to compile
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The project file to compile.",
            Mandatory = $false,
            Position = 1)]
        [ValidatePattern("^((?:build)|(?:pack)|(?:publish)|(?:restore)|(?:run)|(?:test)|(?:(?:.*\.*proj)|(?:.*\.sln)|(?:.*.usings)|(?:.*\.tasks)|(?:.*\.props)|(?:.*\.targets))$")]
        [string]$ProjectFile = (Join-Path -path (Get-Location) -ChildPath "*.*proj"),

        # True if you want to push the build package to the Local feed; false otherwise. Defaults to true.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the Local feed; false otherwise. Defaults to true.")]
        [Alias("pl", "pshloc", "pushlocl", "pushloc" , "plocal")]
        [switch]$PushLocal = $false,

        # True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.")]
        [Alias("pgh", "pshgh", "pushgh")]
        [switch]$PushGitHub = $false,

        # True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.")]
        [Alias("paz", "pshaz", "pushaz")]
        [switch]$PushAzure = $false,

        # True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.")]
        [Alias("pn", "png", "pnu", "pushng")]
        [switch]$PushNuGet = $false,

        # True if you want to restore the build package; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to restore the build package; false otherwise. Defaults to false.")]
        [Alias("no-restore")]
        [switch]$NoRestore = $false,

        # True if you do NOT want to clean the output directory before building. False otherwise.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you do NOT want to clean the output directory before building. False otherwise.")]
        [Alias("no-clean", "nc")]
        [switch]$NoClean = $false,
    
        # The configuration to build with. Defaults to "Local"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("c")]
        [string]$Configuration = "Local",

        # The verbosity of the build. Defaults to "minimal"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [ValidateSet("q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic")]
        [string]$Verbosity = "minimal",

        # The targets to build. Defaults to "Build"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("t")]
        [string[]]$Target = @("Build"),

        # The version of the built package. Defaults to "0.0.1-Local"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("v")]
        [string]$Version = $null,

        # The version of the built assembly file. Defaults to "0.0.1"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("av", "asmv")]
        [string]$AssemblyVersion = $null,
    
        <# 
            Prints a list of available targets without executing the actual build process. 
            By default, the output is written to the console window. 
            If the path to an output file is provided that will be used instead. 
        #>
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("targets", "show-targets", "ts")]
        [string]$PrintTargets = $null,

        # Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately. 
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("p", "prop", "properties")]
        [string[]]$Property = @(),

        <#
            Serializes all build events to a compressed binary file.
            By default the file is in the current directory and named
            "msbuild.binlog". The binary log is a detailed description
            of the build process that can later be used to reconstruct
            text logs and used by other analysis tools. A binary log
            is usually 10-20x smaller than the most detailed text
            diagnostic-level log, but it contains more information
        #>
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("bl", "binlog")]
        [switch]$BinaryLogger = $false,

        # The path to the binary log file to create. If not specified, the default is msbuild.binlog in the current directory.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("blf", "binlog-file")]
        [string]$BinaryLogFile = $null,

        # Prints the help text
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            ParameterSetName = "Help")]
        [Alias("?")]
        [switch]$Help
    )

    # DynamicParam {
    #     $paramDictionary = [System.Management.Automation.RuntimeDefinedParameterDictionary]::new()
    #     if ([System.Text.RegularExpressions.Regex]::IsMatch($Command, "^(?:(?:.*\.*proj)|(?:.*\.sln)|(?:.*.usings)|(?:.*\.tasks)|(?:.*\.props)|(?:.*\.targets))$")) {
    #         $parameterAttribute = [System.Management.Automation.ParameterAttribute]@{
    #             # $parameterAttribute.ValueFromPipeline = $true;
    #             #     $parameterAttribute.ValueFromPipelineByPropertyName                                                        = $true
    #             #     $parameterAttribute.Mandatory                                                                              = $false
    #             #     $parameterAttribute.Position     
    #             Position         = 1
    #             ParameterSetName = "CommandNameInFirstSpot"
    #             HelpMessage      = "The path to the MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in 'proj' and uses that file."
    #         }

    #         $aliasAttribute = [System.Management.Automation.AliasAttribute]::new(@("project", "proj", "pf", "file", "f"));
    #         $patternValidationAttribute = [System.Management.Automation.ValidatePatternAttribute]::new("^.*\.(proj)$");
    #         $projectFileParameter = [System.Management.Automation.RuntimeDefinedParameter]::new("ProjectFile", [string], @($parameterAttribute, $aliasAttribute, $patternValidationAttribute))
    #         $projectFileParameter.Value = Join-Path Get-Location "/*.*proj";
    #         $ProjectFile = "$PSScriptRoot/*.*proj";
    #         $paramDictionary.Add("ProjectFile", $projectFileParameter)
    #     }
    #     else {
    #         $ProjectFile = $Command;
    #         $Command = "build";
    #     }
    #     return $paramDictionary
    # }

    begin {
        Write-Verbose "Invoking MSBuild..."
        # if()
        Write-Verbose "Command: $Command";
        Write-Verbose "ProjectFile: $ProjectFile";
    }
    process {
        if ($Help) {
            if (@("q", "quiet", "m", "minimal").Contains($verbosity)) {
                Get-Help $MyInvocation.MyCommand.Name;
            }
            elseif (@("detailed", "d").Contains($Verbosity)) {
                Get-Help $MyInvocation.MyCommand.Name -Detailed;
            }
            else {
                Get-Help $MyInvocation.MyCommand.Name -Full;
            }
            return;
        }

        if (!$NoClean) {
            Write-Verbose "Cleaning the output directory...";
            rm -rf ./bin;
            rm -rf ./obj;
        }

        $Property = $Property -join ", ";

        Write-Verbose "Executing the following command: `
        dotnet $command `
            $ProjectFile `
            --nologo -v:$Verbosity `
            -c:$Configuration `
        $((-not $Property -eq '' -and -not $null -eq $Property) ? '-p:$Property' : '') `
        $($PushLocal ? '-t:PushLocal' : '') `
        $($PushGitHub ? '-t:PushGitHub' : '') `
        $($PushAzure ? '-t:PushAzure' : '') `
        $($PushNuGet ? '-t:PushNuGet' : '') `
        $($BinaryLogger ? '-bl' : '')   `
        $($BinaryLogFile ? '-bl:$BinaryLogFile' : '')   `
        "


        dotnet $command `
            $ProjectFile `
            --nologo -v:$Verbosity `
            -c:$Configuration `
        $((-not $Property -eq "" -and -not $null -eq $Property) ? "-p:$Property" : "") `
        $($PushLocal ? "-t:PushLocal" : "") `
        $($PushGitHub ? "-t:PushGitHub" : "") `
        $($PushAzure ? "-t:PushAzure" : "") `
        $($PushNuGet ? "-t:PushNuGet" : "") `
        $($BinaryLogger ? "-bl" : '')   `
        $($BinaryLogFile ? "-bl:$BinaryLogFile" : "")   `
            # $(-not [string]::IsNullOrWhiteSpace(($BinaryLogFile) ? "-bl:$BinaryLogFile" : 
            #         $BinaryLogger -eq $true ? "-bl" : 
        #         ""));
    }
    
    end {
    }
}

<#
    .SYNOPSIS
        Packages a project using the dotnet cli

    .DESCRIPTION
        Packages a project using the dotnet cli
            
    .EXAMPLE
        # Build the project and push the package to all four feeds
        Invoke-Pack -Project "C:\Projects\MyProject\MyProject.csproj" -PushLocal -PushGitHub -PushAzure -PushNuGet
    .EXAMPLE
        # Build the project and push the package to the Local feed
        Invoke-Pack -Project "C:\Projects\MyProject\MyProject.csproj" -PushLocal
    .EXAMPLE
        # Don't build the project; output the project's targets to the console
        Invoke-Pack -Project "C:\Projects\MyProject\MyProject.csproj" -Targets 
    .EXAMPLE
        # Don't build the project; output the project's targets to a file instead
        Invoke-Pack -Project "C:\Projects\MyProject\MyProject.csproj" -Targets:MyProject.targets

    .INPUTS
        None

    .OUTPUTS
        None
    
    .COMPONENT
        MSBuild, dotnet cli, nuget
#>
function Invoke-Pack {
    [Alias("Pack", "Dotnet-Pack", "pk")]
    param(
        # The path to the MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in "proj" and uses that file.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            Mandatory = $false,
            Position = 0)]
        [Alias("project", "proj", "pf", "file", "f")]
        [ValidatePattern("^((?:.*\.*proj)|(?:.*\.sln)|(?:.*.usings)|(?:.*\.tasks)|(?:.*\.props)|(?:.*\.targets))$")]
        [string]
        $ProjectFile,

        # True if you want to push the build package to the Local feed; false otherwise. Defaults to true.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the Local feed; false otherwise. Defaults to true.")]
        [Alias("pl", "pshloc", "pushlocl", "pushloc" , "plocal")]
        [switch]$PushLocal = $false,

        # True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the GitHub NuGet feed; false otherwise. Defaults to false.")]
        [Alias("pgh", "pshgh", "pushgh")]
        [switch]$PushGitHub = $false,

        # True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the Azure Artifacts NuGet feed; false otherwise. Defaults to false.")]
        [Alias("paz", "pshaz", "pushaz")]
        [switch]$PushAzure = $false,

        # True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to push the build package to the NuGet.org feed; false otherwise. Defaults to false.")]
        [Alias("pn", "png", "pnu", "pushng")]
        [switch]$PushNuGet = $false,

        # True if you want to restore the build package; false otherwise. Defaults to false.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you want to restore the build package; false otherwise. Defaults to false.")]
        [Alias("no-restore")]
        [switch]$NoRestore = $false,

        # True if you do NOT want to clean the output directory before building. False otherwise.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "True if you do NOT want to clean the output directory before building. False otherwise.")]
        [Alias("no-clean", "nc")]
        [switch]$NoClean = $false,
    
        # The configuration to build with. Defaults to "Local"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("c")]
        [string]$Configuration = "Local",

        # The verbosity of the build. Defaults to "minimal"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [ValidateSet("q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic")]
        [string]$Verbosity = "minimal",

        # The targets to build. Defaults to "Build"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("t")]
        [string[]]$Target = @("Build"),

        # The version of the built package. Defaults to "0.0.1-Local"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("v")]
        [string]$Version = $null,

        # The version of the built assembly file. Defaults to "0.0.1"
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("av", "asmv")]
        [string]$AssemblyVersion = $null,
    
        <# 
            Prints a list of available targets without executing the actual build process. 
            By default, the output is written to the console window. 
            If the path to an output file is provided that will be used instead. 
        #>
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("targets", "show-targets", "ts")]
        [string]$PrintTargets = $null,

        # Set or override these project-level properties. <n> is the property name, and <v> is the property value. Use a semicolon or a comma to separate multiple properties, or specify each property separately. 
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("p", "prop", "properties")]
        [string[]]$Property = @(),

        <#
            Serializes all build events to a compressed binary file.
            By default the file is in the current directory and named
            "msbuild.binlog". The binary log is a detailed description
            of the build process that can later be used to reconstruct
            text logs and used by other analysis tools. A binary log
            is usually 10-20x smaller than the most detailed text
            diagnostic-level log, but it contains more information
        #>
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("bl", "binlog")]
        [switch]$BinaryLogger = $false,

        # The path to the binary log file to create. If not specified, the default is msbuild.binlog in the current directory.
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [Alias("blf", "binlog-file")]
        [string]$BinaryLogFile = $null,

        # Prints the help text
        [Parameter(ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            ParameterSetName = "Help")]
        [Alias("?")]
        [switch]$Help
    )

    Invoke-Build $ProjectFile `
        -Command:pack `
        -ProjectFile:$ProjectFile `
        -Configuration $Configuration `
        -Verbosity $Verbosity `
        -Target $Target `
        -Version $Version `
        -AssemblyVersion $AssemblyVersion `
        -PrintTargets $PrintTargets `
        -NoRestore:$NoRestore `
        -NoClean:$NoClean `
        -PushLocal:$PushLocal `
        -PushGitHub:$PushGitHub `
        -PushAzure:$PushAzure `
        -PushNuGet:$PushNuGet `
        -Property:$Property `
        -BinaryLogger:$BinaryLogger `
        -BinaryLogFile:$BinaryLogFile `
        -Help:$Help
}
