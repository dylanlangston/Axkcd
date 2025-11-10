# --- Configuration ---
VERSION = 
OUTPUT_DIR = ./out
HOST_URL = https://example.com/

# --- Cross-Platform Setup ---

# Default to a Unix-like environment
SHELL = /bin/bash
CP_R = cp -r
ECHO = echo
HELP_COMMAND = grep -E '^[a-zA-Z_%:-]+:.*?\#\# .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?\#\# "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'
define CLEAN_DIR
	@rm -rf "$(1)"
	@mkdir -p "$(1)"
endef
define VALIDATE_TARGET
	@if ! echo "$(1)" | grep -wq "$(2)"; then \
		$(ECHO) "Error: Invalid option '$(2)'."; \
		$(ECHO) "Valid options are: $(1)"; \
		exit 1; \
	fi
endef
define VALIDATE_VAR
	@if [ -z "$($(1))" ]; then \
		$(ECHO) "Error: $(1) is not set. Please provide a value."; \
		exit 1; \
	fi
endef

# If on Windows, override the variables.
ifeq ($(OS),Windows_NT)
SHELL = powershell.exe
.SHELLFLAGS = -NoProfile -ExecutionPolicy Bypass -Command
CP_R = Copy-Item -Recurse -Force
ECHO = $(SHELL) -Command "Write-Host"
define HELP_COMMAND
Get-Content "$(abspath $(firstword $(MAKEFILE_LIST)))" | Select-String '^[a-zA-Z_%:-]+:.*?## ' | ForEach-Object { $$parts = $$_.Line -split ':.*?## '; Write-Host ('{0,-30}' -f $$parts[0]) $$parts[1] -ForegroundColor Cyan }
endef
define CLEAN_DIR
	@if (Test-Path "$(subst /,\,$(1))") { Remove-Item -Recurse -Force "$(subst /,\,$(1))" }
	@if (-not (Test-Path -LiteralPath "$(subst /,\,$(1))")) { New-Item -ItemType Directory -Force -Path "$(subst /,\,$(1))" | Out-Null }
endef
define VALIDATE_TARGET
   	@if (-not ("$(1)" -split ' ' -contains "$(2)")) { $(ECHO) "Error: Invalid option '$(2)'." -ForegroundColor Red; $(ECHO) "Valid options are: $(1)" -ForegroundColor Yellow; exit 1; }
endef
define VALIDATE_VAR
	@if (-not (Test-Path env:$(1))) { $(ECHO) "Error: $(1) is not set. Please provide a value." -ForegroundColor Red; exit 1; }
endef
endif

# --- Makefile Targets ---

help: ## Display the help menu.
	@$(ECHO) AvaloniaXKCD makefile targets:
	@$(HELP_COMMAND)

DEVELOP_KINDS = desktop browser
develop-browser: ## Run the browser project in development mode.
develop-desktop: ## Run the desktop project in development mode.
develop-%: # Run the project for % in development mode.
	$(call VALIDATE_TARGET,$(DEVELOP_KINDS),$*)
	@cd src/AvaloniaXKCD.$(if $(filter $*,desktop),Desktop,Browser); dotnet run

PUBLISH_KINDS = windows linux
publish-windows: ## Publish a self-contained version of the application for 64-bit Windows.
publish-linux: ## Publish a self-contained version of the application for 64-bit Linux.
publish-%: # Publish a self-contained version of the application for %.
	$(call VALIDATE_TARGET,$(PUBLISH_KINDS),$*)
	$(call VALIDATE_VAR,VERSION)
	@$(ECHO) "--- Publishing for $* ---"
	@$(call CLEAN_DIR,$(OUTPUT_DIR))
	@cd src/AvaloniaXKCD.Desktop; dotnet publish --self-contained --runtime $(if $(filter $*,windows),win,linux)-x64 -p:Version=$(VERSION) -p:DebugSymbols=false -p:DebugType=None -o "$(abspath $(OUTPUT_DIR))"

publish-windows-zip: ## Publish Windows package of type zip.
	$(call VALIDATE_VAR,VERSION)
	@$(ECHO) "--- Publishing Windows package: zip ---"
	@$(call CLEAN_DIR,$(OUTPUT_DIR))
	@cd src; pupnet --runtime win-x64 --kind zip -y -v $(VERSION)

LINUX_PUBLISH_KINDS = zip appimage deb rpm flatpak
publish-linux-zip: ## Publish Linux package of type zip.
publish-linux-appimage: ## Publish Linux package of type appimage.
publish-linux-deb: ## Publish Linux package of type deb.
publish-linux-rpm: ## Publish Linux package of type rpm.
publish-linux-flatpak: ## Publish Linux package of type flatpak.
publish-linux-%: # Publish Linux package of type %.
	$(call VALIDATE_TARGET,$(LINUX_PUBLISH_KINDS),$*)
	$(call VALIDATE_VAR,VERSION)
	@$(ECHO) "--- Publishing Linux package: $* ---"
	@$(call CLEAN_DIR,$(OUTPUT_DIR))
	@cd src; pupnet --runtime linux-x64 --kind $* -y -v $(VERSION)

publish-browser: ## Publish the browser version of the application.
	$(call VALIDATE_VAR,VERSION)
	@$(ECHO) "--- Publishing for Browser (WASM) ---"
	@$(call CLEAN_DIR,$(OUTPUT_DIR))
	@cd src/AvaloniaXKCD.Browser; dotnet publish -p:Version=$(VERSION) -p:UseNativeAotLllvm=true -p:NativeDebugSymbols=false -p:StackTraceSupport=false -r browser-wasm -o "$(abspath $(OUTPUT_DIR))"

