$buildDir = ".\build"
$coreDir = $buildDir + "\Core"
$aspnetDir = $buildDir + "\AspNet"
$messengerDir = $buildDir + "\Messenger"
$coreOutputDir = $coreDir + "\Content"
$aspnetOutputDir = $aspnetDir + "\Content"
$messengerOutputDir = $messengerDir + "\Content"
if (test-path $buildDir) { ri -r -fo $buildDir }
mkdir $coreOutputDir | out-null
mkdir $aspnetOutputDir | out-null
mkdir $messengerOutputDir | out-null
copy .\src\TinyIoC\TinyIoC.cs $coreOutputDir
copy .\src\TinyIoC\TinyIoCAspNetExtensions.cs $aspnetOutputDir
copy .\src\TinyIoC\TinyMessenger.cs $messengerOutputDir
.\Tools\nuget\NuGet.exe pack .\TinyIoC.nuspec -b $coreDir -o $coreDir
.\Tools\nuget\NuGet.exe pack .\TinyIoCAspNetExtensions.nuspec -b $aspnetDir -o $aspnetDir
.\Tools\nuget\NuGet.exe pack .\TinyMessenger.nuspec -b $messengerDir -o $messengerDir
