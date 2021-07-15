using System.CommandLine;
using System.Runtime.InteropServices;
using DotnetPackLocal;
using DotnetPackLocal.Commands;
using DotnetPackLocal.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    AnsiConsole.MarkupLine("[red]Only Windows OS is supported for now. Exiting.[/]");
    return 1;
}

ServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton<IWorkingProject, CurrentDirWorkingProject>()
    .AddSingleton<IConfigStore, RegistryConfigStore>()
    // Commands
    .AddSingleton<PackRepoCommand>()
    .AddSingleton<NuGetRepositoryPathCommand>()
    .AddSingleton<LastVersionCommand>()
    .AddSingleton<RootCommandBuilder>()
    //
    .BuildServiceProvider();

var configStore = serviceProvider.GetRequiredService<IConfigStore>();
string nugetLocalRepoPath = Prompts.OptionallyAskForValidNuGetRepoPath(configStore.GetLocalNuGetRepositoryPath());

RootCommand rootCmd = serviceProvider.GetRequiredService<RootCommandBuilder>().BuildRootCommand(nugetLocalRepoPath);
return rootCmd.Invoke(args);