using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Serilog.Log;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.CompleteBuild);

    [Solution] readonly Solution Solution;

    [CI] readonly GitHubActions GitHubActions;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(Name = "BuildVersion")] readonly string BuildVersionParam = "git";

    [Parameter(Name = "BuildNumber")] readonly int BuildNumberParam = 0;

    [Secret] [Parameter("API Key used to publish package to NuGet", Name = "NUGET_KEY")] readonly string NuGetKey;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";

    BuildVersionInfo CurrentBuildVersion;

    Target CalculateVersion => _ => _
        .Executes(() =>
        {
            Information($"Build version: {BuildVersionParam}");

            CurrentBuildVersion = BuildVersionParam switch
            {
                "git" => GitBasedVersion.CalculateVersionFromGit(BuildNumberParam),

                var ver => new BuildVersionInfo { AssemblyVersion = ver, FileVersion = ver, InfoVersion = ver, NuGetVersion = ver }
            };

            Information($"Calculated version: {CurrentBuildVersion}");
        });

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ArtifactsDir.CreateOrCleanDirectory();
        });

    Target Prepare => _ => _
        .DependsOn(CalculateVersion, Clean)
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Prepare)
        .Executes(() =>
        {
            DotNetBuild(c => c
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution.Path)
                .SetVerbosity(DotNetVerbosity.minimal)
                .EnableContinuousIntegrationBuild()
                // version
                .SetVersion(CurrentBuildVersion.NuGetVersion)
                .SetAssemblyVersion(CurrentBuildVersion.AssemblyVersion)
                .SetFileVersion(CurrentBuildVersion.FileVersion)
                .SetInformationalVersion(CurrentBuildVersion.InfoVersion)
            );
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(c => c
                .SetConfiguration(Configuration)
                .SetProject(Solution.Path)
                .SetOutputDirectory(ArtifactsDir)
                .SetVerbosity(DotNetVerbosity.minimal)
                .EnableContinuousIntegrationBuild()
                .EnableNoRestore()
                // version
                .SetVersion(CurrentBuildVersion.NuGetVersion)
                .SetAssemblyVersion(CurrentBuildVersion.AssemblyVersion)
                .SetFileVersion(CurrentBuildVersion.FileVersion)
                .SetInformationalVersion(CurrentBuildVersion.InfoVersion)
            );
        });

    Target CompleteBuild => _ => _
        .DependsOn(Pack);

    Target PublishNuGet => _ => _
        .Requires(() => NuGetKey)
        .DependsOn(Pack)
        .Executes(() =>
        {
            var nugetPackage = ArtifactsDir.GlobFiles("*.nupkg").Single();
            DotNetNuGetPush(c => c
                .SetTargetPath(nugetPackage)
                .SetApiKey(NuGetKey)
                .SetSource("https://www.nuget.org/api/v2/package/")
            );
        });

    // ==============================================
    // ===================== CI =====================
    // ==============================================

    Target CI_DescribeState => _ => _
        .Before(Prepare)
        .Executes(() =>
        {
            var trigger = ResolveCITrigger();
            Information($"Build type: {GitHubActions.RefType}, Ref name: '{GitHubActions.RefName}', Is PR: {GitHubActions.IsPullRequest}, trigger: {trigger}");
        });

    Target CI_Pipeline => _ => _
        .DependsOn(ResolveCITarget(this), CI_DescribeState);

    static Target ResolveCITarget(Build build)
    {
        var trigger = ResolveCITrigger();
        return trigger switch
        {
            CITrigger.SemVerTag => build.PublishNuGet,
            _ => build.CompleteBuild
        };
    }

    enum CITrigger
    {
        Invalid,
        SemVerTag,
        PR,
        MainBranch,
        UnknownBranchOrTag
    }

    static CITrigger ResolveCITrigger()
    {
        var env = GitHubActions.Instance;
        if (env == null)
        {
            return CITrigger.Invalid;
        }

        var tag = env.RefType == "tag" ? env.RefName : null;
        var isPr = env.IsPullRequest;
        var branchName = env.RefName;

        return (tag, isPr, branchName) switch
        {
            (tag: { } t, _, _) when Regex.IsMatch(t, "^v\\d.*") => CITrigger.SemVerTag,
            (_, isPr: true, _) => CITrigger.PR,
            (_, _, branchName: "main") => CITrigger.MainBranch,
            _ => CITrigger.UnknownBranchOrTag
        };
    }
}
