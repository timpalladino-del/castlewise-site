#!/usr/bin/env bash
# Ingest generated GLBs from concepts/manga-runner/gen/in into Assets/InkRun/Models.
# Usage: bash ingest.sh [Name ...]   (default: every GLB in gen/in)   env: YAW=<deg> BUDGET=<tris>
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"; ROOT="$(cd "$HERE/../../.." && pwd)"
IN="$ROOT/gen/in"; OUT="$ROOT/unity/Assets/InkRun/Models"; REVIEW="$ROOT/gen/review"; mkdir -p "$REVIEW"
declare -A HEIGHT=( [Hero]=1.95 [Crew]=1.75 [Gang]=1.75 [Train]=3.0 [Kaiju]=6.0 [Barrier]=1.2 [Sign]=2.3 [Coin]=0.75 [Lantern]=1.3 [Magnet]=1.4 [Dash]=1.4
                    [BuildingCity]=7.7 [BuildingAlley]=9.1 [Containers]=8.2 [KaijuShadow]=0.02 )
declare -A HERO=( [Hero]=1 [Crew]=1 [Gang]=1 )
declare -A ANCHOR=( [Train]=front [BuildingCity]=corner [BuildingAlley]=corner [Containers]=corner )
declare -A OUTNAME=( [Gang]=Crew )      # the chaser prefab loads Models/Crew.glb
names=("$@"); if [ ${#names[@]} -eq 0 ]; then for f in "$IN"/*.glb; do [ -e "$f" ] && names+=("$(basename "${f%.glb}")"); done; fi
for n in "${names[@]}"; do
  src="$IN/$n.glb"; [ -e "$src" ] || { echo "missing $src"; continue; }
  h="${HEIGHT[$n]:-1.0}"; extra="--anchor ${ANCHOR[$n]:-center}"; [ -n "${HERO[$n]:-}" ] && extra="$extra --hero"; outn="${OUTNAME[$n]:-$n}"
  ref=""; [ -e "$ROOT/gen/refs/$(echo $n | tr A-Z a-z)_front.jpg" ] && ref="--ref $ROOT/gen/refs/$(echo $n | tr A-Z a-z)_front.jpg"
  [ "$n" = "Hero" ] && [ -e "$ROOT/gen/refs/nozomi_front.jpg" ] && ref="--ref $ROOT/gen/refs/nozomi_front.jpg"
  python3 "$HERE/import_gen.py" "$src" "$OUT/$outn.glb" --name "$outn" --height "$h" --yaw "${YAW:-0}" --budget "${BUDGET:-15000}" $extra --auto-joints $ref 2>&1 | grep -v "INFO\|Draco\|MeshOpt" | tail -2
done
echo "done -> $OUT"
