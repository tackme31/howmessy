param (
    [Parameter(Mandatory=$true)]
    [string]
    $Version
)

$project = './Howmessy.Cli.csproj'
$runtimes = @('win-x64', 'osx-x64', 'linux-x64')
$runtimes | ForEach-Object {
  $output = "./publish/$_"
  dotnet publish $project -r $_ -c Release -p:PublishSingleFile=true -o $output

  Compress-Archive -Path $output -DestinationPath "./publish/howmessy-$Version-$_.zip"
}