check-docker: ## Ensure Docker is running before using Docker-based targets
ifeq ($(OS),Windows_NT)
	@$(ECHO) "--- Checking Docker on Windows ---"
	@if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { $(ECHO) "Docker is not installed or not on PATH." -ForegroundColor Red; exit 1 }
	@docker ps >$$null 2>&1; if ($$?) { $(ECHO) "Docker is running." -ForegroundColor Green; } else { $(ECHO) "Docker does not appear to be running correctly. Please check Docker Desktop and try again." -ForegroundColor Red; exit 1; }
else
	@$(ECHO) "--- Checking Docker ---"
	@if ! docker info >/dev/null 2>&1; then \
		$(ECHO) "Docker is not running. Attempting to start it..."; \
		sudo dockerd >/tmp/dockerd.log 2>&1 & \
		sleep 5; \
		if ! docker info >/dev/null 2>&1; then \
			$(ECHO) "Failed to start Docker. Check /tmp/dockerd.log for details."; \
			exit 1; \
		fi; \
	fi
	@$(ECHO) "Docker is running."
endif

DOCKER_PUBLISH_WINDOWS_TARGETS = windows windows-zip
DOCKER_PUBLISH_OTHER_TARGETS   = linux linux-zip linux-appimage linux-deb linux-rpm linux-flatpak browser
DOCKER_PUBLISH_ALL_TARGETS   = $(DOCKER_PUBLISH_WINDOWS_TARGETS) $(DOCKER_PUBLISH_OTHER_TARGETS)
DOCKER_OTHER_PUBLISH_CMD = @docker build --rm --network=host --progress=plain -t avaloniaxkcd-builder . --target publish-linux --build-arg TARGET=$* --build-arg VERSION=$(VERSION) --output type=local,dest=$(OUTPUT_DIR)
define DOCKER_WINDOWS_PUBLISH_CMDS
	@docker build --rm -t avaloniaxkcd-builder . --target publish-windows --build-arg TARGET=$* --build-arg VERSION=$(VERSION) --file windows.Dockerfile
	@docker create --name avaloniaxkcd-tmp-container avaloniaxkcd-builder
	@docker cp avaloniaxkcd-tmp-container:C:/app/publish/. $(OUTPUT_DIR)
	@docker rm avaloniaxkcd-tmp-container
endef
docker-publish-windows: ## Publish any platform of type windows via Docker.
docker-publish-windows-zip: ## Publish any platform of type windows-zip via Docker.
docker-publish-linux: ## Publish any platform of type linux via Docker.
docker-publish-linux-zip: ## Publish any platform of type linux-zip via Docker.
docker-publish-linux-appimage: ## Publish any platform of type linux-appimage via Docker.
docker-publish-linux-deb: ## Publish any platform of type linux-deb via Docker.
docker-publish-linux-rpm: ## Publish any platform of type linux-rpm via Docker.
docker-publish-linux-flatpak: ## Publish any platform of type linux-flatpak via Docker.
docker-publish-browser: ## Publish any platform of type browser via Docker.
docker-publish-%: check-docker # Publish any platform of type % via Docker.
	$(call VALIDATE_TARGET,$(DOCKER_PUBLISH_ALL_TARGETS),$*)
	$(call VALIDATE_VAR,VERSION)
	@$(ECHO) "--- Publishing via Docker: $* ---"
	@$(call CLEAN_DIR,$(OUTPUT_DIR))
	$(if $(filter $*,$(DOCKER_PUBLISH_WINDOWS_TARGETS)), \
		$(DOCKER_WINDOWS_PUBLISH_CMDS), \
		$(DOCKER_OTHER_PUBLISH_CMD) \
	)

test: ## Run all tests in the solution.
	@cd src; dotnet test

docker-test: ## Run all test in the solution via Docker.
	@docker build --rm --network=host --progress=plain -t avaloniaxkcd-builder . --target test

restore: ## Restore the nuget packages for the solution.
	@cd src; dotnet restore

sync: ## Run the XKCD sync tool to mirror comics.
	@dotnet run ./src/XKCDSyncTool.cs -- -o ./mirror -b $(HOST_URL)

verify-review: ## Review and approve any changes detected by snapshot tests.
	@cd src; dotnet verify review

setup: ## Setup local environment (install workloads, templates, and tools).
ifneq ($(NO_CERTS),)
	@echo "Skipping interactive dev-certs trust command."
else
	-@dotnet dev-certs https --trust
endif
	@dotnet new install Avalonia.Templates
	@dotnet new install TUnit.Templates
	@dotnet new install Microsoft.NET.Runtime.WebAssembly.Templates
	@dotnet new install Microsoft.VisualStudio.JavaScript.SDK
	@dotnet tool install --global verify.tool
	@dotnet tool install --global dotnet-outdated-tool
	@dotnet tool install --global KuiperZone.PupNet
	@cd src; dotnet workload restore

setup-playwright: ## Setup playwright browsers for testing.
	@cd src; dotnet build; dnx -y Microsoft.Playwright.CLI -p ./AvaloniaXKCD.Tests install --with-deps

format: ## Format source code.
	@cd src; dotnet format

clean: ## Clean local environment (bin/obj folders).
	@cd src; dotnet clean

update: ## Update nuget packages to the latest pre-release.
	@cd src; dotnet9 outdated --upgrade --pre-release Always