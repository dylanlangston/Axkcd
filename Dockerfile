FROM debian:stable-slim AS base
USER root

WORKDIR /root/

# Install General Dependencies
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
     && apt-get -y install --no-install-recommends ca-certificates bash curl unzip xz-utils git make zip python3 default-jdk clang \
     flatpak flatpak-builder dpkg rpm

# Flatpak platform SDK and runtime
RUN flatpak --user remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo \
     && flatpak --user install flathub org.freedesktop.Platform//23.08 org.freedesktop.Sdk//23.08 -y

# AppImageTool
RUN curl -fsSLo /usr/local/bin/appimagetool "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage" \
    && chmod +x /usr/local/bin/appimagetool

# Add Microsoft package signing key & insiders-slow list
RUN curl -fsSLo packages-microsoft-prod.deb https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && curl -fsSL https://packages.microsoft.com/config/debian/12/insiders-slow.list | tee /etc/apt/sources.list.d/microsoft-insiders-slow.list

# Install DOTNET 8 & 9
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
     && apt-get -y install --no-install-recommends dotnet-sdk-8.0 dotnet-sdk-9.0

# Install .NET 10 RC 2
RUN curl -fsSLo dotnet.tar.gz https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.100-rc.2.25502.107/dotnet-sdk-10.0.100-rc.2.25502.107-linux-x64.tar.gz \
     && mkdir /opt/dotnet10-rc2 \
     && tar -zxf dotnet.tar.gz -C /opt/dotnet10-rc2 \
     && mv /usr/bin/dotnet /usr/bin/dotnet9 \
     && ln -s /opt/dotnet10-rc2/dotnet /usr/bin/dotnet

# Setup dotnet workloads
RUN dotnet workload install android wasm-tools

# Clean Image
RUN apt-get clean && rm -rf /var/cache/apt/* && rm -rf /var/lib/apt/lists/* && rm -rf /tmp/*

SHELL ["/bin/bash", "-lc"]

# Install Bun
RUN curl -L --proto '=https' --tlsv1.3 -sSf https://bun.sh/install | bash

FROM base AS setup
COPY . /root/
RUN make setup

FROM base AS test
COPY . /root/
RUN make test

FROM setup AS build-linux
ARG TARGET=linux
ARG VERSION
RUN echo vars target=${TARGET} version=${VERSION}
RUN make publish-${TARGET} VERSION=${VERSION}

FROM scratch AS publish-linux
COPY --from=build-linux /root/out /