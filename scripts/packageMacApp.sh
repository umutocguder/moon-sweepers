#!/usr/bin/env bash
set -e

APP_NAME="11HopemMoonSweepers"
BUNDLE_DIR="Builds/macOS/${APP_NAME}.app"
CONTENTS_DIR="${BUNDLE_DIR}/Contents"
MACOS_DIR="${CONTENTS_DIR}/MacOS"
RESOURCES_DIR="${CONTENTS_DIR}/Resources"

echo "=== Building Native macOS Standalone Application (${APP_NAME}.app) ==="

mkdir -p "${MACOS_DIR}"
mkdir -p "${RESOURCES_DIR}"

# 1. Compile Native Swift Application
echo "Compiling native launcher with swiftc (Apple Silicon arm64)..."
swiftc launcher/main.swift \
  -O \
  -target arm64-apple-macos13.0 \
  -framework Cocoa \
  -framework WebKit \
  -o "${MACOS_DIR}/${APP_NAME}"

chmod +x "${MACOS_DIR}/${APP_NAME}"

# 2. Generate Info.plist
cat <<EOF > "${CONTENTS_DIR}/Info.plist"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>${APP_NAME}</string>
    <key>CFBundleIdentifier</key>
    <string>com.elevenhopem.moonsweepers</string>
    <key>CFBundleName</key>
    <string>11Hopem Moon Sweepers</string>
    <key>CFBundleDisplayName</key>
    <string>11Hopem Moon Sweepers</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>LSMinimumSystemVersion</key>
    <string>13.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSSupportsAutomaticGraphicsSwitching</key>
    <true/>
</dict>
</plist>
EOF

# 3. Synchronize Web & Game Resources into Bundle Resources
echo "Copying game runtime assets into app bundle..."
cp -R public/* "${RESOURCES_DIR}/"

# 4. Ad-hoc code signing for local macOS gatekeeper pass
echo "Applying local ad-hoc codesign..."
codesign --force --deep --sign - "${BUNDLE_DIR}"

echo "=== macOS Standalone Build Complete! ==="
echo "Location: ${BUNDLE_DIR}"
