$ErrorActionPreference = "Stop"

choco install git make python --version=3.12.7 innosetup -y

dotnet tool install --global KuiperZone.PupNet

choco cache delete --all --yes