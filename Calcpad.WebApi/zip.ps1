$sourceRoot = Split-Path $PSScriptRoot -Parent

# all source directories to include in the zip
$sourceDirs = @(
  "Calcpad.Core",
  "Calcpad.OpenXml",
  "Calcpad.WebApi"
)

# create a temporary directory to hold the files to be zipped
$destinationDir = Join-Path $PSScriptRoot ".temp"
if (-not (Test-Path $destinationDir)) {
  New-Item -ItemType Directory -Path $destinationDir | Out-Null
}
$destinationPath = Join-Path $destinationDir "CalcpadWebApi.tar"
# remove existing tar if exists
if (Test-Path $destinationPath) {
  Remove-Item $destinationPath
}

$sourcesPaths = $sourceDirs | ForEach-Object { Join-Path $sourceRoot $_ }

# directories to exclude from the tar (any level)
$excludePatterns = @("obj", "bin", ".vs", ".vscode", ".temp")

# Check if 7z is available
if (-not (Get-Command 7z -ErrorAction SilentlyContinue)) {
  Write-Host "7z not found. Please install 7-Zip." -ForegroundColor Red
  exit
}

# Build the 7z command
$excludeArgs = $excludePatterns | ForEach-Object { "-xr!$_" }
$7zArgs = @("a", "-ttar", "$destinationPath") + $sourcesPaths + $excludeArgs

Write-Host "7z Arguments: $7zArgs" -ForegroundColor Yellow
# Run 7z
& 7z @7zArgs

Write-Host "Created tar file at: $destinationPath" -ForegroundColor Green

# Open the destination directory in File Explorer
Invoke-Item $destinationDir
