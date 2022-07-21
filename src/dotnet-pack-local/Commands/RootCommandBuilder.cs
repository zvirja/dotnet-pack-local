using System.CommandLine;
using System.CommandLine.Invocation;

namespace DotnetPackLocal.Commands;

internal class RootCommandBuilder
{
    private readonly PackRepoCommand _packRepoCommand;
    private readonly LastVersionCommand _lastVersionCommand;
    private readonly NuGetRepositoryPathCommand _nuGetRepositoryPathCommand;

    public RootCommandBuilder(PackRepoCommand packRepoCommand, LastVersionCommand lastVersionCommand, NuGetRepositoryPathCommand nuGetRepositoryPathCommand)
    {
        _packRepoCommand = packRepoCommand;
        _lastVersionCommand = lastVersionCommand;
        _nuGetRepositoryPathCommand = nuGetRepositoryPathCommand;
    }

    public RootCommand BuildRootCommand(string nugetRepositoryPath)
    {
        var rootCmd = new RootCommand("Pack current folder as NuGet")
        {
            new Option<string>(new[] {"--output", "-o"}, () => nugetRepositoryPath, "Output path for NuGet packages"),
            new Option<bool>("--release", "Pack project in release mode"),
            new Option<bool?>("--symbols", () => null, "Specify whether to include symbols. By default included in DEBUG build only"),

            new Command("get-last-version", "Get last version for current repo")
            {
                Handler = CommandHandler.Create(_lastVersionCommand.HandleGet)
            },
            new Command("set-last-version", "Set last version for current repo (interactive mode)")
            {
                Handler = CommandHandler.Create(_lastVersionCommand.HandleSet)
            },
            new Command("get-nuget-repo", "Get configured NuGet repository path")
            {
                Handler = CommandHandler.Create(_nuGetRepositoryPathCommand.HandleGet)
            },
            new Command("set-nuget-repo", "Set configured NuGet repository path (interactive mode)")
            {
                Handler = CommandHandler.Create(_nuGetRepositoryPathCommand.HandleSet)
            }
        };
        rootCmd.Handler = CommandHandler.Create<string, bool, bool?>(_packRepoCommand.Handle);

        return rootCmd;
    }
}