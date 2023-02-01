namespace JustinWritesCode.PowerShell.Enums;
using System.Management.Automation;

[GenerateEnumerationRecordStruct(nameof(Verbosity), "JustinWritesCode.PowerShell")]
public enum Verbosity
{
    [Alias("q")]
    Quiet = 0,
    [Alias("m")]
    Minimal = 1,
    [Alias("n")]
    Normal = 2,
    [Alias("d")]
    Detailed = 3,
    [Alias("diag")]
    Diagnostic = 4
}
