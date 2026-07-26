#!/usr/bin/env bash
# Build script for macOS .app bundle and .dmg package
set -e

VERSION="${1:-1.0.0}"
RID="${2:-osx-arm64}" # osx-arm64 or osx-x64
CONFIGURATION="${3:-Release}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${REPO_ROOT}"

echo "========================================================="
echo " Building SafeScribe AI macOS App & DMG (v${VERSION}, ${RID})"
echo "========================================================="

PUBLISH_DIR="${REPO_ROOT}/publish/${RID}"
DIST_DIR="${REPO_ROOT}/dist"
APP_DIR="${REPO_ROOT}/publish/SafeScribeAI.app"

rm -rf "${PUBLISH_DIR}" "${APP_DIR}"
mkdir -p "${DIST_DIR}"

echo "[1/4] Publishing .NET 10 Avalonia App (${RID})..."
dotnet publish "${REPO_ROOT}/SafeScribe.UI.Avalonia/SafeScribe.UI.Avalonia.csproj" \
    -c "${CONFIGURATION}" \
    -r "${RID}" \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:Version="${VERSION}" \
    -o "${PUBLISH_DIR}"

echo "[2/4] Constructing macOS Application Bundle (.app)..."
mkdir -p "${APP_DIR}/Contents/MacOS"
mkdir -p "${APP_DIR}/Contents/Resources"

cp -R "${PUBLISH_DIR}/"* "${APP_DIR}/Contents/MacOS/"
chmod +x "${APP_DIR}/Contents/MacOS/SafeScribe.UI.Avalonia"

echo "APPL????" > "${APP_DIR}/Contents/PkgInfo"

cat <<EOF > "${APP_DIR}/Contents/Info.plist"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>SafeScribe.UI.Avalonia</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundleIdentifier</key>
    <string>com.safescribe.ai</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>SafeScribe AI</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSMicrophoneUsageDescription</key>
    <string>SafeScribe AI requires access to microphone for live meeting audio transcription.</string>
</dict>
</plist>
EOF

echo "[3/4] Creating Tarball Archive..."
TAR_PATH="${DIST_DIR}/SafeScribeAI-macOS-${RID}-v${VERSION}.tar.gz"
tar -czf "${TAR_PATH}" -C "${REPO_ROOT}/publish" "SafeScribeAI.app"
echo "✓ macOS tarball archive created at: ${TAR_PATH}"

echo "[4/4] Creating DMG Disk Image..."
DMG_PATH="${DIST_DIR}/SafeScribeAI-v${VERSION}-${RID}.dmg"
rm -f "${DMG_PATH}"

if command -v hdiutil &> /dev/null; then
    hdiutil create -volname "SafeScribe AI" -srcfolder "${APP_DIR}" -ov -format UDZO "${DMG_PATH}"
    echo "✓ macOS DMG created successfully at: ${DMG_PATH}"
elif command -v create-dmg &> /dev/null; then
    create-dmg "${APP_DIR}" "${DIST_DIR}"
else
    echo "hdiutil not available (not running on macOS). Skipping DMG creation."
fi

echo "========================================================="
echo " macOS Build Summary for ${RID}:"
ls -lh "${DIST_DIR}"
echo "========================================================="
