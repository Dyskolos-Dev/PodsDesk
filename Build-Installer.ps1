$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'PodsDesk\PodsDesk.csproj'
$setup = Join-Path $PSScriptRoot 'installer\Setup.cs'
$icon = Join-Path $PSScriptRoot 'PodsDesk\Assets\PodsDesk.ico'
$build = Join-Path $PSScriptRoot 'artifacts'
$publish = Join-Path $build 'publish'
New-Item -ItemType Directory -Force -Path $build, $publish | Out-Null
& dotnet publish $project -c Release --self-contained false -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }
Copy-Item (Join-Path $PSScriptRoot 'PodsDesk\README.md') (Join-Path $publish 'README.md') -Force
$payload = Join-Path $build 'payload.zip'
$files = @('PodsDesk.deps.json','PodsDesk.dll','PodsDesk.exe','PodsDesk.runtimeconfig.json','Scanner.exe','README.md') | ForEach-Object { Join-Path $publish $_ }
Compress-Archive -Path $files -DestinationPath $payload -Force
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw '.NET Framework 4.x compiler not found' }
$output = Join-Path $build 'PodsDesk-Setup-1.0.exe'
& $csc /nologo /target:winexe "/out:$output" "/win32icon:$icon" "/resource:$payload,PodsDesk.Payload" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:Microsoft.CSharp.dll $setup
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
Write-Host "Installer: $output"
