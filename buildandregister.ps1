try {
    Remove-Module InvokeDotnetCli -Force -Verbose;
}
catch {
    Write-Output "InvokeDotnetCli module not found; skipped.";
}
dotnet build ./src/InvokeDotnetCli.csproj --nologo -v:normal -nowarn:README.MD_NOT_FOUND
Import-Module $PSScriptRoot/InvokeDotnetCli.psd1 -Verbose;
# Publish-Module -Path ./ -Verbose;
# Register-PSRepository -Name 'PSGallery' -SourceLocation 'https://www.powershellgallery.com/api/v2' -InstallationPolicy Trusted -Verbose;
