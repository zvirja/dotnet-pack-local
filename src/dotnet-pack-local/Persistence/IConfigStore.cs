using System;

namespace DotnetPackLocal.Persistence
{
    internal interface IConfigStore
    {
        public string? GetLocalNuGetRepositoryPath();
        public void SetLocalNuGetRepositoryPath(string? value);

        public Version? GetLastVersionForProject(string repoPath);
        public void SetLastVersionForProject(Version version, string repoPath);
    }
}