#!/usr/bin/env bash
# Build script for Linux .deb package and portable .tar.gz archive
set -e

VERSION="${1:-1.0.0}"
CONFIGURATION="${2:-Release}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${REPO_ROOT}"

echo "========================================================="
echo " Building SafeScribe AI Linux Package (v${VERSION})"
echo "========================================================="

PUBLISH_DIR="${REPO_ROOT}/publish/linux-x64"
DIST_DIR="${REPO_ROOT}/dist"
DEB_DIR="${REPO_ROOT}/publish/deb_package"

rm -rf "${PUBLISH_DIR}" "${DEB_DIR}"
mkdir -p "${DIST_DIR}"

echo "[1/4] Publishing .NET 10 Avalonia App (linux-x64)..."
dotnet publish "${REPO_ROOT}/SafeScribe.UI.Avalonia/SafeScribe.UI.Avalonia.csproj" \
    -c "${CONFIGURATION}" \
    -r linux-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:Version="${VERSION}" \
    -o "${PUBLISH_DIR}"

echo "[2/4] Creating Portable Tarball Archive..."
TAR_PATH="${DIST_DIR}/SafeScribeAI-Linux-v${VERSION}-x64.tar.gz"
tar -czf "${TAR_PATH}" -C "${PUBLISH_DIR}" .
echo "✓ Linux tarball created at: ${TAR_PATH}"

echo "[3/4] Structuring Debian Package (.deb)..."
DEB_APP_DIR="${DEB_DIR}/usr/lib/safescribe-ai"
DEB_BIN_DIR="${DEB_DIR}/usr/bin"
DEB_DESKTOP_DIR="${DEB_DIR}/usr/share/applications"
DEB_DOC_DIR="${DEB_DIR}/usr/share/doc/safescribe-ai"
DEB_CONTROL_DIR="${DEB_DIR}/DEBIAN"

mkdir -p "${DEB_APP_DIR}" "${DEB_BIN_DIR}" "${DEB_DESKTOP_DIR}" "${DEB_DOC_DIR}" "${DEB_CONTROL_DIR}"

cp -R "${PUBLISH_DIR}/"* "${DEB_APP_DIR}/"
chmod +x "${DEB_APP_DIR}/SafeScribe.UI.Avalonia"

ln -sf /usr/lib/safescribe-ai/SafeScribe.UI.Avalonia "${DEB_BIN_DIR}/safescribe-ai"

cat <<EOF > "${DEB_DESKTOP_DIR}/safescribe-ai.desktop"
[Desktop Entry]
Name=SafeScribe AI
Comment=Privacy-first, 100% Local AI Meeting Assistant
Exec=safescribe-ai
Icon=utilities-terminal
Terminal=false
Type=Application
Categories=Office;Utility;AudioVideo;
Keywords=Audio;Meeting;Transcription;Whisper;AI;
EOF

cat <<EOF > "${DEB_CONTROL_DIR}/control"
Package: safescribe-ai
Version: ${VERSION}
Architecture: amd64
Maintainer: SafeScribe AI Team <support@safescribe.ai>
Description: SafeScribe AI Meeting Assistant
 Privacy-first, 100% local AI meeting assistant that captures, transcribes,
 diarizes, and summarizes meetings entirely on local infrastructure.
EOF

echo "[4/4] Compiling Debian Package..."
DEB_OUT_PATH="${DIST_DIR}/safescribe-ai_${VERSION}_amd64.deb"

if command -v dpkg-deb &> /dev/null; then
    dpkg-deb --build "${DEB_DIR}" "${DEB_OUT_PATH}"
    echo "✓ Debian package created successfully at: ${DEB_OUT_PATH}"
else
    echo "dpkg-deb not found. Skipping .deb creation."
fi

echo "========================================================="
echo " Linux Build Summary:"
ls -lh "${DIST_DIR}"
echo "========================================================="
