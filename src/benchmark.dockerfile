FROM mcr.microsoft.com/vscode/devcontainers/dotnet:dev-7.0
ARG version
ARG password
ARG TARGETARCH
ARG BUILDPLATFORM

# Prepare folders
WORKDIR /home/vscode/src
ADD README.md README.md
ADD src/ .

# Build and test
RUN dotnet build . -c Release
RUN dotnet Orleans.Streaming.Grains.Performance/bin/Release/net7.0/Orleans.Streaming.Grains.Performance.dll -m -t --filter "*Test*"