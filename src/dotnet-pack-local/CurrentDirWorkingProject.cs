using System.IO;
using DotnetPackLocal.Persistence;
using LibGit2Sharp;
using Version = System.Version;

namespace DotnetPackLocal
{
    internal class CurrentDirWorkingProject : IWorkingProject
    {
        private readonly IConfigStore _configStore;

        public string WorkingDir { get; }
        public string NormalizedRepoPath { get; }

        public CurrentDirWorkingProject(IConfigStore configStore)
        {
            _configStore = configStore;

            WorkingDir = Directory.GetCurrentDirectory();
            NormalizedRepoPath = GetNormalizedRepoRoot(WorkingDir);
        }

        public Version GetLastVersion() => _configStore.GetLastVersionForProject(NormalizedRepoPath) ?? new Version(0, 99, 0);

        public void SetLastVersion(Version version) => _configStore.SetLastVersionForProject(version, NormalizedRepoPath);

        public Version AdvanceVersion()
        {
            var lastVersion = GetLastVersion();
            var nextVersion = new Version(lastVersion.Major, lastVersion.Minor, lastVersion.Build + 1);
            SetLastVersion(nextVersion);

            return nextVersion;
        }

        private static string GetNormalizedRepoRoot(string workingDir)
        {
            var repoRoot = workingDir;
            if (Repository.IsValid(repoRoot))
            {
                repoRoot = new Repository(repoRoot).Info.WorkingDirectory;
            }

            repoRoot = repoRoot.ToLowerInvariant().TrimEnd('/').TrimEnd('\\');

            return repoRoot;
        }
    }
}