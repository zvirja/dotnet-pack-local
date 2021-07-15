using System;

namespace DotnetPackLocal
{
    internal interface IWorkingProject
    {
        public string WorkingDir { get; }
        public string NormalizedRepoPath { get; }

        public Version GetLastVersion();
        public void SetLastVersion(Version version);

        public Version AdvanceVersion();
    }
}