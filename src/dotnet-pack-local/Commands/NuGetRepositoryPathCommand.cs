using DotnetPackLocal.Persistence;
using Spectre.Console;

namespace DotnetPackLocal.Commands;

internal class NuGetRepositoryPathCommand
{
    private readonly IConfigStore _configStore;

    public NuGetRepositoryPathCommand(IConfigStore configStore)
    {
        _configStore = configStore;
    }

    public void HandleGet()
    {
        AnsiConsole.MarkupLine("[olive]Configured NuGet repository path:[/] {0}", _configStore.GetLocalNuGetRepositoryPath() ?? "<not configured>");
    }

    public void HandleSet()
    {
        AnsiConsole.MarkupLine("[olive]Configured NuGet repository path:[/] {0}", _configStore.GetLocalNuGetRepositoryPath() ?? "<not configured>");

        AnsiConsole.WriteLine();
        string newPath = Prompts.OptionallyAskForValidNuGetRepoPath();
        _configStore.SetLocalNuGetRepositoryPath(newPath);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Successfully set NuGet repository path to:[/] {0}", newPath);
    }
}