using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

namespace Robbie111.AtlasImage
{
    /// <summary>
    /// 커스텀 EditorWindow 기반 스프라이트 피커
    /// - 검색 필드로 이름 필터링
    /// - 썸네일 그리드 프리뷰 (AssetPreview)
    /// - PackAtlas / SetAtlasLabelToSprites 불필요
    /// </summary>
    public class AtlasSpritePickerWindow : EditorWindow
    {
        private const float ThumbnailSize = 64f;
        private const float Padding = 6f;
        private const float LabelHeight = 16f;
        private const float CellSize = ThumbnailSize + LabelHeight + Padding * 2f;
        private const float MinWindowWidth = 320f;
        private const float WindowHeight = 420f;

        private SpriteAtlas _atlas;
        private UnityAction<string> _onChange;
        private string _currentSpriteName;

        private string _searchText = "";
        private Vector2 _scrollPos;

        private Sprite[] _allSprites;
        private List<Sprite> _filteredSprites = new List<Sprite>();

        private GUIStyle _selectedStyle;
        private GUIStyle _cellStyle;

        /// <summary>
        /// Atlas 스프라이트 피커 창을 엽니다.
        /// </summary>
        public static void Show(SpriteAtlas atlas, string currentSpriteName, UnityAction<string> onChange)
        {
            var window = GetWindow<AtlasSpritePickerWindow>(true, "Sprite Picker", true);
            window.Init(atlas, currentSpriteName, onChange);
            window.minSize = new Vector2(MinWindowWidth, WindowHeight);
            window.ShowUtility();
        }

        private void Init(SpriteAtlas atlas, string currentSpriteName, UnityAction<string> onChange)
        {
            _atlas = atlas;
            _currentSpriteName = currentSpriteName;
            _onChange = onChange;
            _searchText = "";
            _scrollPos = Vector2.zero;

            LoadSprites();

            // 스프라이트 수에 맞게 AssetPreview 캐시 크기를 확장한다.
            // 기본 캐시 크기(88)를 초과하는 아틀라스(예: 398개)에서
            // 썸네일이 계속 evict되어 "..." 상태가 유지되는 문제를 방지한다.
            AssetPreview.SetPreviewTextureCacheSize(_allSprites.Length + 16);
        }

        private void LoadSprites()
        {
            if (!_atlas)
            {
                _allSprites = Array.Empty<Sprite>();
                _filteredSprites.Clear();
                return;
            }

            var sp = new SerializedObject(_atlas).FindProperty("m_PackedSprites");
            _allSprites = sp == null
                ? Array.Empty<Sprite>()
                : Enumerable.Range(0, sp.arraySize)
                    .Select(i => sp.GetArrayElementAtIndex(i).objectReferenceValue as Sprite)
                    .Where(s => s)
                    .OrderBy(s => s.name)
                    .ToArray();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filteredSprites.Clear();
            if (string.IsNullOrEmpty(_searchText))
            {
                _filteredSprites.AddRange(_allSprites);
            }
            else
            {
                var lower = _searchText.ToLower();
                _filteredSprites.AddRange(_allSprites.Where(s => s.name.ToLower().Contains(lower)));
            }
        }

        private void InitStyles()
        {
            if (_selectedStyle != null) return;

            _selectedStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0.24f, 0.49f, 0.90f, 0.6f)) }
            };

            _cellStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 9,
                clipping = TextClipping.Clip
            };
        }

        private void Update()
        {
            // AssetPreview가 백그라운드에서 썸네일을 생성하는 동안 자동으로 화면을 갱신한다.
            // 이 루프가 없으면 마우스 이벤트가 없는 한 "..." 상태가 유지된다.
            if (AssetPreview.IsLoadingAssetPreviews())
                Repaint();
        }

        private void OnGUI()
        {
            InitStyles();

            // 검색 바
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Search", EditorStyles.toolbarButton, GUILayout.Width(50));
                var newSearch = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
                if (newSearch != _searchText)
                {
                    _searchText = newSearch;
                    ApplyFilter();
                    _scrollPos = Vector2.zero;
                }

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)) && !string.IsNullOrEmpty(_searchText))
                {
                    _searchText = "";
                    ApplyFilter();
                    _scrollPos = Vector2.zero;
                    GUI.FocusControl(null);
                }
            }

            // 상태 바
            EditorGUILayout.LabelField($"{_filteredSprites.Count} / {_allSprites.Length} 개", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);

            // 그리드 영역
            float areaWidth = position.width - 16f;
            int cols = Mathf.Max(1, Mathf.FloorToInt(areaWidth / CellSize));
            int rows = Mathf.CeilToInt((_filteredSprites.Count + 1f) / cols); // +1: 선택 없음 셀
            float totalHeight = rows * CellSize;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            var gridRect = GUILayoutUtility.GetRect(areaWidth, totalHeight);

            int index = 0;

            // "선택 없음" 셀
            DrawCell(gridRect, index, cols, null, "");
            index++;

            // 스프라이트 셀
            foreach (var sprite in _filteredSprites)
            {
                DrawCell(gridRect, index, cols, sprite, sprite.name);
                index++;
            }

            EditorGUILayout.EndScrollView();

            // 하단 버튼
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    Close();
                }
            }
            EditorGUILayout.Space(4);
        }

        private void DrawCell(Rect gridRect, int index, int cols, Sprite sprite, string label)
        {
            int col = index % cols;
            int row = index / cols;

            var cellRect = new Rect(
                gridRect.x + col * CellSize + Padding,
                gridRect.y + row * CellSize + Padding,
                CellSize - Padding * 2f,
                CellSize - Padding * 2f
            );

            bool isSelected = (sprite == null && string.IsNullOrEmpty(_currentSpriteName))
                              || (sprite != null && sprite.name == _currentSpriteName);

            // 선택 배경
            if (isSelected)
                GUI.Box(cellRect, GUIContent.none, _selectedStyle);

            // 썸네일
            var thumbRect = new Rect(cellRect.x, cellRect.y, cellRect.width, ThumbnailSize);
            if (sprite != null)
            {
                var preview = AssetPreview.GetAssetPreview(sprite);
                if (preview != null)
                    GUI.DrawTexture(thumbRect, preview, ScaleMode.ScaleToFit);
                else
                    EditorGUI.LabelField(thumbRect, "...", _cellStyle);
            }
            else
            {
                // 선택 없음 셀
                var iconRect = new Rect(thumbRect.x + thumbRect.width / 2f - 16f, thumbRect.y + thumbRect.height / 2f - 16f, 32f, 32f);
                EditorGUI.LabelField(iconRect, "∅", new GUIStyle(EditorStyles.boldLabel) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
            }

            // 라벨
            var labelRect = new Rect(cellRect.x, cellRect.y + ThumbnailSize, cellRect.width, LabelHeight);
            GUI.Label(labelRect, string.IsNullOrEmpty(label) ? "-" : label, _cellStyle);

            // 클릭 처리
            if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
            {
                _currentSpriteName = label;
                _onChange?.Invoke(label);
                Repaint();

                // 더블 클릭 시 창 닫기
                if (Event.current.clickCount == 2)
                    Close();

                Event.current.Use();
            }

            // 호버 효과
            if (Event.current.type == EventType.MouseMove && cellRect.Contains(Event.current.mousePosition))
                Repaint();
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            var tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
