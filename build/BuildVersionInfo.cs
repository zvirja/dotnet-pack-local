public record BuildVersionInfo
{
    public string AssemblyVersion { get; init; }
    public string FileVersion { get; init; }
    public string InfoVersion { get; init; }
    public string NuGetVersion { get; init; }

    public override string ToString() => $"Assembly: {AssemblyVersion}, Info: {InfoVersion}, File: {FileVersion} NuGet: {NuGetVersion}";
}