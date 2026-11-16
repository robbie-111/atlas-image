# UI Atlas Image for Unity

uGUI용 `SpriteAtlas` 기반 Image 컴포넌트입니다. 아틀라스와 스프라이트 이름으로 이미지를 지정하고, 인스펙터에서 아틀라스 전용 스프라이트 선택기와 9-slice Border 편집기를 제공합니다.

[mob-sakai/AtlasImage](https://github.com/mob-sakai/AtlasImage)의 fork입니다.

## 레포 구성

레포 루트는 Unity 프로젝트가 아닙니다. 배포할 패키지와 그 패키지를 검증하는 프로젝트가 들어 있습니다.

| 경로 | 내용 |
|---|---|
| `Packages/com.robbie111.atlas-image/` | UPM 패키지. 실제로 배포되는 부분입니다. |
| `SampleProject/` | 상대 경로 `file:` 의존성으로 패키지를 쓰는 Unity 6000.2.15f1 프로젝트입니다. 샘플 씬 `Assets/AtlasImageSample/Scenes/AtlasImageSample.unity`가 빌드 설정에 등록되어 있습니다. |

## 설치

`Packages/manifest.json`에 Git URL로 추가합니다. 패키지가 하위 폴더에 있으므로 `?path=`가 필요합니다.

```json
"com.robbie111.atlas-image": "https://github.com/robbie-111/atlas-image.git?path=Packages/com.robbie111.atlas-image"
```

특정 리비전은 `...atlas-image#v1.0.0`처럼 끝에 붙입니다. 의존성(`com.unity.ugui`, `com.unity.2d.sprite`)은 Unity Registry에서 자동으로 설치됩니다.

## `com.111percent.thirdparty.atlas-image`에서 옮기기

패키지 id, 설치 경로, 어셈블리 이름, namespace가 모두 바뀌었습니다.

```diff
- "com.111percent.thirdparty.atlas-image": "1.0.3"
+ "com.robbie111.atlas-image": "https://github.com/robbie-111/atlas-image.git?path=Packages/com.robbie111.atlas-image"
```

- 코드의 `using Percent.AtlasImage;`를 `using Robbie111.AtlasImage;`로 바꿉니다.
- `.asmdef`가 어셈블리를 이름으로 참조했다면 `Percent.AtlasImage.Runtime` → `Robbie111.AtlasImage`, `Percent.AtlasImage.Editor` → `Robbie111.AtlasImage.Editor`로 바꿉니다. GUID로 참조했다면 그대로 동작합니다.
- 스크립트 GUID는 그대로이므로 씬·프리팹에 저장된 `AtlasImage` 컴포넌트는 다시 연결할 필요가 없습니다.
- 최소 Unity 버전은 `6000.0`입니다.

## 사용법

설정, 메뉴, 코드 예제, 리소스 구성 가이드는 [패키지 README](Packages/com.robbie111.atlas-image/README.md)를 참고하세요. 실행 가능한 샘플은 Package Manager 창에서 가져올 수 있습니다.

## 라이선스

MIT. [LICENSE](Packages/com.robbie111.atlas-image/LICENSE)를 참고하세요.
