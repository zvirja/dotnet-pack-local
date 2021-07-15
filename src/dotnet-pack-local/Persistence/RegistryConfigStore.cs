using System;
using Microsoft.Win32;

namespace DotnetPackLocal.Persistence
{
    internal class RegistryConfigStore : IConfigStore
    {
        private const string RegistryStoreKey = @"SOFTWARE\Zvirja\DotnetPackLocal";

        public string? GetLocalNuGetRepositoryPath() => GetRegValue("LocalNuGetStore");

        public void SetLocalNuGetRepositoryPath(string? value) => SetRegValue("LocalNuGetStore", value);

        public Version? GetLastVersionForProject(string repoPath)
        {
            string? lastKnownVersionStr = GetRegValue(repoPath);
            return lastKnownVersionStr != null
                ? Version.Parse(lastKnownVersionStr)
                : null;
        }

        public void SetLastVersionForProject(Version version, string repoPath)
        {
            SetRegValue(repoPath, version.ToString());
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