# escape=`

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS base-windows
WORKDIR C:\

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command `
    "Set-ExecutionPolicy Bypass -Scope Process -Force; `
     [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12; `
     iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'));"
RUN setx /M PATH "C:\ProgramData\chocolatey\bin;%PATH%"
RUN choco feature enable -n=allowGlobalConfirmation

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command `
    "choco install git -y; `
     choco install make -y; `
     choco install python --version=3.12.7 -y; `
     choco install openjdk17 -y; `
     choco install innosetup -y; `
     choco install nodejs.install -y; `
     irm https://bun.sh/install.ps1 | iex"

RUN setx /M PATH "C:\ProgramData\chocolatey\lib\make\tools\install\bin;%PATH%"

RUN powershell -NoProfile -ExecutionPolicy Bypass -Command `
    "choco install dotnet-8.0-sdk -y; `
     choco install dotnet-9.0-sdk -y; `
     choco install dotnet-sdk --pre -y"

RUN choco cache delete --all --yes

FROM base-windows AS setup-windows
COPY . C:/root/
WORKDIR C:/root/
RUN make setup NO_CERTS=true

FROM setup-windows AS build-windows
ARG TARGET=windows
ARG VERSION
COPY . C:/root/
WORKDIR C:/root/
RUN make publish-${TARGET} VERSION=${VERSION}

FROM scratch AS publish-windows
COPY --from=build-windows /root/out /