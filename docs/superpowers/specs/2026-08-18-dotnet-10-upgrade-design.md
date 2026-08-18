# .NET 10 Upgrade Design

## Objective

Upgrade the supported cross-platform Event Assembler Core release from .NET 6
to .NET 10, push the validated change to `master`, and publish a new GitHub
release with the existing Linux, Windows, and macOS artifacts.

## Success Conditions

- `Nintenlord/Nintenlord.csproj` and `Event Assembler/Core/Core.csproj` target
  `net10.0`.
- GitHub Actions installs the .NET 10 SDK and continues to build all three
  operating-system artifacts.
- A clean local restore, Release build, and publish complete without errors.
- The published dependency metadata identifies `.NETCoreApp,Version=v10.0`.
- The upgrade commit is present on `origin/master`.
- release `v2026.08.18` exists at that commit and contains the Linux, macOS,
  and Windows ZIP assets produced by a successful tag workflow.

## Scope

The release workflow currently builds only `Event Assembler/Core/Core.csproj`.
That project references `Nintenlord/Nintenlord.csproj`, so both SDK-style
projects must move together.

The legacy GUI and conversion utility projects remain on .NET Framework 4.0.
Migrating them would require a separate Windows-specific modernization effort
and would not affect the cross-platform Core artifacts requested here.

## Considered Approaches

1. **Retarget the supported Core graph to .NET 10.** Update the two SDK-style
   projects and CI SDK version. This is the recommended approach because it
   satisfies the release objective with the smallest compatibility surface.
2. **Multi-target .NET 6 and .NET 10.** This preserves the old runtime but does
   not make the release unambiguously .NET 10 and complicates artifact
   selection.
3. **Migrate every project to .NET 10.** The WinForms projects would require
   SDK conversion and `net10.0-windows`, expanding the work into an unrelated
   Windows-only port.

## Design

Change both `<TargetFramework>` values from `net6.0` to `net10.0`. Keep current
package references, compiler settings, project references, and framework-
dependent publishing behavior unchanged. Change `actions/setup-dotnet` from
`6.0.x` to `10.0.x`. Add a concise README requirement so consumers know the
new runtime baseline.

No source-code behavior changes or compatibility fallbacks are planned. Build
or packaging failures remain visible through normal `dotnet` and GitHub
Actions failures.

## Validation and Release Loop

1. Restore, build, and publish Core in Release mode with the installed .NET 10
   SDK.
2. Inspect the publish metadata and package contents to confirm the .NET 10
   target and expected distributable files.
3. Review the diff, commit it with the required co-author trailer, and push
   `master`.
4. Wait for the `master` CI workflow to succeed.
5. Create GitHub release `v2026.08.18` at the pushed commit.
6. Wait for the tag workflow to succeed, then verify the release is published
   and has exactly the expected three platform ZIP assets.
