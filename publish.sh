#!/usr/bin/env bash
# publish.sh — builds self-contained single-file binaries for all targets
set -euo pipefail

PROJECT="src/TermBullet/TermBullet.csproj"
OUT_BASE="publish"

rids=(
  "win-x64"
  "linux-x64"
  "osx-x64"
  "osx-arm64"
)

for rid in "${rids[@]}"; do
  echo ">>> Publishing $rid..."
  dotnet publish "$PROJECT" \
    -c Release \
    -r "$rid" \
    -o "$OUT_BASE/$rid" \
    --self-contained true
  echo "    -> $OUT_BASE/$rid"
done

echo ""
echo "Done. Artifacts:"
for rid in "${rids[@]}"; do
  ls "$OUT_BASE/$rid/"
done
