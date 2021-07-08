using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using LibGit2Sharp;
using Spectre.Console;

namespace DotnetPackLocal
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AnsiConsole.MarkupLine("[red]Only Windows OS is supported for now. Exiting.[/]");
                return 1;
            }

            var rootCommand = new RootCommand("Pack current folder as NuGet")
            {
                new Option<string>(new[] {"--output", "-o"}, GetNuGetLocalRepoPath, "Output path for NuGet packages"),
                new Option<bool>("--release", () => false, "Pack project in release mode"),
                new Option<bool?>("--symbols", () => null, "Include symbols. By default included in DEBUG build only"),
            };
            rootCommand.Handler = CommandHandler.Create<string, bool, bool?>(HandlePackRepo);

            return rootCommand.Invoke(args);
        }

        private static int HandlePackRepo(string output, bool release, bool? symbols)
        {
            AnsiConsole.MarkupLine("[olive]NuGet output folder:[/] {0}", output);

            var currentDir = Directory.GetCurrentDirectory();
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", currentDir);

            var repoRoot = GetRepoRoot();
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            var configuration = release ? "Release" : "Debug";
            AnsiConsole.MarkupLine("[olive]Build configuration:[/] {0}", configuration);

            var includeSymbols = symbols ?? !release;
            AnsiConsole.MarkupLine("[olive]Include symbols to package:[/] {0}", includeSymbols);

            var version = Configuration.GetNextVersion(repoRoot);
            AnsiConsole.MarkupLine("[green]Pack version:[/] {0}", version);

            AnsiConsole.WriteLine();

            var dotnetArgs = $"pack " +
                             $"/p:Version={version} " +
                             $"--output {output} " +
                             $"--configuration {configuration} " +
                             (includeSymbols ? "/p:AllowedOutputExtensionsInPackageBuildOutputFolder=\\\".dll;.exe;.json;.xml.pdb\\\"" : "") +
                             $"--verbosity=minimal ";
            AnsiConsole.MarkupLine("[grey]dotnet {0}[/]", dotnetArgs);

            var proc = Process.Start(new ProcessStartInfo("dotnet", dotnetArgs)
            {
                WorkingDirectory = currentDir
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

        private static string GetNuGetLocalRepoPath()
        {
            string? localNugetStore = Configuration.LocalNugetStore;
            for (;;)
            {
                if (localNugetStore == null)
                {
                    localNugetStore = AnsiConsole.Ask<string>("Please configure path to NuGet repo:");
                }

                if (Directory.Exists(localNugetStore))
                {
                    Configuration.LocalNugetStore = localNugetStore;
                    return localNugetStore;
                }

                AnsiConsole.MarkupLine("[red]Directory does not exist: {0}[/]", localNugetStore);
                localNugetStore = null;
            }
        }

        private static string GetRepoRoot()
        {
            var rootDir = Directory.GetCurrentDirectory();
            if (Repository.IsValid(rootDir))
            {
                rootDir = new Repository(rootDir).Info.WorkingDirectory;
            }

            return rootDir;
        }
    }
}