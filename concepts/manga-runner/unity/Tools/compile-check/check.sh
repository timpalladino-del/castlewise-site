#!/usr/bin/env bash
# Compile-checks the Ink Run scripts outside Unity against stand-in Unity APIs, then runs the
# pure-logic tests. Catches syntax, type errors and logic regressions; it cannot catch wrong
# Unity API usage the stubs don't model.
# Uses Mono's mcs when installed (apt-get install mono-devel); otherwise falls back to the Roslyn
# compiler and .NET runtime bundled inside any installed Unity 6 Editor (no extra install on a Mac).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
scripts="$here/../../Assets/InkRun/Scripts"
tests="$here/../../Assets/InkRun/Tests"
out="$(mktemp -d)"
trap 'rm -rf "${out:?}"' EXIT
W="-nowarn:0169,0649,0414,0108,0660,0661"

if command -v mcs >/dev/null 2>&1; then
  vt=/usr/lib/mono/4.5/Facades/System.ValueTuple.dll
  cs() { mcs -langversion:latest $W "$@"; }
  lib() { cs -target:library "$@"; }
  exe() { cs -target:exe "$@"; }
  run() { (cd "$out" && mono TestRunner.exe InkRunTests.dll); }
  extra=(-r:"$vt")
else
  scripting=""
  for d in $(ls -d /Applications/Unity/Hub/Editor/*/Unity.app/Contents/Resources/Scripting 2>/dev/null | sort -V); do
    [ -f "$d/DotNetSdkRoslyn/csc.dll" ] && [ -x "$d/NetCoreRuntime/dotnet" ] && scripting="$d"
  done
  if [ -z "$scripting" ]; then echo "check.sh: need mono (mcs) or a Unity 6 Editor with DotNetSdkRoslyn" >&2; exit 2; fi
  dotnet="$scripting/NetCoreRuntime/dotnet"
  csc="$scripting/DotNetSdkRoslyn/csc.dll"
  fw="$(ls -d "$scripting"/NetCoreRuntime/shared/Microsoft.NETCore.App/* | sort -V | tail -1)"
  fwver="$(basename "$fw")"
  refs=()
  for f in "$fw"/System.Runtime.dll "$fw"/System.Private.CoreLib.dll "$fw"/System.Collections.dll "$fw"/System.Console.dll "$fw"/System.Linq.dll "$fw"/System.Runtime.Extensions.dll "$fw"/netstandard.dll "$fw"/mscorlib.dll; do refs+=(-r:"$f"); done
  cs() { "$dotnet" "$csc" -nologo -noconfig -langversion:latest $W "${refs[@]}" "$@"; }
  lib() { cs -target:library "$@"; }
  exe() { cs -target:exe "$@"; printf '{"runtimeOptions":{"tfm":"net6.0","framework":{"name":"Microsoft.NETCore.App","version":"%s"}}}' "$fwver" > "$out/TestRunner.runtimeconfig.json"; }
  run() { (cd "$out" && "$dotnet" TestRunner.dll InkRunTests.dll); }
  extra=()
  echo "compiler: Unity-bundled Roslyn ($scripting)"
fi

lib -out:"$out/UnityStubs.dll" "$here/UnityStubs.cs"
for define in UNITY_EDITOR "" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM"; do
  lib ${define:+-define:"$define"} -out:"$out/InkRun.dll" -r:"$out/UnityStubs.dll" ${extra[@]+"${extra[@]}"} -recurse:"$scripts/*.cs"
  echo "compile ok: ${define:-device build}"
done
lib -out:"$out/NUnitStub.dll" "$here/NUnitStub.cs"
lib -out:"$out/InkRunTests.dll" -r:"$out/InkRun.dll" -r:"$out/UnityStubs.dll" -r:"$out/NUnitStub.dll" ${extra[@]+"${extra[@]}"} -recurse:"$tests/*.cs"
if command -v mcs >/dev/null 2>&1; then exe -out:"$out/TestRunner.exe" -r:"$out/NUnitStub.dll" "$here/TestRunner.cs"; else exe -out:"$out/TestRunner.dll" -r:"$out/NUnitStub.dll" "$here/TestRunner.cs"; fi
run
