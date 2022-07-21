using System.Diagnostics;
using Spectre.Console;

namespace DotnetPackLocal.Commands
{
    internal class PackRepoCommand
    {
        private readonly IWorkingProject _workingProject;

        public PackRepoCommand(IWorkingProject workingProject)
        {
            _workingProject = workingProject;
        }

        public int Handle(string output, bool release, bool? symbols)
        {
            AnsiConsole.MarkupLine("[olive]NuGet output folder:[/] {0}", output);

            var workingDir = _workingProject.WorkingDir;
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", workingDir);

            var repoRoot = _workingProject.NormalizedRepoPath;
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            var configuration = release ? "Release" : "Debug";
            AnsiConsole.MarkupLine("[olive]Build configuration:[/] {0}", configuration);

            var includeSymbols = symbols ?? !release;
            AnsiConsole.MarkupLine("[olive]Include symbols to package:[/] {0}", includeSymbols);

            var version = _workingProject.AdvanceVersion();
            AnsiConsole.MarkupLine("[green]Pack version:[/] {0}", version);

            AnsiConsole.WriteLine();

            var dotnetArgs = $"pack " +
                             $"/p:Version={version} " +
                             $"--output \"{output}\" " +
                             $"--configuration {configuration} " +
                             (includeSymbols ? "/p:AllowedOutputExtensionsInPackageBuildOutputFolder=\\\".dll;.exe;.json;.xml;.pdb\\\" " : "") +
                             $"--verbosity=minimal ";
            AnsiConsole.MarkupLine("[grey]dotnet {0}[/]", dotnetArgs);

            var proc = Process.Start(new ProcessStartInfo("dotnet", dotnetArgs)
            {
                WorkingDirectory = workingDir
            });
            if (proc == null)
            {
                AnsiConsole.MarkupLine("[red]Unable to start dotnet process[/]");
                return 2;
            }

            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                AnsiConsole.MarkupLine("[red]dotnet pack exited with error[/]");
                return 3;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green1]Successfully packed![/] Version: [bold]{0}[/]", version);
            return 0;
        }
    }
}
