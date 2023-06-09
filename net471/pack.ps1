$outputDir = ".\package\"
$build = "Release"
$version = "2.2.0"

.\AdvancedTaskManager\.nuget\NuGet.exe pack ".\AdvancedTaskManager\AdvancedTask.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
