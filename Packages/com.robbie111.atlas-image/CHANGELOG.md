# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0]

First release under the new package id and repository layout. Runtime behavior is the same as `com.111percent.thirdparty.atlas-image` 1.0.3.

### Changed

- **Breaking — package id** changed from `com.111percent.thirdparty.atlas-image` to `com.robbie111.atlas-image`, installed from a Git URL.
- **Breaking — assembly names** changed: `Percent.AtlasImage.Runtime` → `Robbie111.AtlasImage`, `Percent.AtlasImage.Editor` → `Robbie111.AtlasImage.Editor`. Asmdefs that reference the package by GUID keep working; references by name must be updated.
- **Breaking — namespace** changed from `Percent.AtlasImage` to `Robbie111.AtlasImage`. Script GUIDs are unchanged, so serialized `AtlasImage` components in scenes and prefabs stay connected.
- Minimum Unity version raised to `6000.0`. Declared dependencies are now `com.unity.ugui` 2.0.0 and `com.unity.2d.sprite`, which the editor code actually uses; the unused `com.unity.test-framework` dependency was removed.
- The sample now ships through Package Manager (`Samples~/AtlasImageSample`) and contains only the Demo atlas.

## Prior history (`com.111percent.thirdparty.atlas-image`)

### 1.0.3 - 2026-04-23

- Fixed: Replaced the reflection call to `SpriteAtlasUtility.PackAtlases` with a direct call to the public Unity 6 API.

### 1.0.2 - 2026-04-17

- Added: Custom sprite picker window (Unity 6 compatibility).
- Added: Editor window that converts Images to AtlasImage in bulk.
- Added: GameObject menu items for AtlasImage.

### 1.0.1 - 2026-01-20

- Added: User guide.

### 1.0.0 - 2026-01-16

- Added: AtlasImage component for UI.
