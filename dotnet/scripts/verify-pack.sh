#!/bin/sh
set -eu

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
DOTNET_DIR=$(cd "$SCRIPT_DIR/.." && pwd)
PACKAGE_DIR=${1:?usage: verify-pack.sh <folder holding the .nupkg and .snupkg files>}
ENGINE=Geekmotors.Crucible
ADAPTER=Geekmotors.Crucible.Xunit
REPOSITORY_URL=https://github.com/Geekmotors/crucible-ai
FRAMEWORKS="net8.0 net10.0"
LC_ALL=C
export LC_ALL

VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$DOTNET_DIR/Directory.Build.props")
COMMIT=$(git -C "$DOTNET_DIR" rev-parse HEAD 2> /dev/null) || {
  echo "verify-pack: $DOTNET_DIR is not in a git checkout; the SourceLink commit cannot be checked" >&2
  exit 2
}
FAILURES=0

fail() {
  echo "verify-pack: $1" >&2
  FAILURES=$((FAILURES + 1))
}

expected_files() {
  printf '%s\n' "$1.nuspec" README.md
  for framework in $FRAMEWORKS; do
    printf '%s\n' "lib/$framework/$1.dll" "lib/$framework/$1.xml"
  done
  if [ "$1" = "$ADAPTER" ]; then
    printf '%s\n' "build/$ADAPTER.targets" "buildTransitive/$ADAPTER.targets"
  fi
}

expected_symbol_files() {
  printf '%s\n' "$1.nuspec"
  for framework in $FRAMEWORKS; do
    printf '%s\n' "lib/$framework/$1.pdb"
  done
}

check_listing() {
  unexpected=$(package_files "$2" | sort | diff - "$WORK/$1.expected" || true)
  [ -z "$unexpected" ] || fail "${2##*/} contents differ from the expected list (< packed, > expected):
$unexpected"
}

package_files() {
  unzip -Z1 "$1" | grep -v -e '^_rels/' -e '^\[Content_Types\]\.xml$' -e '^package/services/metadata/core-properties/'
}

nuspec_has() {
  if ! printf '%s' "$NUSPEC" | grep -F -q -- "$2"; then
    fail "$1.nuspec lacks $2"
  fi
}

check_package() {
  package=$PACKAGE_DIR/$1.$VERSION.nupkg
  symbols=$PACKAGE_DIR/$1.$VERSION.snupkg
  if [ ! -f "$package" ]; then
    fail "missing $package"
    return
  fi
  if ! unzip -tq "$package" > /dev/null 2>&1; then
    fail "$package is not a readable package (zip)"
    return
  fi
  [ -f "$symbols" ] || fail "missing symbol package $symbols"

  expected_files "$1" | sort > "$WORK/$1.expected"
  check_listing "$1" "$package"

  if ! NUSPEC=$(unzip -p "$package" "$1.nuspec" 2> /dev/null); then
    fail "$1.nupkg has no $1.nuspec"
    return
  fi
  nuspec_has "$1" "<id>$1</id>"
  nuspec_has "$1" "<version>$VERSION</version>"
  nuspec_has "$1" '<license type="expression">MIT</license>'
  nuspec_has "$1" '<readme>README.md</readme>'
  nuspec_has "$1" "<projectUrl>$REPOSITORY_URL</projectUrl>"
  REPOSITORY=$(printf '%s' "$NUSPEC" | grep -o '<repository [^>]*>' || true)
  for attribute in 'type="git"' "url=\"$REPOSITORY_URL\"" "commit=\"$COMMIT\""; do
    case "$REPOSITORY" in
      *" $attribute"*) ;;
      *) fail "$1.nuspec repository is '$REPOSITORY', expected $attribute" ;;
    esac
  done

  unzip -p "$package" README.md | cmp -s - "$DOTNET_DIR/README.md" || fail "$1.nupkg README.md differs from dotnet/README.md"

  [ -f "$symbols" ] || return 0
  if ! unzip -tq "$symbols" > /dev/null 2>&1; then
    fail "$symbols is not a readable symbol package (zip)"
    return
  fi
  expected_symbol_files "$1" | sort > "$WORK/$1.symbols.expected"
  check_listing "$1.symbols" "$symbols"
  for framework in $FRAMEWORKS; do
    unzip -p "$symbols" "lib/$framework/$1.pdb" > "$WORK/$1.pdb" 2> /dev/null || true
    grep -a -F -q "https://raw.githubusercontent.com/Geekmotors/crucible-ai/$COMMIT/" "$WORK/$1.pdb" ||
      fail "$1.snupkg lib/$framework/$1.pdb has no SourceLink to $REPOSITORY_URL at commit $COMMIT (the SDK takes it from the git remote origin)"
  done
}

