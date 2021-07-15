using System;
using Spectre.Console;

namespace DotnetPackLocal.Commands
{
    internal class LastVersionCommand
    {
        private readonly IWorkingProject _workingProject;

        public LastVersionCommand(IWorkingProject workingProject)
        {
            _workingProject = workingProject;
        }

        public void HandleGet()
        {
            string workingDir = _workingProject.WorkingDir;
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", workingDir);

            string repoRoot = _workingProject.NormalizedRepoPath;
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            Version lastRepoVersion = _workingProject.GetLastVersion();
            AnsiConsole.MarkupLine("[olive]Last repo version:[/] {0}", lastRepoVersion);
        }

        public void HandleSet()
        {
            string workingDir = _workingProject.WorkingDir;
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", workingDir);

            string repoRoot = _workingProject.NormalizedRepoPath;
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            Version lastRepoVersion = _workingProject.GetLastVersion();
            AnsiConsole.MarkupLine("[olive]Last repo version:[/] {0}", lastRepoVersion);

            AnsiConsole.WriteLine();
            Version lastVersion = Prompts.AskForBaseProjectVersion();
            _workingProject.SetLastVersion(lastVersion);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Successfully set base version to:[/] {0}", lastVersion);
        }
    }
}