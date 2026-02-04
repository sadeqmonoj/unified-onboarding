# --------------------------------------------
# .NET 10 UnifiedOnboarding Makefile
# Place this at your solution root
# --------------------------------------------
# Change these if you bump versions
BFF_IMAGE_NAME = unifiedonboarding.auth.bff
BFF_IMAGE_VERSION = 1.0.0
BUILD_CONFIGURATION = Release
TFM = net10.0
# Default target
.PHONY: build
build:
@dotnet build /ll
# --------------------------------------------------------------------
# Local run without Docker (for dev / debugging)
# --------------------------------------------------------------------
.PHONY: run-bff
run-bff:
@cd ./UnifiedOnboarding.Auth.Bff && dotnet run
# --------------------------------------------------------------------
# Docker: publish & build image for BFF (self-contained, linux-x64)
# --------------------------------------------------------------------
.PHONY: dotnet-bff-publish
dotnet-bff-publish:
@echo "Publishing UnifiedOnboarding.Auth.Bff for linux-x64, self-contained..."
@cd ./UnifiedOnboarding.Auth.Bff && dotnet publish \
--os linux \
--arch x64 \
-c $(BUILD_CONFIGURATION) \
--self-contained
@echo "Building Docker image for UnifiedOnboarding.Auth.Bff..."
@docker build \
-f ./UnifiedOnboarding.Auth.Bff/Dockerfile \
./UnifiedOnboarding.Auth.Bff/bin/$(BUILD_CONFIGURATION)/$(TFM)/linux-x64/publish \
-t $(BFF_IMAGE_NAME):$(BFF_IMAGE_VERSION) \
-t $(BFF_IMAGE_NAME):latest
# --------------------------------------------------------------------
# Docker Compose: infra + app
# (optional, only if you create docker-compose files)
# --------------------------------------------------------------------
.PHONY: docker-up
docker-up:
@docker-compose -f docker-compose.yml up -d
.PHONY: docker-down
docker-down:
@docker-compose -f docker-compose.yml down
# Convenience target to rebuild image and restart containers
.PHONY: docker-rebuild
docker-rebuild: dotnet-bff-publish docker-down docker-up