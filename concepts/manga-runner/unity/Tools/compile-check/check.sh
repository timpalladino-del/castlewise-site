#!/usr/bin/env bash
# Compile-checks the Ink Run scripts outside Unity, against stand-in Unity APIs.
# Catches syntax and most type errors; it cannot catch wrong Unity API usage the stubs don't model.
# Needs the Mono C# compiler: apt-get install mono-mcs
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
scripts="$here/../../Assets/InkRun/Scripts"
out="$(mktemp -d)"
vt=/usr/lib/mono/4.5/Facades/System.ValueTuple.dll
mcs -target:library -langversion:latest -out:"$out/UnityStubs.dll" -nowarn:0169,0649,0414,0108,0660,0661 "$here/UnityStubs.cs"
for define in UNITY_EDITOR ""; do
  mcs -target:library -langversion:latest ${define:+-define:$define} -out:"$out/InkRun.dll" \
      -r:"$out/UnityStubs.dll" -r:"$vt" -nowarn:0169,0649,0414,0108 -recurse:"$scripts/*.cs"
  echo "ok: ${define:-device build}"
done
rm -rf "$out"
