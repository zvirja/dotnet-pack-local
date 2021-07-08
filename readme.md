[![Build status](https://ci.appveyor.com/api/projects/status/3t8wauku3bew47ty/branch/main?svg=true)](https://ci.appveyor.com/project/Zvirja/dotnet-pack-local/branch/main) [![Nuget version](https://img.shields.io/nuget/v/dotnet-pack-local.svg)](https://www.nuget.org/packages/dotnet-pack-local/)

# dotnet-build-local

It's a dotnet tool to pack project to a local NuGet repo. It takes care of package version to make sure that it steadily increases.
Additionally it packs symbols into the final package for `Debug` builds

# Installation

Tool is distributed as a dotnet tool. To install it you should have .NET Core 3.1 installed (or above). Then run the following command:
```
dotnet tool install -g dotnet-pack-local
```

# Usage

To run the tool just navigate to a directory which is a part of the repo (usual repository root), open command line and run the tool by name:
```
dotnet-pack-local
```

## Options

Run the tool with `--help` or just `-h` flag to see the possible options.
