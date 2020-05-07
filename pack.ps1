$outputDir = ".\package\"
$build = "Release"
$version = "1.2.0"

.\src\.nuget\NuGet.exe pack ".\src\AdvancedTask.csproj" -IncludeReferencedProjects -properties Configuration=$build -Version $version -OutputDirectory $outputDir
