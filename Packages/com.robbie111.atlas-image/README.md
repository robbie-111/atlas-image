# UI Atlas Image

uGUI에서 `SpriteAtlas`를 직접 다루는 Image 컴포넌트입니다. 아틀라스와 스프라이트 이름으로 이미지를 지정하고, 인스펙터에서 아틀라스 전용 스프라이트 선택기와 9-slice Border 편집기를 제공합니다.

[mob-sakai/AtlasImage](https://github.com/mob-sakai/AtlasImage)를 fork해 버그를 고치고, Unity 6용 스프라이트 선택 창과 변환 도구를 추가했습니다.

## 주요 기능

1. `SpriteAtlas` + 스프라이트 이름으로 이미지 지정 및 런타임 변경
2. `SpriteAtlas` 전용 스프라이트 선택기
3. 인스펙터 프리뷰에서 Sprite Border(9-slice) 편집 (Sprite Editor를 열 필요 없음)
4. Image ↔ AtlasImage 변환 (컴포넌트 단위, 선택한 오브젝트 일괄 변환)

어떤 스프라이트가 어떤 아틀라스에 들어 있는지 인스펙터에서 바로 보이므로, 중복된 UI 리소스를 찾아 정리하기도 쉽습니다.

## 요구 사항

- Unity 6000.0 이상
- `com.unity.ugui` 2.0.0, `com.unity.2d.sprite` (패키지 의존성으로 자동 설치)

## 설치

Package Manager의 **Add package from git URL**에 입력합니다.

```text
https://github.com/robbie-111/atlas-image.git?path=Packages/com.robbie111.atlas-image
```

특정 리비전은 끝에 `#<태그 또는 커밋>`을 붙입니다.

## 프로젝트 설정

`Edit > Project Settings > Editor`의 **Sprite Packing Mode**를 `Disabled`에서 다음 중 하나로 바꿉니다. `Disabled`이면 에디터에서 AtlasImage가 제대로 표시되지 않습니다.

| 모드 | 동작 |
|---|---|
| **Sprite Atlas V2 - Enabled** | 편집 모드와 플레이 모드에서 항상 아틀라스 텍스처를 사용합니다. |
| **Sprite Atlas V2 - Enabled for Builds** | 플레이어·에셋 번들·Addressables 빌드 때만 패킹합니다. 편집 모드에서는 원본 텍스처를 씁니다. |

## 사용법

### 컴포넌트 추가와 변환

| 메뉴 | 동작 |
|---|---|
| `Add Component > AtlasImage` | AtlasImage 추가 |
| `GameObject > UI > Atlas Image` | AtlasImage 오브젝트 생성 |
| `GameObject > UI > Button - Atlas Image` | AtlasImage 배경 + TextMeshPro 텍스트를 가진 버튼 생성 |
| `GameObject > UI > Convert Image to AtlasImage` | 선택한 오브젝트들의 Image를 아틀라스를 골라 일괄 변환 |
| Image 컴포넌트 우클릭 → `Convert To AtlasImage` | 기존 Image를 AtlasImage로 변환 |
| AtlasImage 컴포넌트 우클릭 → `Convert To Image` | 기본 Image로 되돌림 |

`Button - Atlas Image`는 TextMeshPro를 쓰므로 프로젝트에 TMP Essential Resources가 없으면 Unity가 가져오기를 묻습니다.

### 인스펙터

- **Sprite Atlas** 드롭다운에서 아틀라스를 바꿉니다.
- **Sprite** 항목을 누르면 해당 아틀라스의 스프라이트만 보이는 선택 창이 열립니다.
- 프리뷰 영역의 **Border** 버튼으로 9-slice 영역을 직접 편집합니다. 적용하면 원본 텍스처의 Border가 저장되고 아틀라스가 다시 패킹됩니다.

### 코드

```csharp
using Robbie111.AtlasImage;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private AtlasImage atlasImage;

    public void ChangeSpriteName(string spriteName)
    {
        if (!atlasImage)
        {
            return;
        }

        atlasImage.spriteName = spriteName;   // 같은 아틀라스 안에서 이름으로 교체
    }
}
```

`atlasImage.spriteAtlas = myAtlas;`로 아틀라스도 코드에서 바꿀 수 있습니다. 애니메이션 등으로 `sprite`가 직접 바뀌면 `spriteName`도 그 스프라이트 이름으로 맞춰집니다.

## 샘플

Package Manager 창에서 이 패키지를 선택하고 **Samples > AtlasImage Sample > Import**를 누릅니다. 씬 `Scenes/AtlasImageSample.unity`에서 하단 버튼을 누르면 `ChangeSpriteName`으로 같은 아틀라스의 다른 스프라이트로 바뀝니다.

## SpriteAtlas 리소스 구성 가이드

AtlasImage를 쓰는 데 꼭 필요하지는 않은 권장 사항입니다. 자세한 내용은 [Unity 매뉴얼](https://docs.unity3d.com/kr/2022.3/Manual/sprite-atlas.html)을 참고하세요.

### 왜 아틀라스를 쓰는가

- **메모리**: GPU는 2의 거듭제곱(POT) 크기 텍스처를 가장 효율적으로 다룹니다. 작은 스프라이트를 POT 크기 텍스처 하나에 모아 패킹하면 낭비되는 영역이 줄어듭니다.
- **Draw Call**: 같은 아틀라스의 스프라이트는 한 번의 Draw Call로 묶일 수 있습니다.
- 배경처럼 크고 한 화면에 하나만 쓰는 텍스처는 아틀라스에 넣지 않는 편이 나을 수 있습니다. 같은 화면에 함께 나오는 리소스끼리 묶는 것이 핵심입니다.

### 폴더 구조

원본 스프라이트와 아틀라스 에셋을 분리하고, 스프라이트 폴더와 아틀라스를 1:1로 맞춥니다.

```text
Assets/UI/
 ├─ Sprites/
 │   ├─ Common/
 │   ├─ Lobby/
 │   └─ HUD/
 └─ Atlases/
     ├─ UI_Common_Atlas.spriteatlasv2
     ├─ UI_Lobby_Atlas.spriteatlasv2
     └─ UI_HUD_Atlas.spriteatlasv2
```

원본 스프라이트는 비압축으로 두는 것이 품질과 관리 면에서 안정적입니다.

### 이름 규칙 (예시)

- 아틀라스: `UI_{Category}_Atlas`, `UI_{Screen}_Atlas` — 예: `UI_Common_Atlas`, `UI_BattleHUD_Atlas`
- 스프라이트: `{type}_{name}_{state}` — 예: `btn_play_normal`, `icon_gold_small`, `bg_popup_dark`

### 분리 기준

- 항상 로드되는 UI는 `UI_Common_Atlas`로, 특정 화면에서만 쓰는 리소스는 그 화면 전용 아틀라스로 나눕니다.
- 너무 큰 아틀라스는 메모리 사용량이 늘고 패킹 효율이 떨어지므로 나눕니다.
- 애니메이션용 스프라이트는 별도 아틀라스로 관리합니다.
- 새 스프라이트를 추가하기 전에 기존 아틀라스에 비슷한 이미지가 있는지 AtlasImage 선택기로 확인하고, 여러 화면에서 쓰는 것은 Common으로 옮깁니다.

## 라이선스

MIT. [LICENSE](LICENSE)를 참고하세요.