check_engine_assembly() {
  for framework in $FRAMEWORKS; do
    if ! unzip -p "$PACKAGE_DIR/$ENGINE.$VERSION.nupkg" "lib/$framework/$ENGINE.dll" > "$WORK/$ENGINE.dll" 2> /dev/null; then
      fail "$ENGINE.nupkg has no lib/$framework/$ENGINE.dll"
      continue
    fi
    for schema in coherent extract; do
      grep -a -F -q "schemas/$schema.md" "$WORK/$ENGINE.dll" || fail "lib/$framework/$ENGINE.dll does not embed schemas/$schema.md"
    done
    grep -a -q "CrucibleSpec.dotnet-v$VERSION_PATTERN" "$WORK/$ENGINE.dll" ||
      fail "lib/$framework/$ENGINE.dll does not declare CrucibleSpec dotnet-v$VERSION"
  done
}

check_folder_holds_only_the_release() {
  for name in $ENGINE $ADAPTER; do
    printf '%s\n' "$name.$VERSION.nupkg" "$name.$VERSION.snupkg"
  done | sort > "$WORK/folder.expected"
  extra=$(ls -A "$PACKAGE_DIR" | sort | comm -23 - "$WORK/folder.expected")
  [ -z "$extra" ] || fail "$PACKAGE_DIR holds files that are not part of the release (they would be published):
$extra"
}

check_exact_dependency() {
  NUSPEC=$(unzip -p "$PACKAGE_DIR/$ADAPTER.$VERSION.nupkg" "$ADAPTER.nuspec" 2> /dev/null) || return 0
  pinned=$(printf '%s' "$NUSPEC" | grep -c -F "<dependency id=\"$ENGINE\" version=\"[$VERSION]\"" || true)
  any=$(printf '%s' "$NUSPEC" | grep -c -F "<dependency id=\"$ENGINE\"" || true)
  frameworks=$(printf '%s\n' $FRAMEWORKS | wc -l | tr -d ' ')
  if [ "$pinned" != "$frameworks" ] || [ "$any" != "$frameworks" ]; then
    fail "$ADAPTER must depend on $ENGINE [$VERSION] (exactly) for each of $FRAMEWORKS; found $pinned pinned of $any"
  fi
}

case "$VERSION" in
  '' | *[!0-9A-Za-z.-]*)
    echo "verify-pack: expected exactly one <Version> in dotnet/Directory.Build.props, found '$VERSION'" >&2
    exit 2
    ;;
esac
VERSION_PATTERN=$(printf '%s' "$VERSION" | sed 's/[.]/\\./g')
[ -d "$PACKAGE_DIR" ] || { echo "verify-pack: no folder $PACKAGE_DIR" >&2; exit 2; }
WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT
trap 'exit 130' HUP INT TERM

check_folder_holds_only_the_release
check_package "$ENGINE"
check_package "$ADAPTER"
[ -f "$PACKAGE_DIR/$ENGINE.$VERSION.nupkg" ] && check_engine_assembly
[ -f "$PACKAGE_DIR/$ADAPTER.$VERSION.nupkg" ] && check_exact_dependency

if [ "$FAILURES" -gt 0 ]; then
  echo "verify-pack: $FAILURES problem(s) in $PACKAGE_DIR" >&2
  exit 1
fi
echo "verify-pack: $ENGINE and $ADAPTER $VERSION OK (commit $COMMIT)"
