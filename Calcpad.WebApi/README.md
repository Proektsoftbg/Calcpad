# README - Calcpad.WebApi

## Introduction

Calcpad.WebApi exposes the capabilities of Calcpad.Core as an HTTP API. You can integrate this API into existing projects to provide Calcpad functionality in a web environment.

## Scripts

This project provides cross-platform scripts based on PowerShell to simplify deployment tasks:

- zip.ps1

  Packages the WebApi project and all dependent projects into a zip archive for easy upload to a server for build/deploy.

- docker-build.ps1

  Builds a Docker image. The final image name is `calcpad/webapi:latest`.

Note

On non-Windows platforms, PowerShell may need to be installed manually. See: https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.5

## Deployment (recommended: Docker)

The recommended way to deploy is using Docker. The following steps show how to deploy WebApi inside Docker.

1. Package the project

	From the WebApi root directory run:

	```powershell
	# run packaging script
	./zip.ps1
	```

2. Upload and extract the package on the server

	If you are deploying to Docker on the same machine, you can skip this step.

3. Build the image on the server

	From the `Calcpad.WebApi` directory run:

	```powershell
	./build.ps1
	```

4. Initial configuration

	Copy `docker-compose.yml` and the `config` folder to the installation location. Example:

	```bash
	cp ./data ~/app/calcpad
	```

	Then edit `appsettings.Production.json` according to `Calcpad.WebApi/appsettings.json` to configure production settings. If this is only for testing, these changes are optional.

5. Start

	```bash
	cd ~/app/calcpad
	sudo docker compose up -d
	```

## API

Swagger

When the project runs in debug mode it exposes a Swagger UI. By default the UI is available at: http://localhost:5098/swagger/index.html — use it to explore the API.

The implemented OpenAPI spec is available as `api/swagger.json` or from the running service at http://localhost:5098/swagger/v1/swagger.json. Import this into API tools such as Apifox to inspect the endpoints.

## Integrating with an existing service

1. The server should obtain a token via `/api/v1/user/sign-in` and refresh it periodically.
2. Upload Calcpad files (for example `.txt`, `.cpd`, `.cpdz`) using the endpoints in `CalcpadFileController.cs`.
3. For non-.cpdz files note:
	- Image resources must be uploaded first via `/api/v1/calcpad-file` to receive a resource URL.
	- Included files must be uploaded first. After upload, get the file's `uniqueId` and reference it in the main file like this:

	  ```tex
	  #include ./included_cpd.txt'?uid=uniqueId'
	  ```

	  After a normal path, append `'?uid=xxx'` so the server can recognize the referenced file by its id before uploading the main file.

## Development

Open the solution in Visual Studio to run and debug the project locally.

