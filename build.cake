#addin "Cake.FileHelpers"

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release");
var artifactsDir    = Directory("./artifacts");
var sourceDir       = Directory("./src");
var solution        = "./src/SqlStreamStore.sln";
var buildNumber     = string.IsNullOrWhiteSpace(EnvironmentVariable("BUILD_NUMBER")) ? "0" : EnvironmentVariable("BUILD_NUMBER");

var projectSqlStreamStore = "./src/SqlStreamStore/SqlStreamStore.csproj";
var projectSqlStreamStoreTests = "./src/SqlStreamStore.Tests/SqlStreamStore.Tests.csproj";
var projectSqlStreamStoreMsSql = "./src/SqlStreamStore.MsSql/SqlStreamStore.MsSql.csproj";
var projectSqlStreamStoreMsSqlTests = "./src/SqlStreamStore.MsSql.Tests/SqlStreamStore.MsSql.Tests.csproj";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(() =>
{
	DotNetCoreRestore(solution);
});

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    if (Context.Environment.Platform.IsUnix())
    {
        Warning("Full solution building is not possible on Unix based systems. Building individual projects instead.");

        DotNetCoreBuild(projectSqlStreamStore, settings);
        DotNetCoreBuild(projectSqlStreamStoreMsSql, settings);
        DotNetCoreBuild(projectSqlStreamStoreTests, settings);
        DotNetCoreBuild(projectSqlStreamStoreMsSqlTests, settings);
    }
    else
    {
        DotNetCoreBuild(solution, settings);
    }
});

Task("RunTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testProjects = new string[] { "SqlStreamStore.Tests", "SqlStreamStore.MsSql.Tests" };

    var processes = testProjects.Select(TestAssembly).ToArray();

    foreach (var process in processes) {
        using (process) {
            process.WaitForExit();
        }
    }
});

Task("DotNetPack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var versionSuffix = "build" + buildNumber.ToString().PadLeft(5, '0');

    var dotNetCorePackSettings   = new DotNetCorePackSettings
    {
        OutputDirectory = artifactsDir,
		NoBuild = true,
		Configuration = configuration,
        VersionSuffix = versionSuffix
    };
    
	DotNetCorePack("./src/SqlStreamStore", dotNetCorePackSettings);
	DotNetCorePack("./src/SqlStreamStore.MsSql", dotNetCorePackSettings);
});

Task("Default")
    .IsDependentOn("RunTests")
    .IsDependentOn("DotNetPack");

RunTarget(target);

IProcess TestAssembly(string name)
    => StartAndReturnProcess(
        "dotnet",
        new ProcessSettings {
            Arguments = $"xunit -quiet -parallel all -configuration {configuration} -nobuild",
            WorkingDirectory = sourceDir + Directory(name)
        });
