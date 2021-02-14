param (
	[switch]$Debug,
	[switch]$NoBuild,
	[switch]$NoReport,
	[switch]$AppVeyor
)

If (-not $NoBuild) {
	# Run a build to ensure everything is up-to-date
	If ($Debug) {
		.\build.ps1 -Debug -Incremental
	} Else {
		.\build.ps1 -Incremental
	}

	If (-not $?) {
		$host.UI.WriteErrorLine('Build failed; coverage analysis aborted.')
		Exit $LASTEXITCODE
	}
}

If ($Debug) {
	$Configuration = 'Debug'
} Else {
	$Configuration = 'Release'
}

$packageConfig = [xml](Get-Content ..\.nuget\packages.config)
$opencover_version = $packageConfig.SelectSingleNode('/packages/package[@id="OpenCover"]').version
$reportgenerator_version = $packageConfig.SelectSingleNode('/packages/package[@id="ReportGenerator"]').version
$xunitrunner_version = $packageConfig.SelectSingleNode('/packages/package[@id="xunit.runner.console"]').version

$packages_folder = '..\packages'
$opencover_console = "$packages_folder\OpenCover.$opencover_version\tools\OpenCover.Console.exe"
$xunit_runner_console_net452 = "$packages_folder\xunit.runner.console.$xunitrunner_version\tools\net452\xunit.console.x86.exe"
$xunit_runner_console_net46 = "$packages_folder\xunit.runner.console.$xunitrunner_version\tools\net46\xunit.console.x86.exe"
$report_generator = "$packages_folder\ReportGenerator.$reportgenerator_version\tools\ReportGenerator.exe"
$report_folder = '.\OpenCover.Reports'
$target_dll = "..\DocumentationAnalyzers\DocumentationAnalyzers.Test\bin\$Configuration\net452\DocumentationAnalyzers.Test.dll"
$target_dll_csharp7 = "..\DocumentationAnalyzers\DocumentationAnalyzers.Test.CSharp7\bin\$Configuration\net46\DocumentationAnalyzers.Test.CSharp7.dll"

If (Test-Path $report_folder) {
	Remove-Item -Recurse -Force $report_folder
}

mkdir $report_folder | Out-Null

$register_mode = 'user'
If ($AppVeyor) {
	$AppVeyorArg = '-appveyor'
	$register_mode = 'Path32'
}

&$opencover_console `
	-register:$register_mode `
	-returntargetcode `
	-hideskipped:All `
	-filter:"+[Documentation*]*" `
	-excludebyattribute:*.ExcludeFromCodeCoverage* `
	-excludebyfile:*\*Designer.cs `
	-output:"$report_folder\OpenCover.DocumentationAnalyzers.xml" `
	-target:"$xunit_runner_console_net452" `
	-targetargs:"$target_dll -noshadow $AppVeyorArg"

If ($AppVeyor -and -not $?) {
	$host.UI.WriteErrorLine('Build failed; coverage analysis aborted.')
	Exit $LASTEXITCODE
}

&$opencover_console `
	-register:$register_mode `
	-returntargetcode `
	-hideskipped:All `
	-filter:"+[Documentation*]*" `
	-excludebyattribute:*.ExcludeFromCodeCoverage* `
	-excludebyfile:*\*Designer.cs `
	-output:"$report_folder\OpenCover.DocumentationAnalyzers.xml" `
	-mergebyhash -mergeoutput `
	-target:"$xunit_runner_console_net46" `
	-targetargs:"$target_dll_csharp7 -noshadow $AppVeyorArg"

If ($AppVeyor -and -not $?) {
	$host.UI.WriteErrorLine('Build failed; coverage analysis aborted.')
	Exit $LASTEXITCODE
}

If (-not $NoReport) {
	&$report_generator -targetdir:$report_folder -reports:$report_folder\OpenCover.*.xml
	$host.UI.WriteLine("Open $report_folder\index.htm to see code coverage results.")
}
