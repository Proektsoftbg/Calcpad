<#
Readme for build.ps1

This script is designed to build the Docker image for the Calcpad WebAPI project.

It performs the following steps:
1. Checks if Docker is installed on the system.
2. Sets the working directory to the project root.
3. Builds the Docker image using the Dockerfile located in the Calcpad.WebApi directory.

Usage:
To use this script:
1. Ensure that Docker is installed and running on your machine.
2. Navigate to the project root directory (where this script is located).
3. Execute the script by running: .\build.ps1

The script will output a success message once the image is built.
#>

# check if docker is installed
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
  Write-Error "Docker is not installed. Please install Docker to proceed."
  exit 1
}

$originLocation = Get-Location
$rootDir = Split-Path $PSScriptRoot -Parent
Set-Location $rootDir

# run docker build
docker build -t calcpad/webapi:latest -f ./Calcpad.WebApi/Dockerfile .

# set back to script root
Set-Location $originLocation

Write-Host "Docker image 'calcpad/webapi:latest' built successfully." -ForegroundColor Green