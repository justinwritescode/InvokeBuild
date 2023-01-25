@{
    ModuleVersion    = '0.0.1'
    GUID             = '270fdc85-f9a6-4285-9409-b1056558a54b'
    Author           = 'Justin Chase'
    CompanyName      = 'JustinWritesCode'
    Copyright        = '© 2023 Justin Chase <justin@justinwritescode.com>, All Rights Reserved'
    Description      = 'Builds a project using the dotnet cli'
    ScriptsToProcess = @(Join-Path $PSScriptRoot 'Invoke-Build.ps1')
    CmdletsToExport  = @('Invoke-Build')
    PrivateData      = @{
        PSData = @{
            ProjectUri = 'https://github.com/justinwritescode/InvokeBuild'
            License    = 'MIT'
            Tags       = @('build', 'dotnet', 'cli', 'nuget', 'package', 'push')
        }
    }
}
