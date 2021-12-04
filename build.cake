#load "nuget:?package=PleOps.Cake&version=0.6.0"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.WarningsAsErrors = false;
    info.CoverageTarget = 75;

    info.AddLibraryProjects("Pleosoft.XdeltaSharp");
    info.AddApplicationProjects("Pleosoft.XdeltaSharp.Cli");
    info.AddTestProjects("Pleosoft.XdeltaSharp.UnitTests");

    info.PreviewNuGetFeed = "https://pkgs.dev.azure.com/pleonex/Pleosoft/_packaging/Pleosoft-Preview/nuget/v3/index.json";
});

Task("Clean")
    .Does<BuildInfo>(info =>
{
    var settings = new DotNetCoreCleanSettings {
        Configuration = info.Configuration,
    };
    DotNetCoreClean(info.SolutionFile, settings);
});

Task("CleanBuild")
    .IsDependentOn("Define-Project")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
