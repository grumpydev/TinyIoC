$buildDir = ".\build"
$outputDir = $buildDir + "\Content"
if (test-path $buildDir) { ri -r -fo $buildDir }
mkdir $outputDir | out-null
copy .\src\TinyIoC\TinyIoC.cs $outputDir
.\Tools\nuget\NuGet.exe pack .\TinyIoC.nuspec -b .\build -o .\build