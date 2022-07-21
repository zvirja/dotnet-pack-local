using System;
using System.IO;
using Spectre.Console;

namespace DotnetPackLocal;

internal static class Prompts
{
    public static string OptionallyAskForValidNuGetRepoPath(string? currentValue = null)
    {
        for (;;)
        {
            if (currentValue == null)
            {
                currentValue = AnsiConsole.Ask<string>("Please configure path to NuGet repo:");
            }

            if (Directory.Exists(currentValue))
            {
                return currentValue;
            }

            AnsiConsole.MarkupLine("[red]Directory does not exist: {0}[/]", currentValue);
            currentValue = null;
        }
    }

    public static Version AskForBaseProjectVersion()
    {
        for (;;)
        {
            var newLastVerStr = AnsiConsole.Ask<string>("Enter new base version: ");
            if (Version.TryParse(newLastVerStr, out Version? version))
            {
                return version;
            }

            AnsiConsole.MarkupLine("[red]Cannot parse value as valid version[/]");
        }
    }
}
