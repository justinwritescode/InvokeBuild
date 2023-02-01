@{
    RootModule         = "$PSScriptRoot/bin/InvokeDotnetCli.dll"
    ModuleVersion      = '0.0.1'
    GUID               = 'a177a1e3-44c4-48ec-a237-aa8a5a8fa985'
    Author             = 'Justin Chase'
    CompanyName        = 'JustinWritesCode'
    Copyright          = '© 2023 Justin Chase <justin@justinwritescode.com>, All Rights Reserved'
    Description        = 'Builds a project using the dotnet cli'
    CmdletsToExport    = @("Invoke-Build", "Invoke-Pack", "Invoke-Clean", "Invoke-Dotnet")
    # ScriptsToProcess  = @(Join-Path $PSScriptRoot 'buildandregister.ps1')
    # Variables to export from this module
    # VariablesToExport  = @('*')
    # Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
    # AliasesToExport    = @('*')
    # NestedModules      = @("$PSScriptRoot/bin/InvokeBuild.dll")
    RequiredAssemblies = @("$PSScriptRoot/bin/JustinWritesCode.PowerShell.dll", "$PSScriptRoot/bin/Microsoft.Build.dll", "$PSScriptRoot/bin/InvokeDotnetCli.dll", "$PSScriptRoot/bin/JustinWritesCode.Enumerations.Abstractions.dll")
    # RootModule        = @("$PSScriptRoot/bin/Local/InvokeBuild.dll")
    # FunctionsToExport = @('Invoke-Build', 'Invoke-Pack', 'Invoke-Function')
    PrivateData        = @{
        PSData       = @{
            ProjectUri = 'https://github.com/justinwritescode/InvokeBuild'
            License    = 'MIT'
            Tags       = @('build', 'dotnet', 'cli', 'nuget', 'package', 'push')
            LicenseUri = "https://opensource.org/lidenses/MIT"
        }
        PSModuleInfo = @{
            ProjectUri = 'https://github.com/justinwritescode/InvokeBuild'
            License    = 'MIT'
            Tags       = @('build', 'dotnet', 'cli', 'nuget', 'package', 'push')
            LicenseUri = "https://opensource.org/lidenses/MIT"
        }
    }
}
