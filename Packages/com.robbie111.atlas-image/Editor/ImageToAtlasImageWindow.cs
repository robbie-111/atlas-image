using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Robbie111.AtlasImage
{
    /// <summary>
    /// 선택한 GameObject의 Image 컴포넌트를 AtlasImage로 일괄 변환하는 EditorWindow.
    /// - Image의 모든 속성(color, type, fill 등)과 RectTransform을 보존한다.
    /// - 기존 sprite.name을 spriteName에 자동 매핑한다.
    /// - 변환 전체를 하나의 Undo 그룹으로 묶어 Ctrl+Z 한 번으로 되돌릴 수 있다.
    /// - 선택된 SpriteAtlas는 static으로 유지되어 창을 재열기 시에도 그대로 남는다.
    /// </summary>
    public class ImageToAtlasImageWindow : EditorWindow
    {
        private static GameObject[] _selectedObjects;
        private static SpriteAtlas _selectedAtlas;

        // ── MenuItem (AtlasImageMenuItems에서 위임 호출) ───────────────────────

        internal static void ShowWindow()
        {
            _selectedObjects = Selection.gameObjects;
            if (_selectedObjects.Length == 0) return;

            var window = GetWindow<ImageToAtlasImageWindow>(true, "Image를 AtlasImage로 변환", true);
            window.minSize = new Vector2(400, 140);
            window.maxSize = new Vector2(400, 140);
            window.ShowUtility();
        }

        internal static bool ValidateShowWindow()
        {
            return Selection.gameObjects.Any(go =>
            {
                var img = go.GetComponent<Image>();
                return img != null && !(img is AtlasImage);
            });
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            // 변환 가능한 오브젝트 수 계산
            int convertible = _selectedObjects == null ? 0 : _selectedObjects.Count(go =>
            {
                var img = go.GetComponent<Image>();
                return img != null && !(img is AtlasImage);
            });

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                $"선택된 오브젝트: {_selectedObjects?.Length ?? 0}개  (변환 대상: {convertible}개)",
                EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            // AtlasImageEditor.DrawAtlasPopup() 재사용
            AtlasImageEditor.DrawAtlasPopup(
                EditorGUILayout.GetControlRect(),
                new GUIContent("SpriteAtlas"),
                new GUIContent("선택하세요"),
                _selectedAtlas,
                atlas => { _selectedAtlas = atlas; Repaint(); }
            );

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(_selectedAtlas == null || convertible == 0))
                {
                    if (GUILayout.Button("변환", GUILayout.Height(30)))
                    {
                        ApplyConversion(_selectedObjects, _selectedAtlas);
                        Close();
                    }
                }

                if (GUILayout.Button("취소", GUILayout.Height(30)))
                    Close();
            }

            EditorGUILayout.Space(4);
        }

        // ── 변환 로직 ──────────────────────────────────────────────────────────

        private static void ApplyConversion(GameObject[] selection, SpriteAtlas atlas)
        {
            if (atlas == null || selection == null) return;

            int groupId = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Convert Image to AtlasImage");

            int convertedCount = 0;

            foreach (var go in selection)
            {
                var image = go.GetComponent<Image>();
                if (image == null || image is AtlasImage) continue;

                // ── Image 속성 저장 ──
                var sprite              = image.sprite;
                var color               = image.color;
                var material            = image.material;
                var raycastTarget       = image.raycastTarget;
                var raycastPadding      = image.raycastPadding;
                var maskable            = image.maskable;
                var imageType           = image.type;
                var fillMethod          = image.fillMethod;
                var fillOrigin          = image.fillOrigin;
                var fillAmount          = image.fillAmount;
                var fillClockwise       = image.fillClockwise;
                var preserveAspect      = image.preserveAspect;
                var pixelsPerUnit       = image.pixelsPerUnitMultiplier;
                var wasEnabled          = image.enabled;

                // ── RectTransform 속성 저장 ──
                var rt              = go.transform as RectTransform;
                var anchoredPos     = rt.anchoredPosition;
                var anchorMin       = rt.anchorMin;
                var anchorMax       = rt.anchorMax;
                var sizeDelta       = rt.sizeDelta;
                var pivot           = rt.pivot;

                // ── Image 제거 → AtlasImage 추가 ──
                Undo.DestroyObjectImmediate(image);
                var atlasImage = Undo.AddComponent<AtlasImage>(go);

                // ── 속성 복원 ──
                atlasImage.color                  = color;
                atlasImage.material               = material;
                atlasImage.raycastTarget          = raycastTarget;
                atlasImage.raycastPadding         = raycastPadding;
                atlasImage.maskable               = maskable;
                atlasImage.type                   = imageType;
                atlasImage.fillMethod             = fillMethod;
                atlasImage.fillOrigin             = fillOrigin;
                atlasImage.fillAmount             = fillAmount;
                atlasImage.fillClockwise          = fillClockwise;
                atlasImage.preserveAspect         = preserveAspect;
                atlasImage.pixelsPerUnitMultiplier = pixelsPerUnit;
                atlasImage.enabled                = wasEnabled;

                // ── AtlasImage 전용 속성 설정 ──
                atlasImage.spriteAtlas = atlas;
                if (sprite != null)
                    atlasImage.spriteName = sprite.name;

                // ── RectTransform 복원 ──
                rt.pivot            = pivot;
                rt.anchorMin        = anchorMin;
                rt.anchorMax        = anchorMax;
                rt.anchoredPosition = anchoredPos;
                rt.sizeDelta        = sizeDelta;

                EditorUtility.SetDirty(go);
                convertedCount++;
            }

            // 변환 전체를 하나의 Undo 그룹으로 묶는다
            Undo.CollapseUndoOperations(groupId);

            if (convertedCount > 0)
                Debug.Log($"[AtlasImage] {convertedCount}개의 Image를 AtlasImage로 변환 완료 (Atlas: {atlas.name})");
        }
    }
}
