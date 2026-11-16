# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A fork of [mob-sakai/AtlasImage](https://github.com/mob-sakai/AtlasImage), shipped as a source-only UPM package (`com.robbie111.atlas-image`). `AtlasImage` is a uGUI `Image` subclass that draws a sprite chosen by `SpriteAtlas` + sprite name, plus editor tooling: an atlas-only sprite picker, an inspector 9-slice border editor, and Image ↔ AtlasImage conversion.

No TypeScript/Node, no npm scripts, no Makefile, no CI in this repo.

## Repository layout

The repository root is **not** a Unity project.

| Path | What it is |
|---|---|
| `Packages/com.robbie111.atlas-image/` | The UPM package — the only thing that ships |
| `SampleProject/` | A Unity 6000.2.15f1 project consuming the package via `file:../../Packages/com.robbie111.atlas-image` |

The sample exists in **two places on purpose**:

| Copy | Role |
|---|---|
| `Packages/com.robbie111.atlas-image/Samples~/AtlasImageSample/` | What ships. `Samples~` is invisible to Unity, so it is never compiled in place |
| `SampleProject/Assets/AtlasImageSample/` | The editable copy used for compile checks and manual testing |

Keep them in sync when changing the sample. Both copies share the same `.meta` GUIDs.

## Package metadata

| Field | Value |
|---|---|
| `name` | `com.robbie111.atlas-image` |
| `unity` (minimum) | `6000.0` |
| `dependencies` | `com.unity.ugui` 2.0.0 (includes TextMeshPro), `com.unity.2d.sprite` 1.0.0 |

## Assemblies

| asmdef | Platforms |
|---|---|
| `Runtime/Robbie111.AtlasImage.asmdef` | All |
| `Editor/Robbie111.AtlasImage.Editor.asmdef` | Editor only; references `Unity.2D.Sprite.Editor`, `Unity.TextMeshPro` |
| `Samples~/AtlasImageSample/Robbie111.AtlasImage.Sample.asmdef` | All |

Editor and Sample asmdefs reference the runtime asmdef by GUID `153531aa335e642b7bae162c97115e41`. **Never regenerate `.meta` files**: scenes and prefabs bind `AtlasImage` by its script GUID.

Namespace is `Robbie111.AtlasImage` for both runtime and editor code (single namespace by design — an `.Editor` sub-namespace would shadow `UnityEditor.Editor`). The sample uses `Robbie111.AtlasImage.Samples`.

## Code architecture

### Runtime — `AtlasImage.cs`

- Overrides `SetMaterialDirty()` to detect sprite-name changes and call `spriteAtlas.GetSprite(spriteName)`.
- When an animation sets `sprite` directly, strips the `"(Clone)"` suffix and syncs `m_SpriteName` back.
- `OnPopulateMesh()` clears the mesh when `overrideSprite` is null (avoids drawing an empty white Image).

### Editor

- `AtlasImageEditor.cs` — inspector, atlas/sprite popups, and the `CONTEXT/Image` convert menu items; re-packs an atlas with the public `SpriteAtlasUtility.PackAtlases` after a border change.
- `SpritePreview.cs` — draws the preview through internal `SpriteDrawUtility.DrawSprite` (reflection, `MethodInfo.Invoke`); applying a border calls `TextureImporter.SaveAndReimport()` and then fires `onApplyBorder` so the editor re-packs the atlas.
- `AtlasSpritePickerWindow.cs` — atlas-only sprite picker.
- `AtlasImageMenuItems.cs`, `ImageToAtlasImageWindow.cs` — `GameObject/UI/...` menu items and the bulk Image → AtlasImage converter.

## Required project setting

`Edit > Project Settings > Editor > Sprite Packing Mode` must be `Sprite Atlas V2 - Enabled` or `Sprite Atlas V2 - Enabled for Builds`. With `Disabled`, AtlasImage does not render correctly in the editor.

## Verifying changes

Open `SampleProject/` in Unity 6000.2.15f1, confirm it compiles, and play `Assets/AtlasImageSample/Scenes/AtlasImageSample.unity` (the bottom buttons swap sprites by name). There are no automated tests.

## Release

Bump `version` in `package.json`, add a `CHANGELOG.md` entry (Keep a Changelog), and tag `vX.Y.Z` so consumers can pin `#vX.Y.Z` in the Git URL.
