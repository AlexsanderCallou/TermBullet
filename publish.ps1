param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$ProjectPath = "src/TermBullet/TermBullet.csproj"

$PublishRoot = "publish"
$WinOut = "$PublishRoot/win-x64"
$LinuxOut = "$PublishRoot/linux-x64"
$DistOut = "$PublishRoot/dist"

$WindowsZip = "$DistOut/termbullet_$Version`_windows_x64.zip"
$LinuxTar = "$DistOut/termbullet_$Version`_linux_x64.tar.gz"
$Checksums = "$DistOut/termbullet_$Version`_checksums.txt"

Write-Host "Limpando artefatos antigos..."

Remove-Item -Recurse -Force $WinOut -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $LinuxOut -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $DistOut -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force $DistOut | Out-Null

Write-Host "Restaurando pacotes..."
dotnet restore

Write-Host "Compilando..."
dotnet build -c Release --no-restore

Write-Host "Executando testes..."
dotnet test -c Release --no-build

Write-Host "Publicando Windows x64..."
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$Version `
    -o $WinOut

Write-Host "Publicando Linux x64..."
dotnet publish $ProjectPath `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$Version `
    -o $LinuxOut

Write-Host "Gerando ZIP Windows..."
Compress-Archive `
    -Path "$WinOut/*" `
    -DestinationPath $WindowsZip `
    -Force

Write-Host "Gerando TAR.GZ Linux..."
tar -czf $LinuxTar -C $LinuxOut .

Write-Host "Gerando checksums..."

$WinHash = Get-FileHash $WindowsZip -Algorithm SHA256
$LinuxHash = Get-FileHash $LinuxTar -Algorithm SHA256

@(
    "$($WinHash.Hash.ToLower())  $(Split-Path $WindowsZip -Leaf)"
    "$($LinuxHash.Hash.ToLower())  $(Split-Path $LinuxTar -Leaf)"
) | Set-Content $Checksums

Write-Host ""
Write-Host "Arquivos gerados:"
Get-ChildItem $DistOut | Select-Object Name, Length

Write-Host ""
Write-Host "Publish finalizado com sucesso."
