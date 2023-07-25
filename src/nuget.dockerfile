FROM surveily/developer.dotnet:7.0-sdk
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
RUN dotnet test . -c Release --no-build --no-restore

# Nuget pack
RUN mkdir nuget
RUN dotnet pack --no-restore -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -p:PackageVersion=${version} -o nuget/ Orleans.Streaming.Grains/Orleans.Streaming.Grains.csproj

# Nuget push
WORKDIR /home/vscode/src/nuget
RUN dotnet nuget push -s nuget.org -k ${password} Orleans.Streaming.Grains.${version}.nupkg