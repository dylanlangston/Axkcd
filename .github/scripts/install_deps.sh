#!/bin/bash

set -e

export DEBIAN_FRONTEND=noninteractive
apt-get update

PACKAGES="
    ca-certificates
    bash
    curl
    unzip
    xz-utils
    git
    make
    zip
    python3
    default-jdk
    clang
    libfuse2
    file
    dpkg
    rpm
"

echo "DEBUG: Received INSTALL_BUILD_TOOLS as '${INSTALL_BUILD_TOOLS}'"

if [ "${INSTALL_BUILD_TOOLS,,}" = "true" ]; then
    PACKAGES="$PACKAGES flatpak flatpak-builder"
fi

apt-get -y install --no-install-recommends $PACKAGES

if [ "${INSTALL_BUILD_TOOLS,,}" = "true" ]; then
    flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
    flatpak install flathub org.freedesktop.Platform//23.08 org.freedesktop.Sdk//23.08 -y

    curl -fsSLo appimagetool-x86_64.AppImage "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
    chmod +x appimagetool-x86_64.AppImage
    ./appimagetool-x86_64.AppImage --appimage-extract
    mv squashfs-root /opt/appimagetool
    ln -s /opt/appimagetool/AppRun /usr/local/bin/appimagetool-x86_64.AppImage
    rm appimagetool-x86_64.AppImage
fi

dotnet workload install android wasm-tools

apt-get clean