#!/bin/bash
# Clean-up
rm -rf ./out/
rm -rf ./staging_folder/

# .NET publish
# self-contained is recommended, so final users won't need to install .NET
dotnet publish "WateryTart.Platform.Linux.csproj"  --verbosity quiet  /p:PublishProfile="Linux-x64"  --output "./out/linux-x64"
dotnet publish "WateryTart.Platform.Linux.csproj"  --verbosity quiet  /p:PublishProfile="LinuxArm"  --output "./out/linux-arm64"

# Staging directory
mkdir staging_folder

# Debian control file
mkdir ./staging_folder/DEBIAN
cp ./control ./staging_folder/DEBIAN

# Starter script
mkdir ./staging_folder/usr
mkdir ./staging_folder/usr/bin
cp ./WateryTart.sh ./staging_folder/usr/bin/WateryTart
chmod +x ./staging_folder/usr/bin/WateryTart # set executable permissions to starter script

# Other files
mkdir ./staging_folder/usr/lib
mkdir ./staging_folder/usr/lib/WateryTart
cp -f -a ./out/linux-x64/. ./staging_folder/usr/lib/WateryTart/ # copies all files from publish dir
chmod -R a+rX ./staging_folder/usr/lib/WateryTart/ # set read permissions to all files
chmod +x ./staging_folder/usr/lib/WateryTart/WateryTartLinux # set executable permissions to main executable

# Desktop shortcut
mkdir ./staging_folder/usr/share
mkdir ./staging_folder/usr/share/applications
cp ./WateryTart.desktop ./staging_folder/usr/share/applications/WateryTart.desktop

# Desktop icon
# A 1024px x 1024px PNG, like VS Code uses for its icon
mkdir ./staging_folder/usr/share/pixmaps
cp ../Assets/logo.png ./staging_folder/usr/share/pixmaps/WateryTart.png

# Hicolor icons
mkdir ./staging_folder/usr/share/icons
mkdir ./staging_folder/usr/share/icons/hicolor
mkdir ./staging_folder/usr/share/icons/hicolor/scalable
mkdir ./staging_folder/usr/share/icons/hicolor/scalable/apps
cp ../Assets/logo.svg ./staging_folder/usr/share/icons/hicolor/scalable/apps/WateryTart.svg

# Make x64 .deb file
dpkg-deb --root-owner-group --build --nocheck ./staging_folder/ ./WateryTart_Linux_amd64.deb

#make arm64 .deb file
rm -rf ./staging_folder/usr/lib/WateryTart
cp -f -a ./out/linux-arm64/. ./staging_folder/usr/lib/WateryTart/ # copies all files from publish dir
chmod -R a+rX ./staging_folder/usr/lib/WateryTart/ # set read permissions to all files
chmod +x ./staging_folder/usr/lib/WateryTart/WateryTartLinux # set executable permissions to main executable
dpkg-deb --root-owner-group --build --nocheck ./staging_folder/ ./WateryTart_Linux_arm64.deb
