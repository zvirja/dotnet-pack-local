using System;
using Microsoft.Win32;

namespace DotnetPackLocal
{
    public static class Configuration
    {
        private const string RegistryStoreKey = @"SOFTWARE\Zvirja\DotnetPackLocal";

        public static string? LocalNugetStore
        {
            get => GetRegValue("LocalNuGetStore");
            set => SetRegValue("LocalNuGetStore", value);
        }

        public static Version GetLastRepoVersion(string normalizedRepoRoot)
        {
            string? lastKnownVersionStr = GetRegValue(normalizedRepoRoot);
            return lastKnownVersionStr != null
                ? Version.Parse(lastKnownVersionStr)
                : new Version(0, 99, 0);
        }

        public static void SetLastRepoVersion(string normalizedRepoRoot, Version version)
        {
            SetRegValue(normalizedRepoRoot, version.ToString());
        }

        public static Version GetNextRepoVersion(string normalizedRepoRoot)
        {
            Version lastKnownVersion = GetLastRepoVersion(normalizedRepoRoot);
            var nextVersion = new Version(lastKnownVersion.Major, lastKnownVersion.Minor, lastKnownVersion.Build + 1);
            SetLastRepoVersion(normalizedRepoRoot, nextVersion);
            return nextVersion;
        }

        private static void SetRegValue(string valueName, string? value)
        {
            using RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryStoreKey, writable: true)
                                       ?? Registry.CurrentUser.CreateSubKey(RegistryStoreKey, writable: true);

            if (value != null)
            {
                regKey.SetValue(valueName, value);
            }
            else
            {
                regKey.DeleteValue(valueName, throwOnMissingValue: false);
            }
        }

        private static string? GetRegValue(string valueName)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(RegistryStoreKey, writable: false);

            return regKey?.GetValue(valueName) as string;
        }
    }
}