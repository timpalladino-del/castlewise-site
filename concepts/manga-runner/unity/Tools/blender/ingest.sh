#!/usr/bin/env bash
# Ingest generated GLBs from concepts/manga-runner/gen/in into Assets/InkRun/Models.
# Usage: bash ingest.sh [Name ...]   (default: every GLB in gen/in)   env: YAW=<deg> BUDGET=<tris>
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"; ROOT="$(cd "$HERE/../../.." && pwd)"
IN="$ROOT/gen/in"; OUT="$ROOT/unity/Assets/InkRun/Models"; REVIEW="$ROOT/gen/review"; mkdir -p "$REVIEW"
declare -A HEIGHT=( [Hero]=1.95 [Gang]=1.95 [Train]=2.6 [Kaiju]=6.0 [Barrier]=1.1 [Sign]=2.3 [Coin]=0.7 [Lantern]=1.3 [Magnet]=1.4 [Dash]=1.4 )
declare -A HERO=( [Hero]=1 [Gang]=1 )
names=("$@"); if [ ${#names[@]} -eq 0 ]; then for f in "$IN"/*.glb; do [ -e "$f" ] && names+=("$(basename "${f%.glb}")"); done; fi
for n in "${names[@]}"; do
  src="$IN/$n.glb"; [ -e "$src" ] || { echo "missing $src"; continue; }
  h="${HEIGHT[$n]:-1.0}"; extra=""; [ -n "${HERO[$n]:-}" ] && extra="--hero"
  python3 "$HERE/import_gen.py" "$src" "$OUT/$n.glb" --height "$h" --yaw "${YAW:-0}" --budget "${BUDGET:-15000}" $extra 2>&1 | grep -v "INFO\|Draco\|MeshOpt" | tail -2
done
echo "done -> $OUT"
