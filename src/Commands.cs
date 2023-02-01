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


namespace JustinWritesCode.PowerShell.Enums;

[GenerateEnumerationRecordStruct("DotnetCommand", "JustinWritesCode.PowerShell")]
public enum DotnetCommand
{
    [Display(Name = "None", Description = "Don't use this.")]
    _,
    [Display(Name = "Build", Description = "Builds a project and its dependencies.")]
    build,
    [Display(Name = "Test", Description = "Runs unit tests in a project.")]
    test,
    [Display(Name = "Pack", Description = "Creates a NuGet package.")]
    pack,
    [Display(Name = "Publish", Description = "Publishes a project for deployment.")]
    publish,
    [Display(Name = "Clean", Description = "Cleans the output of a project.")]
    clean,
    [Display(Name = "Restore", Description = "Restores the dependencies and tools of a project.")]
    restore,
    [Display(Name = "Run", Description = "Runs an application without any explicit compile or launch commands.")]
    run,
    [Display(Name = "Help", Description = "Displays help for a command.")]
    help
}
