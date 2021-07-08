using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using LibGit2Sharp;
using Spectre.Console;
using Version = System.Version;

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

            var rootCmd = new RootCommand("Pack current folder as NuGet")
            {
                new Option<string>(new[] {"--output", "-o"}, GetNuGetLocalRepoPath, "Output path for NuGet packages"),
                new Option<bool>("--release", () => false, "Pack project in release mode"),
                new Option<bool?>("--symbols", () => null, "Include symbols. By default included in DEBUG build only"),

                new Command("set-last-version", "Set last version for current repo")
                {
                    Handler = CommandHandler.Create(HandleSetBaseVersion)
                },
                new Command("get-last-version", "Get last version for current repo")
                {
                    Handler = CommandHandler.Create(HandleGetBaseVersion)
                }
            };
            rootCmd.Handler = CommandHandler.Create<string, bool, bool?>(HandlePackRepo);

            return rootCmd.Invoke(args);
        }

        private static int HandlePackRepo(string output, bool release, bool? symbols)
        {
            AnsiConsole.MarkupLine("[olive]NuGet output folder:[/] {0}", output);

            var currentDir = Directory.GetCurrentDirectory();
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", currentDir);

            var repoRoot = GetNormalizedRepoRoot();
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            var configuration = release ? "Release" : "Debug";
            AnsiConsole.MarkupLine("[olive]Build configuration:[/] {0}", configuration);

            var includeSymbols = symbols ?? !release;
            AnsiConsole.MarkupLine("[olive]Include symbols to package:[/] {0}", includeSymbols);

            var version = Configuration.GetNextRepoVersion(repoRoot);
            AnsiConsole.MarkupLine("[green]Pack version:[/] {0}", version);

            AnsiConsole.WriteLine();

            var dotnetArgs = $"pack " +
                             $"/p:Version={version} " +
                             $"--output {output} " +
                             $"--configuration {configuration} " +
                             (includeSymbols ? "/p:AllowedOutputExtensionsInPackageBuildOutputFolder=\\\".dll;.exe;.json;.xml.pdb\\\" " : "") +
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

        private static void HandleSetBaseVersion()
        {
            string currentDir = Directory.GetCurrentDirectory();
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", currentDir);
 
            string repoRoot = GetNormalizedRepoRoot();
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            Version lastRepoVersion = Configuration.GetLastRepoVersion(repoRoot);
            AnsiConsole.MarkupLine("[olive]Last repo version:[/] {0}", lastRepoVersion);

            for (;;)
            {
                var newLastVerStr = AnsiConsole.Ask<string>("Enter new base version: ");
                if (Version.TryParse(newLastVerStr, out Version? newLastVer))
                {
                    Configuration.SetLastRepoVersion(repoRoot, newLastVer);
                    AnsiConsole.MarkupLine("[green]Successfully set base version to {0}[/]", newLastVer);
                    break;
                }

                AnsiConsole.MarkupLine("[red]Cannot parse value as valid version[/]");
            }
        }
        
        private static void HandleGetBaseVersion()
        {
            string currentDir = Directory.GetCurrentDirectory();
            AnsiConsole.MarkupLine("[olive]Current directory:[/] {0}", currentDir);
 
            string repoRoot = GetNormalizedRepoRoot();
            AnsiConsole.MarkupLine("[olive]Repo root:[/] {0}", repoRoot);

            Version lastRepoVersion = Configuration.GetLastRepoVersion(repoRoot);
            AnsiConsole.MarkupLine("[olive]Last repo version:[/] {0}", lastRepoVersion);
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

        private static string GetNormalizedRepoRoot()
        {
            var repoRoot = Directory.GetCurrentDirectory();
            if (Repository.IsValid(repoRoot))
            {
                repoRoot = new Repository(repoRoot).Info.WorkingDirectory;
            }
            
            repoRoot = repoRoot.ToLowerInvariant().TrimEnd('/').TrimEnd('\\');

            return repoRoot;
        }
    }
}