#!/bin/bash

set -e

export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get -y install --no-install-recommends \
    ca-certificates \
    bash \
    curl \
    unzip \
    xz-utils \
    git \
    make \
    zip \
    python3 \
    default-jdk \
    clang \
    libfuse2 \
    file \
    flatpak \
    flatpak-builder \
    dpkg \
    rpm \
    dotnet-sdk-8.0 \
    dotnet-sdk-9.0 \
    dotnet-sdk-10.0

flatpak --user remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
flatpak --user install flathub org.freedesktop.Platform//23.08 org.freedesktop.Sdk//23.08 -y

curl -fsSLo appimagetool-x86_64.AppImage "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
chmod +x appimagetool-x86_64.AppImage
./appimagetool-x86_64.AppImage --appimage-extract
mv squashfs-root /opt/appimagetool
ln -s /opt/appimagetool/AppRun /usr/local/bin/appimagetool
rm appimagetool-x86_64.AppImage

dotnet workload install android wasm-tools

curl -L --proto '=https' --tlsv1.3 -sSf https://bun.sh/install | bash

apt-get clean