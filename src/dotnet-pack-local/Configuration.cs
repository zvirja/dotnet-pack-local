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

        public static Version GetNextVersion(string repoRoot)
        {
            repoRoot = repoRoot.ToLowerInvariant().TrimEnd('/').TrimEnd('\\');

            string? lastKnownVersionStr = GetRegValue(repoRoot);
            var lastKnownVersion = lastKnownVersionStr != null
                ? Version.Parse(lastKnownVersionStr)
                : new Version(0, 99, 0);

            var nextVersion = new Version(lastKnownVersion.Major, lastKnownVersion.Minor, lastKnownVersion.Build + 1);
            SetRegValue(repoRoot, nextVersion.ToString());

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