using TMPro;
using UnityEditor;
using UnityEditor.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Robbie111.AtlasImage
{
    /// <summary>
    /// GameObject/UI 메뉴에 AtlasImage 관련 항목을 등록한다.
    /// </summary>
    internal static class AtlasImageMenuItems
    {
        private const string kUILayerName = "UI";

        // ── Atlas Image ────────────────────────────────────────────────────────

        [MenuItem("GameObject/UI/Atlas Image")]
        private static void CreateAtlasImage(MenuCommand menuCommand)
        {
            var go = ObjectFactory.CreateGameObject("Atlas Image", typeof(RectTransform));
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            ObjectFactory.AddComponent<CanvasRenderer>(go);
            ObjectFactory.AddComponent<AtlasImage>(go);

            PlaceUIElementRoot(go, menuCommand);
        }

        // ── Button - Atlas Image ───────────────────────────────────────────────

        [MenuItem("GameObject/UI/Button - Atlas Image")]
        private static void CreateAtlasImageButton(MenuCommand menuCommand)
        {
            // 루트: Button - Atlas Image
            var root = ObjectFactory.CreateGameObject("Button - Atlas Image", typeof(RectTransform));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(160f, 30f);
            ObjectFactory.AddComponent<CanvasRenderer>(root);

            var atlasImage = ObjectFactory.AddComponent<AtlasImage>(root);
            atlasImage.color = Color.white;

            var button = ObjectFactory.AddComponent<Button>(root);
            button.targetGraphic = atlasImage;
            SetDefaultColorTransitionValues(button);

            // 자식: Text (TMP)
            var childText = ObjectFactory.CreateGameObject("Text (TMP)", typeof(RectTransform));
            Undo.SetTransformParent(childText.transform, root.transform, "");
            SetLayerRecursively(childText, root.layer);

            var text = ObjectFactory.AddComponent<TextMeshProUGUI>(childText);
            text.text = "Button";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            var textRect = childText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            PlaceUIElementRoot(root, menuCommand);
        }

        // ── Convert Image to AtlasImage ────────────────────────────────────────

        [MenuItem("GameObject/UI/Convert Image to AtlasImage")]
        private static void ShowConvertWindow(MenuCommand menuCommand)
        {
            ImageToAtlasImageWindow.ShowWindow();
        }

        [MenuItem("GameObject/UI/Convert Image to AtlasImage", true)]
        private static bool ValidateShowConvertWindow()
        {
            return ImageToAtlasImageWindow.ValidateShowWindow();
        }

        // ── Canvas 헬퍼 ────────────────────────────────────────────────────────

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;

            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;

                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                var canvas = CreateNewUI();
                Undo.SetTransformParent(canvas.transform, parent.transform, "");
                parent = canvas;
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);
            SetParentAndAlign(element, parent);

            if (!explicitParentChoice)
                SetPositionVisibleInSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Undo.RegisterFullObjectHierarchyUndo(parent == null ? element : parent, "");
            Undo.SetCurrentGroupName("Create " + element.name);

            Selection.activeGameObject = element;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null) return;

            Undo.SetTransformParent(child.transform, parent.transform, "");

            var rectTransform = child.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                var localPosition = rectTransform.localPosition;
                localPosition.z = 0;
                rectTransform.localPosition = localPosition;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
            }

            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        private static void SetPositionVisibleInSceneView(RectTransform canvasRect, RectTransform itemRect)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            if (sceneView == null || sceneView.camera == null) return;

            var camera = sceneView.camera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f),
                    camera,
                    out var localPos))
                return;

            localPos.x = Mathf.Clamp(
                localPos.x + canvasRect.sizeDelta.x * canvasRect.pivot.x,
                0, canvasRect.sizeDelta.x);
            localPos.y = Mathf.Clamp(
                localPos.y + canvasRect.sizeDelta.y * canvasRect.pivot.y,
                0, canvasRect.sizeDelta.y);

            var position = new Vector3(
                localPos.x - canvasRect.sizeDelta.x * itemRect.anchorMin.x,
                localPos.y - canvasRect.sizeDelta.y * itemRect.anchorMin.y,
                0);

            var minLocal = new Vector3(
                canvasRect.sizeDelta.x * (0 - canvasRect.pivot.x) + itemRect.sizeDelta.x * itemRect.pivot.x,
                canvasRect.sizeDelta.y * (0 - canvasRect.pivot.y) + itemRect.sizeDelta.y * itemRect.pivot.y,
                0);
            var maxLocal = new Vector3(
                canvasRect.sizeDelta.x * (1 - canvasRect.pivot.x) - itemRect.sizeDelta.x * itemRect.pivot.x,
                canvasRect.sizeDelta.y * (1 - canvasRect.pivot.y) - itemRect.sizeDelta.y * itemRect.pivot.y,
                0);

            position.x = Mathf.Clamp(position.x, minLocal.x, maxLocal.x);
            position.y = Mathf.Clamp(position.y, minLocal.y, maxLocal.y);

            itemRect.anchoredPosition = position;
            itemRect.localRotation = Quaternion.identity;
            itemRect.localScale = Vector3.one;
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            var selectedGo = Selection.activeGameObject;
            var canvas = selectedGo != null ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas)) return canvas.gameObject;

            var canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            foreach (var c in canvasArray)
                if (IsValidCanvas(c)) return c.gameObject;

            return CreateNewUI();
        }

        private static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy) return false;
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0) return false;
            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle()) return false;
            return true;
        }

        private static GameObject CreateNewUI()
        {
            var root = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer(kUILayerName);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            StageUtility.PlaceGameObjectInCurrentStage(root);

            bool customScene = false;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
                customScene = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            if (!customScene)
                EnsureEventSystem();

            return root;
        }

        private static void EnsureEventSystem()
        {
            var esys = Object.FindFirstObjectByType<EventSystem>();
            if (esys != null) return;

            var eventSystem = ObjectFactory.CreateGameObject("EventSystem");
            ObjectFactory.AddComponent<EventSystem>(eventSystem);
            InputModuleComponentFactory.AddInputModule(eventSystem);
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        private static void SetDefaultColorTransitionValues(Selectable selectable)
        {
            var colors = selectable.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
            selectable.colors = colors;
        }
    }
}
