$buildDir = ".\build"
$coreDir = $buildDir + "\Core"
$aspnetDir = $buildDir + "\AspNet"
$messengerDir = $buildDir + "\Messenger"
$winrtDir = $buildDir + "\WinRT"
$coreOutputDir = $coreDir + "\Content"
$aspnetOutputDir = $aspnetDir + "\Content"
$winrtOutputDir = $winrtDir + "\Content"
$messengerOutputDir = $messengerDir + "\Content"
if (test-path $buildDir) { ri -r -fo $buildDir }
mkdir $coreOutputDir | out-null
mkdir $aspnetOutputDir | out-null
mkdir $winrtOutputDir | out-null
mkdir $messengerOutputDir | out-null
copy .\src\TinyIoC\TinyIoC.cs $coreOutputDir
copy .\src\TinyIoC\TinyIoCAspNetExtensions.cs $aspnetOutputDir
copy .\src\TinyIoC.MetroStyle\TypeExtender.cs $winrtOutputDir
copy .\src\TinyIoC\TinyMessenger.cs $messengerOutputDir
.\Tools\nuget\NuGet.exe pack .\TinyIoC.nuspec -basepath $coreDir -o $coreDir
.\Tools\nuget\NuGet.exe pack .\TinyIoCAspNetExtensions.nuspec -basepath $aspnetDir -o $aspnetDir
.\Tools\nuget\NuGet.exe pack .\TinyIoCWinRT.nuspec -basepath $winrtDir -o $winrtDir
.\Tools\nuget\NuGet.exe pack .\TinyMessenger.nuspec -basepath $messengerDir -o $messengerDir
