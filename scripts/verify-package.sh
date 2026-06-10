#!/usr/bin/env bash
# Verifies that the produced Stricture.Net .nupkg contains both required assets at the correct paths.
set -euo pipefail

CONFIG="${1:-Release}"
PKG_DIR="src/Stricture.Package/bin/${CONFIG}"

nupkg="$(ls -1 "${PKG_DIR}"/Stricture.Net.*.nupkg 2>/dev/null | head -n 1 || true)"
if [[ -z "${nupkg}" ]]; then
  echo "ERROR: no Stricture.Net.*.nupkg found in ${PKG_DIR}. Run 'dotnet pack -c ${CONFIG}' first." >&2
  exit 1
fi

echo "Inspecting ${nupkg}"

# List archive entries (unzip -Z1 if available, else python's zipfile).
if command -v unzip >/dev/null 2>&1; then
  entries="$(unzip -Z1 "${nupkg}")"
else
  entries="$(python3 -c 'import sys,zipfile; print("\n".join(zipfile.ZipFile(sys.argv[1]).namelist()))' "${nupkg}")"
fi

required=(
  "analyzers/dotnet/cs/Stricture.Engine.dll"
  "lib/netstandard2.0/Stricture.Abstractions.dll"
)

status=0
for path in "${required[@]}"; do
  if echo "${entries}" | grep -qx "${path}"; then
    echo "  OK   ${path}"
  else
    echo "  MISS ${path}" >&2
    status=1
  fi
done

if [[ "${status}" -ne 0 ]]; then
  echo "ERROR: package is missing one or more required assets." >&2
  exit 1
fi

echo "Package layout verified."
