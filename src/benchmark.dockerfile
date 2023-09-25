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
WORKDIR /home/vscode/src/Orleans.Streaming.Grains.Performance
RUN dotnet run -c Release