#!/usr/bin/env bash
# Compile-checks the Ink Run scripts outside Unity against stand-in Unity APIs, then runs the
# pure-logic tests. Catches syntax, type errors and logic regressions; it cannot catch wrong
# Unity API usage the stubs don't model. Needs Mono: apt-get install mono-devel
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
scripts="$here/../../Assets/InkRun/Scripts"
tests="$here/../../Assets/InkRun/Tests"
out="$(mktemp -d)"
vt=/usr/lib/mono/4.5/Facades/System.ValueTuple.dll
W="-nowarn:0169,0649,0414,0108,0660,0661"
mcs -target:library -langversion:latest -out:"$out/UnityStubs.dll" $W "$here/UnityStubs.cs"
for define in UNITY_EDITOR "" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  mcs -target:library -langversion:latest ${define:+-define:$define} -out:"$out/InkRun.dll" \
      -r:"$out/UnityStubs.dll" -r:"$vt" $W -recurse:"$scripts/*.cs"
  echo "compile ok: ${define:-device build}"
done
mcs -target:library -langversion:latest -out:"$out/NUnitStub.dll" "$here/NUnitStub.cs"
mcs -target:library -langversion:latest -out:"$out/InkRunTests.dll" -r:"$out/InkRun.dll" -r:"$out/UnityStubs.dll" -r:"$out/NUnitStub.dll" -r:"$vt" $W -recurse:"$tests/*.cs"
mcs -target:exe -langversion:latest -out:"$out/TestRunner.exe" -r:"$out/NUnitStub.dll" "$here/TestRunner.cs"
(cd "$out" && mono TestRunner.exe InkRunTests.dll)
rm -rf "${out:?}"
