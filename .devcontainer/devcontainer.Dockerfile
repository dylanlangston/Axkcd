FROM mcr.microsoft.com/devcontainers/base:debian as base

# Install General Dependencies
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
     && apt-get -y install --no-install-recommends ca-certificates bash curl make git python3 default-jdk sdkmanager xdg-utils git-lfs \
     libx11-6 libx11-dev libxext6 libxext-dev libxrender1 libxrender-dev libsm6 libice6 \
     flatpak flatpak-builder dpkg rpm

# Flatpak platform SDK and runtime
RUN flatpak --user remote-add --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo \
     && flatpak --user install flathub org.freedesktop.Platform//23.08 org.freedesktop.Sdk//23.08 -y

# AppImageTool
RUN curl -fsSLo appimagetool-x86_64.AppImage "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage" \
    && chmod +x appimagetool-x86_64.AppImage \
    && ./appimagetool-x86_64.AppImage --appimage-extract \
    && mv squashfs-root /opt/appimagetool \
    && ln -s /opt/appimagetool/AppRun /usr/local/bin/appimagetool-x86_64.AppImage \
    && rm appimagetool-x86_64.AppImage
    
# Add Microsoft package signing key & insiders-slow list
RUN curl -fsSLo packages-microsoft-prod.deb https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && curl -fsSL https://packages.microsoft.com/config/debian/12/insiders-slow.list | tee /etc/apt/sources.list.d/microsoft-insiders-slow.list

# Install DOTNET 8, 9, and 10
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
     && apt-get -y install --no-install-recommends dotnet-sdk-8.0 dotnet-sdk-9.0 dotnet-sdk-10.0

# Setup dotnet workloads
RUN dotnet workload install android wasm-tools

# Accept sdkmanager licenes
RUN yes | sdkmanager --licenses

# Install Android SDK
RUN sdkmanager "cmdline-tools;latest" "platform-tools" "platforms;android-34" "build-tools;34.0.0"

# Install Docker
RUN install -m 0755 -d /etc/apt/keyrings \
     && curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc \
     && chmod a+r /etc/apt/keyrings/docker.asc \
     && echo \
     "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian \
     $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
     tee /etc/apt/sources.list.d/docker.list > /dev/null \
     && apt-get update && apt-get -y install --no-install-recommends docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

RUN usermod -aG docker vscode

# Clean Image
RUN apt-get clean && rm -rf /var/cache/apt/* && rm -rf /var/lib/apt/lists/* && rm -rf /tmp/*

# Important we change to the vscode user that the devcontainer runs under
USER vscode
WORKDIR /home/vscode

# Install Bun
RUN curl -L --proto '=https' --tlsv1.3 -sSf https://bun.sh/install | bash

# Install git lfs
RUN git lfs install