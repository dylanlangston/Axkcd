# escape=`

FROM chocolatey/choco:latest-windows AS base
WORKDIR C:\

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command `
    "choco install git -y; `
     choco install make -y; `
     choco install python --version=3.12.7 -y; `
     choco install innosetup -y;"
    #  choco install openjdk17 -y; 
    #  choco install bun -y
     

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command `
    "choco install dotnet-8.0-sdk -y; `
     choco install dotnet-9.0-sdk -y; `
     choco install dotnet-sdk --pre -y"

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command "choco cache delete --all --yes"

RUN dotnet tool install --global KuiperZone.PupNet

# FROM base AS setup
# COPY src/AvaloniaXKCD/AvaloniaXKCD.csproj C:/root/src/AvaloniaXKCD/AvaloniaXKCD.csproj
# COPY src/AvaloniaXKCD.Browser/AvaloniaXKCD.Browser.csproj C:/root/src/AvaloniaXKCD.Browser/AvaloniaXKCD.Browser.csproj
# COPY src/AvaloniaXKCD.Browser/BuildSite.targets C:/root/src/AvaloniaXKCD.Browser/BuildSite.targets
# COPY src/AvaloniaXKCD.Desktop/AvaloniaXKCD.Desktop.csproj C:/root/src/AvaloniaXKCD.Desktop/AvaloniaXKCD.Desktop.csproj
# COPY src/AvaloniaXKCD.Exports/AvaloniaXKCD.Exports.csproj C:/root/src/AvaloniaXKCD.Exports/AvaloniaXKCD.Exports.csproj
# COPY src/AvaloniaXKCD.Generators/AvaloniaXKCD.Generators.csproj C:/root/src/AvaloniaXKCD.Generators/AvaloniaXKCD.Generators.csproj
# COPY src/AvaloniaXKCD.Site/package.json C:/root/src/AvaloniaXKCD.Site/package.json
# COPY src/AvaloniaXKCD.Site/bun.lock C:/root/src/AvaloniaXKCD.Site/bun.lock
# COPY src/AvaloniaXKCD.Site/AvaloniaXKCD.Site.esproj C:/root/src/AvaloniaXKCD.Site/AvaloniaXKCD.Site.esproj
# COPY src/AvaloniaXKCD.Tests/AvaloniaXKCD.Tests.csproj C:/root/src/AvaloniaXKCD.Tests/AvaloniaXKCD.Tests.csproj
# COPY src/XKCDCore/XKCDCore.csproj C:/root/src/XKCDCore/XKCDCore.csproj
# COPY src/Directory.Build.props C:/root/src/Directory.Build.props
# COPY src/Directory.Packages.props C:/root/src/Directory.Packages.props
# COPY src/nuget.config C:/root/src/nuget.config
# COPY src/AvaloniaXKCD.slnx C:/root/src/AvaloniaXKCD.slnx
# COPY makefile C:/root/makefile
# WORKDIR C:/root/
# RUN make setup NO_CERTS=true

FROM base AS build
ARG BUILD_TARGET=windows
ARG BUILD_VERSION
COPY . C:/root/
WORKDIR C:/root/
RUN make publish-${BUILD_TARGET} VERSION=${BUILD_VERSION}

FROM scratch AS publish
COPY --from=build /root/out /