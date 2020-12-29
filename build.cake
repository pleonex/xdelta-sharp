#load "nuget:?package=PleOps.Cake&version=0.4.0"

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

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
