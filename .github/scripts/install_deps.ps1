$ErrorActionPreference = "Stop"

choco install git make python --version=3.12.7 innosetup -y

choco install dotnet-8.0-sdk dotnet-9.0-sdk dotnet-sdk --pre -y

dotnet tool install --global KuiperZone.PupNet

choco cache delete --all --yes