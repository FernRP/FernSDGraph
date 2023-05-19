using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FernGraph;
using FernGraph.Editor;
using FernNPRCore.StableDiffusionGraph;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernRender.FernNPRCore.StableDiffusionGraph.SDGraph.Editor
{
    [CustomNodeView(typeof(SDPromptRegisterNode))]
    public class SDPromptRegisterNodeView : NodeView
    {

        public static readonly GUIContent
            _iconAdd = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Add"),
            _iconMinus = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Minus").image, "Minus"),
            _iconEdit = new GUIContent(EditorGUIUtility.IconContent("editicon.sml").image, "Edit"),
            _iconSave = new GUIContent(EditorGUIUtility.IconContent("SaveActive").image, "Save"),
            _favorite_on = new GUIContent(EditorGUIUtility.IconContent("Favorite Icon").image, "Favorite"),
            _forward = new GUIContent(EditorGUIUtility.IconContent("forward").image, "forward");
        private List<List<string>> config_positive;
        private List<List<string>> config_negative;
        private Dictionary<string, string> CN2EN;
        private List<TagData> tags;
        private SDPromptRegisterNode cur_register;
        private PromptFavoriteData favorite_Asset;
        
        
        public struct TagData
        {
            public string tag;
            public int col;
            public int priority;
            public string other;
        }

        public SDPromptRegisterNodeView()
        {
            config_positive = new List<List<string>>();
            config_negative = new List<List<string>>();
            searchwords = new List<TagData>();
            CN2EN = new Dictionary<string, string>();
            tags = new List<TagData>();
            promptDataRects = new List<Rect>();
        }
        public void resolve_csv(TextAsset csv, ref List<List<string>> csv_temp)
        {
            var csv_lines = csv.text.Split("\n");
            foreach (var line in csv_lines)
            {
                var words = line.Split(",");
                List<string> lineItems = new List<string>();
                var temp_str = "";
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    word = word.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    if (string.IsNullOrEmpty(word)) continue;
                    if (word.StartsWith("\""))
                    {
                        temp_str = word.Replace("\"", "");
                        continue;
                    }

                    if (string.IsNullOrEmpty(temp_str))
                    {
                        lineItems.Add(word);
                        continue;
                    }
                    
                    if (!word.EndsWith("\""))
                    {
                        temp_str += "," + word;
                        continue;
                    }
                    
                    temp_str += "," + word.Replace("\"", "");
                    lineItems.Add(temp_str);
                    temp_str = "";
                }
                if (lineItems.Count <= 0)continue;
                csv_temp.Add(lineItems);
            }
        }

        public void resolve_tag(List<List<string>> tag)
        {
            tags.Clear();
            foreach (var list in tag)
            {
                if (list.Count < 3) continue;
                var other = list.Count < 4 ? "" : list[3];
                tags.Add(new TagData()
                {
                    tag = list[0],
                    col = int.Parse(list[1]),
                    priority = int.Parse(list[2]),
                    other = other,
                });
            }
            tags.Sort((a, b) => b.priority.CompareTo(a.priority));
        }
        public void LoadConfigTxt()
        {
            favorite_Asset = Resources.Load<PromptFavoriteData>("SDTag/favorite");
            TextAsset config_positive_Asset = Resources.Load<TextAsset>("SDTag/config_positive_novelai");
            var config_positive_Text = config_positive_Asset.text.Split("\n");
            config_positive.Clear();
            foreach (var line in config_positive_Text)
            {
                var words = line.Split("\t");
                List<string> lineItems = new List<string>();
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    word = word.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    if (string.IsNullOrEmpty(word)) continue;
                    lineItems.Add(word);
                }

                if (lineItems.Count > 0)
                {
                    config_positive.Add(lineItems);
                }
            }
            TextAsset danbooru_tag_Asset = Resources.Load<TextAsset>("SDTag/danbooru");
            TextAsset e621_tag_Asset = Resources.Load<TextAsset>("SDTag/e621");
            var tagList = new List<List<string>>();
            resolve_csv(danbooru_tag_Asset, ref tagList);
            resolve_csv(e621_tag_Asset, ref tagList);
            resolve_tag(tagList);
            
            TextAsset config_negative_Asset = Resources.Load<TextAsset>("SDTag/config_negative_common");
            var config_negative_Text = config_negative_Asset.text.Split("\n");
            config_negative.Clear();
            foreach (var line in config_negative_Text)
            {
                var words = line.Split("\t");
                List<string> lineItems = new List<string>();
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    word = word.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    if (string.IsNullOrEmpty(word)) continue;
                    lineItems.Add(word);
                }

                if (lineItems.Count > 0)
                {
                    config_negative.Add(lineItems);
                }
            }
            
            TextAsset CN2ENAsset = Resources.Load<TextAsset>("SDTag/CN2EN");
            var CN2ENtext = CN2ENAsset.text.Split("\n");
            CN2EN.Clear();
            foreach (var line in CN2ENtext)
            {
                var words = line.Split("\t");
                List<string> lineItems = new List<string>();
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    word = word.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    if (string.IsNullOrEmpty(word)) continue;
                    lineItems.Add(word);
                }

                if (lineItems.Count > 1)
                {
                    if (CN2EN.ContainsKey(lineItems[0]))
                    {
                        continue;
                    }
                    CN2EN.Add(lineItems[0], lineItems[1]);
                }
            }
        }

        protected override void OnInitialize()
        {
            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            
            var button = new Button(RefreshPrompt); // TODO: DraggableButton For Test, Should use Button(RefreshPrompt).
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.FlexEnd;
            button.style.bottom = 0;
            button.style.right = 0;
            titleButtonContainer.Add(button);
           
            LoadConfigTxt();
            RefreshExpandedState();
        }

        private void RefreshPrompt()
        {
            LoadConfigTxt();
        }
        
        
        Rect cur;
        static int hotControl;
        static Vector2 downPosition;

        private void GenerateDrag(Rect src, 
            string dragTitle = "",
            Action<Vector2> onStartDrag = null, 
            Action<Vector2> onDragExited = null, 
            Action<Vector2> onDragUpdated = null)
        {
            Event e = Event.current;
            var cid = GUIUtility.GetControlID(FocusType.Passive);
            var in_click_area = src.Contains(e.mousePosition);
            
            switch (e.GetTypeForControl(cid))
            {
                case EventType.MouseDown:
                    if (in_click_area)
                    {
                        // GUIUtility.hotControl ??? 
                        hotControl = cid;
                        downPosition = e.mousePosition;
                    }
                    break;
                case EventType.MouseUp:
                    if (hotControl == cid)
                        hotControl = 0;
                    break;
                case EventType.MouseDrag:
                    if (hotControl == cid && in_click_area)
                    {
                        if ((downPosition - e.mousePosition).magnitude > 1f)
                        {
                            onStartDrag?.Invoke(e.mousePosition);
                            DragAndDrop.PrepareStartDrag();
                            // DragAndDrop.SetGenericData
                            DragAndDrop.StartDrag(dragTitle);
                            cur = src;
                            e.Use();
                        }
                    }
                    break;
                case EventType.DragUpdated:
                    cur.position = e.mousePosition - cur.size / 2.0f;
                    onDragUpdated?.Invoke(e.mousePosition);
                    e.Use();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    // DragAndDrop.GetGenericData
                    e.Use();
                    break;
                case EventType.DragExited:
                    hotControl = 0;
                    onDragExited?.Invoke(e.mousePosition);
                    e.Use();
                    break;
            }
        }

        #region DrawWord

        
        void DrawWord(List<PromptData> promptDatas, TagData word)
        {
            EditorGUILayout.BeginHorizontal();
            var color = GUI.color;
            var enWord = TryGetENWord(word.tag);
            var showWord = enWord;
            GetShowWord(ref showWord, 18);
            if (word.col != -1)
            {
                GUI.color = (word.col < 7 ? colorConfigSearch[word.col] : colorConfigSearch[0]);
            }
            if (GUILayout.Button(new GUIContent(showWord, enWord)))
            {
                SetCurrentWord(promptDatas.Count);
                RegisterUndo("prompt Add");
                promptDatas.Add(new PromptData()
                {
                    word = word.tag,
                });
                refresh = true;

            }
            GUI.color = color;
            EditorGUILayout.EndHorizontal();
        }
        void DrawWord(List<PromptData> promptDatas, string word)
        {
            EditorGUILayout.BeginHorizontal();
            var color = GUI.color;
            var enWord = TryGetENWord(word);
            var showWord = enWord;
            GetShowWord(ref showWord, 18);
            if (GUILayout.Button(new GUIContent(showWord, enWord)))
            {
                SetCurrentWord(promptDatas.Count);
                RegisterUndo("prompt Add");
                promptDatas.Add(new PromptData()
                {
                    word = word,
                });
                refresh = true;

            }
            GUI.color = color;
            EditorGUILayout.EndHorizontal();
        }

        private string TryGetENWord(string word)
        {
            if (CN2EN.ContainsKey(word))
            {
                return showInEn ? CN2EN[word] : word;
            }
            return word;
        }


        #endregion
        
        #region SearchWords

        
        Vector2 s1;
        Vector2 s2;
        Vector2 s3;
        Vector2 s4;
        List<bool> foldouts = new List<bool>();
        private int step = 0;
        private int maxSearchWord = 100;
        private EditorCoroutine search_coroutine;
        public IEnumerator SearchWords(int menusIndex, string searchingText, List<List<string>> config)
        {
            searchwords.Clear();
            searchwords.Add(new TagData()
            {
                tag = searchingText,
                col = -1,
            });
            if (menusIndex == 0 || menusIndex > config.Count)
            {
                foreach (var line in config)
                {
                    for (var i = 1; i < line.Count; i++)
                    {
                        bool contains = true;
                        // whole word match search
                        var cn = line[i].ToLower();
                        var en = cn;
                        if (CN2EN.TryGetValue(cn, out var temp))
                            en = temp;
                    
                        searchingText = searchingText.ToLower();
                        var keywords = searchingText.Split(' ', ',', ';', '|', '*', '&');// Some possible separators
                        foreach (var keyword in keywords)
                        {
                            var isMatch = false;
                            isMatch |= cn.Contains(keyword);
                            isMatch |= en.Contains(keyword);
                            contains &= isMatch;
                        }
                        if (contains)
                        {
                            searchwords.Add(new TagData()
                            {
                                tag = line[i],
                                col = -1,
                            });
                            if (searchwords.Count >= maxSearchWord) break;
                            step++;
                        }
                        if (step > 20)
                        {
                            step = 0;
                            yield return null;
                        }
                    }
                }
                foreach (var tagData in tags)
                {
                    bool contains = true;
                    // whole word match search
                    var en = tagData.tag.ToLower();
                    searchingText = searchingText.ToLower();
                    var keywords = searchingText.Split(' ', ',', ';', '|', '*', '&');// Some possible separators
                    foreach (var keyword in keywords)
                    {
                        var isMatch = false;
                        isMatch |= en.Contains(keyword);
                        contains &= isMatch;
                    }
                    if (contains)
                    {
                        searchwords.Add(tagData);
                        if (searchwords.Count >= maxSearchWord) break;
                        step++;
                    }
                    if (step > 20)
                    {
                        step = 0;
                        yield return null;
                    }
                }
            }
            else
            {
                var line = config[menusIndex - 1];
                for (var i = 1; i < line.Count; i++)
                {
                    bool contains = true;
                    // whole word match search
                    var cn = line[i].ToLower();
                    var en = cn;
                    if (CN2EN.TryGetValue(cn, out var temp))
                        en = temp;
                    
                    searchingText = searchingText.ToLower();
                    var keywords = searchingText.Split(' ', ',', ';', '|', '*', '&');// Some possible separators
                    foreach (var keyword in keywords)
                    {
                        var isMatch = false;
                        isMatch |= cn.Contains(keyword);
                        isMatch |= en.Contains(keyword);
                        contains &= isMatch;
                    }
                    if (contains)
                    {
                        searchwords.Add(new TagData()
                        {
                            tag = line[i],
                            col = -1,
                        });
                        if (searchwords.Count >= maxSearchWord) break;
                        step++;
                    }
                    if (step > 20)
                    {
                        step = 0;
                        yield return null;
                    }
                }
            }
        }
        public string searchingText;
        public static bool DrawSearchField(ref string searchingText, GUIContent[] searchModeMenus, int selectedMenusIndex = 0, Action<object, string[], int> menuCallback = null) // 
        {
            var toolbarSeachTextFieldPopup = new GUIStyle("ToolbarSeachTextFieldPopup");

            bool isHasChanged = false;
            EditorGUI.BeginChangeCheck();
			
            var rect = EditorGUILayout.GetControlRect();
            // searching mode
            var modeRect = new Rect(rect){width = 20f};
            if (Event.current.type == EventType.MouseDown && modeRect.Contains(Event.current.mousePosition))
            {
                EditorUtility.DisplayCustomMenu(rect, searchModeMenus, selectedMenusIndex, 
                    (data, options, selected) =>
                    {
                        // selected
                        menuCallback?.Invoke(data, options, selected);
                    }, null);
                Event.current.Use();
            }
            searchingText = EditorGUI.TextField(rect, String.Empty, searchingText, toolbarSeachTextFieldPopup);

			
            if (EditorGUI.EndChangeCheck())
                isHasChanged = true;

            
            // display search mode
            if (string.IsNullOrEmpty(searchingText) && Event.current.type == EventType.Repaint)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    Rect position1 = toolbarSeachTextFieldPopup.padding.Remove(new Rect(rect.x, rect.y, rect.width, toolbarSeachTextFieldPopup.fixedHeight > 0.0 ? toolbarSeachTextFieldPopup.fixedHeight : rect.height));
                    int fontSize = EditorStyles.label.fontSize;
                    EditorStyles.label.fontSize = toolbarSeachTextFieldPopup.fontSize;
                    EditorStyles.label.Draw(position1, $"{searchModeMenus[selectedMenusIndex]} (提示词仅支持英文输入)" , false, false, false, false);
                    EditorStyles.label.fontSize = fontSize;
                }
            }
			
            return isHasChanged;
        }

        private bool refresh = true;
    

        #endregion
        private bool showInEn = false;
        private PromptRegisterData register_temp = new PromptRegisterData();
        private bool showFavorite = false;
        private int toolBarIndex = 0;
        private int favoriteIndex = -1;
        private List<TagData> searchwords;
        private string[] toolBarOptions = {"Positive Prompt", "Negative Prompt"};
        private string[] colorConfigStr = {"红","棕","橙","黄","緑","蓝","紫","粉","黑","灰","白","金","银","透明"};
        public static Color[] colorConfig =
        {
            new Color(1.0f, 0.3f, 0.3f),
            new Color(0.65f, 0.49f, 0.24f),
            new Color(1.0f, 0.5f, 0f),
            new Color(1.0f, 1.0f, 0.3f),
            new Color(0.3f, 1.0f, 0.3f),
            new Color(0.3f, 0.7f, 1.0f),
            new Color(0.7f, 0.3f, 1.0f),
            new Color(1.0f, 0.4f, 0.7f),
            new Color(0.1f, 0.1f, 0.1f),
            Color.gray, 
            new Color(1.0f, 1.0f, 1.0f),
            new Color(1.0f, 0.95f, 0.6f),
            new Color(0.74f, 0.78f, 0.8f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f),
        };
        public static Color[] colorConfigSearch =
        {
            new Color(0.3f, 0.7f, 1.0f),
            new Color(1.0f, 0.3f, 0.3f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f),
            new Color(0.7f, 0.3f, 1.0f),
            new Color(0.1f, 0.6f, 0.1f),
            new Color(1.0f, 0.5f, 0f),
            new Color(0.65f, 0.49f, 0.24f),
        };

        void OnGUI()
        {
            if(Target is not SDPromptRegisterNode register) return;
            cur_register = register;
            var config = toolBarIndex == 0 ? config_positive : config_negative;
            var len = config.Count;
            var modeMenus = new GUIContent[len + 1];
            modeMenus[0] = new GUIContent("all");
            for (var i = 0; i < len; i++)
            {
                var line = config[i];
                if (line.Count > 0)
                {
                    modeMenus[i + 1] = new GUIContent(line[0]);
                }
            }

            var registerRegisterData = register.RegisterData;
            var promptDatas = toolBarIndex == 0 ? registerRegisterData.positiveDatas : registerRegisterData.negativeDatas;
            var menusIndex = toolBarIndex == 0 ? positiveMenusIndex : negativeMenusIndex;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            // ----------------------------------------------DrawSearchField---------------------------------------
            EditorGUILayout.BeginHorizontal();
            var DoSearch = DrawSearchField(ref searchingText, modeMenus, menusIndex, (o, strings, arg3) =>
            {
                SetMenusIndex(arg3);
            });
            if (GUILayout.Button(showInEn ? "EN" : "CN", GUILayout.Width(30)))
                showInEn = !showInEn;
            
            EditorGUILayout.EndHorizontal();
            // ------------------------------------------------------------------------------------------------
            
            
            // ----------------------------------------------draw Prompt---------------------------------------
            EditorGUILayout.BeginHorizontal();
            
            // ----------------------------------------------1 ---------------------------------------
            EditorGUILayout.BeginVertical(GUILayout.Width(140));
            s1 = EditorGUILayout.BeginScrollView(s1, GUILayout.Height(300));
            if (!string.IsNullOrEmpty(searchingText))
            {
                if (DoSearch)
                {
                    step = 0;
                    if (search_coroutine != null)
                    {
                        EditorCoroutineUtility.StopCoroutine(search_coroutine);
                        search_coroutine = null;
                    }
                    search_coroutine = EditorCoroutineUtility.StartCoroutine(SearchWords(menusIndex, searchingText, config), this);
                }
                
                if (searchwords is { Count: > 0 })
                {
                    foreach (var word in searchwords)
                    {
                        DrawWord(promptDatas, word);
                    }
                }
            }
            else
            {
                while (foldouts.Count < config.Count)
                {
                    foldouts.Add(false);
                }
                for (var i = 0; i < config.Count; i++)
                {
                    if (menusIndex != 0 && menusIndex != i + 1) continue;
                    var line = config[i];
                    EditorGUILayout.BeginVertical("helpbox");
                    foldouts[i] = EditorGUILayout.Foldout(foldouts[i], line[0], true);
                    if (foldouts[i])
                    {
                        for (var j = 1; j < line.Count; j++)
                        {
                            DrawWord(promptDatas, line[j]);
                        }
                    }
                    EditorGUILayout.EndVertical();

                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // ------------------------------------------------------------------------------------------------
            
            
            // ----------------------------------------------2 ---------------------------------------
            var current_idx = toolBarIndex == 0 ? cur_positive_idx : cur_negative_idx;
            EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(300));
            EditorGUI.BeginChangeCheck();
            var title_t = EditorGUILayout.TextArea(registerRegisterData.title.tooltip);
            if (EditorGUI.EndChangeCheck())
            {
                registerRegisterData.title.tooltip = title_t;
                GetShowWord(ref title_t, 18);
                registerRegisterData.title.text = title_t;
            }
            toolBarIndex = GUILayout.Toolbar(toolBarIndex, toolBarOptions);
            DrawSplitter();
            s2 = EditorGUILayout.BeginScrollView(s2, GUILayout.Height(230));
            promptDataRects.Clear();
            DrawPromptWords(current_idx, 0, promptDatas);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            var showCurrentWord = current_idx != -1 && current_idx < promptDatas.Count;
            EditorGUILayout.LabelField(showCurrentWord ? TryGetENWord(promptDatas[current_idx].word) : "no selected word");
            if (GUILayout.Button(_favorite_on, GUILayout.Width(30), GUILayout.Height(20)))
                showFavorite = !showFavorite;
            
            if (GUILayout.Button("clear", GUILayout.Width(40), GUILayout.Height(20)))
            {
                RegisterUndo("prompt Discard");
                promptDatas.Clear();
                SetCurrentWord(-1);
                current_idx = -1;
                showCurrentWord = false;
                refresh = true;
            }
            
                    
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            // ------------------------------------------------------------------------------------------------
            
            
            // ----------------------------------------------3 ---------------------------------------
            if (showCurrentWord)
            {
                EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(160));
                EditorGUILayout.BeginHorizontal(GUILayout.Width(160));
                // var color = GUI.color;
                // var color_index = promptDatas[current_idx].color;
                // GUI.color = color_index > 0 ? colorConfig[color_index].SkinColor() : color;
                EditorGUILayout.LabelField(TryGetENWord(promptDatas[current_idx].word), GUILayout.Width(140));
                // GUI.color = color;

                if (GUILayout.Button(_iconMinus, GUILayout.Width(20)))
                {
                    RegisterUndo("prompt Remove");
                    promptDatas.RemoveAt(current_idx);
                    refresh = true;
                    SetCurrentWord(-1);
                    current_idx = -1;
                }
                EditorGUILayout.EndHorizontal();
                
                DrawSplitter();
                if (current_idx != -1)
                {
                    s4 = EditorGUILayout.BeginScrollView(s4, GUILayout.Height(270));
                    DrawColorBar(0, current_idx, promptDatas);
                    GUILayout.Space(4);
                    DrawSplitter();
                    GUILayout.Space(4);
                    EditorGUILayout.LabelField("Weight", new GUIStyle("helpbox"), GUILayout.Width(160));
                    EditorGUI.BeginChangeCheck();
                    var Weight = EditorGUILayout.Slider(promptDatas[current_idx].weight, -0.6f, 0.6f, GUILayout.Width(160));
                    if (EditorGUI.EndChangeCheck())
                    {
                        RegisterUndo("prompt SetWeight");
                        promptDatas[current_idx] = promptDatas[current_idx].SetWeight(Weight);
                        refresh = true;
                    }
                    GUILayout.Space(4);
                    DrawSplitter();
                    GUILayout.Space(4);
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(160));
                    EditorGUILayout.LabelField("Process", new GUIStyle("helpbox"), GUILayout.Width(136));
                    if (GUILayout.Button(promptDatas[current_idx].end ? "E" : "S", GUILayout.Width(20)))
                    {
                        RegisterUndo("prompt SetProcessType");
                        promptDatas[current_idx] = promptDatas[current_idx].SetProcessType(!promptDatas[current_idx].end);
                    }
                        
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.BeginChangeCheck();
                    var Process = EditorGUILayout.Slider(promptDatas[current_idx].process, 0, 1, GUILayout.Width(160));
                    if (EditorGUI.EndChangeCheck())
                    {
                        RegisterUndo("prompt SetProcess");
                        promptDatas[current_idx] = promptDatas[current_idx].SetProcess(Process);
                        refresh = true;
                    }
                    EditorGUILayout.EndScrollView();
                }
                
                EditorGUILayout.EndVertical();
                
            }
            // ------------------------------------------------------------------------------------------------
            
            // ----------------------------------------------4 ---------------------------------------
            if (showFavorite)
            {
                EditorGUILayout.BeginVertical("helpbox");
                s3 = EditorGUILayout.BeginScrollView(s3, GUILayout.Width(140), GUILayout.Height(270));


                var guiContent = new GUIContent((favoriteIndex == -1 ? registerRegisterData.title : register_temp.title));
                guiContent.text += "*";
                var old = GUI.color;
                var select_col = new Color(0.45f, 0.65f, 1.0f);
                select_col *= 1.5f;
                GUI.color = favoriteIndex == -1 ? select_col : old;

                if (GUILayout.Button(guiContent))
                {
                    favoriteIndex = -1;
                    register_temp.CopyTo(registerRegisterData);
                }
                if (favorite_Asset)
                {
                    for (var i = 0; i < favorite_Asset.FavoriteData.Count; i++)
                    {
                        guiContent = new GUIContent(favorite_Asset.FavoriteData[i].title);
                        GUI.color = favoriteIndex == i ? select_col : old;
                        if (!GUILayout.Button(guiContent)) continue;
                        if (favoriteIndex == -1)
                            register.RegisterData.CopyTo(register_temp);
                            
                        favoriteIndex = i;
                        favorite_Asset.FavoriteData[i].CopyTo(register.RegisterData);
                    }
                }

                GUI.color = old;
                
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                GUILayout.Space(10);
                if (GUILayout.Button(_iconAdd, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    var registerData = new PromptRegisterData();
                    registerRegisterData.CopyTo(registerData);
                    favorite_Asset.FavoriteData.Add(registerData);
                    EditorUtility.SetDirty(favorite_Asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                EditorGUI.BeginDisabledGroup(favoriteIndex == -1);
                if (GUILayout.Button(_iconEdit, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    favoriteIndex = -1;
                    registerRegisterData.CopyTo(register_temp);
                }
                if (GUILayout.Button(_iconSave, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    registerRegisterData.CopyTo(favorite_Asset.FavoriteData[favoriteIndex]);
                    EditorUtility.SetDirty(favorite_Asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if (GUILayout.Button(_iconMinus, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    favorite_Asset.FavoriteData.RemoveAt(favoriteIndex);
                    EditorUtility.SetDirty(favorite_Asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    favoriteIndex--;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            // ----------------------------------------------draw Prompt end---------------------------------------
            
            EditorGUILayout.EndHorizontal();
            
            // -------------------------------------------show Prompt-----------------------------------------
            var positive = register.Prompt.positive;
            var negative = register.Prompt.negative;
            if (!string.IsNullOrEmpty(positive) || !string.IsNullOrEmpty(register.Prompt.negative))
            {
                EditorGUILayout.BeginHorizontal();
                var styleTextArea = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                EditorGUILayout.TextArea(
                    positive, 
                    styleTextArea,
                    GUILayout.MaxHeight(60),
                    GUILayout.MinWidth(120),
                    GUILayout.ExpandHeight(true)
                );
                EditorGUILayout.TextArea(
                    negative, 
                    styleTextArea,
                    GUILayout.MaxHeight(60),
                    GUILayout.MinWidth(120),
                    GUILayout.ExpandHeight(true)
                );
                EditorGUILayout.EndHorizontal();
            }
            // ------------------------------------------------------------------------------------------------
            
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                
                register.Prompt.positive = RegisterPrompt(registerRegisterData.positiveDatas);
                register.Prompt.negative = RegisterPrompt(registerRegisterData.negativeDatas);
            }
        }

        void GetShowWord(ref string word, int width)
        {
            var lastIndex = word.Length;
            for (var i = 0; i < word.Length; i++)
            {
                if (word[i] > 127)
                {
                    width -= 2;
                    if (width > 0) continue;
                    lastIndex = width < - 1 ? i - 2 : i - 1;
                    break;
                }

                width -= 1;
                if (width > 0) continue;
                lastIndex = i - 3;
                break;
            }
            word = lastIndex < word.Length ? word.Substring(0, lastIndex) + "..." : word;
        }
        private void DrawColorBar(int startIndex, int wordIdx, List<PromptData> promptDatas)
        {
            EditorGUILayout.BeginHorizontal();
            var color = GUI.color;
            var nextIndex = Mathf.Min(startIndex + 7, colorConfigStr.Length);
            for (var i = startIndex; i < nextIndex; i++)
            {
                var prompt_col = promptDatas[wordIdx].color;
                var promptCol = 1 << i;
                var contains_col = (prompt_col & promptCol) != 0;
                var show_col = colorConfig[i];
                show_col *= contains_col ? 1.5f : 0.35f;
                GUI.color = show_col;
                var controlRect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
                if (GUI.Button(controlRect, new GUIContent("", colorConfigStr[i])))
                {
                    if (contains_col)
                        prompt_col &= ~promptCol;
                    else
                        prompt_col |= promptCol;
                    
                    RegisterUndo("prompt SetColor");
                    promptDatas[wordIdx] = promptDatas[wordIdx].SetColor(prompt_col);
                    refresh = true;
                }
                GUI.color = color;
                controlRect.height = 2;
                controlRect.x += controlRect.width - 4;
                controlRect.width = 2;
                controlRect.y += 2;
                EditorGUI.DrawRect(controlRect, contains_col ? show_col : Color.gray);
            }
            EditorGUILayout.EndHorizontal();
            if (nextIndex < colorConfigStr.Length)
            {
                DrawColorBar(nextIndex, wordIdx, promptDatas);
            }
        }
        public string RegisterPrompt(List<PromptData> promptDatas)
        {
            var Positive = "";
            var count = promptDatas.Count;
            for (var i = 0; i < count; i++)
            {
                var promptData = promptDatas[i];
                if (!CN2EN.TryGetValue(promptData.word, out var en_word))
                {
                    if (promptData.word.Any(t => t > 127)) continue;
                    en_word = promptData.word;
                }
                var weight = promptData.weight;
                var process = promptData.process;
                var end = promptData.end;
                var useWeight = Math.Abs(weight) > 0.01f;

                // Process---------------------------------------
                if (process is > 0.01f and < 0.99f)
                {
                    Positive += "[";
                }
                
                
                // weight---------------------------------------
                if (useWeight)
                    Positive += "(";
                
                // merge color---------------------------------------
                var colorIndex = promptData.color;
                var colPrompt = "";
                var colNum = 0;
                for (var j = 0; j < colorConfig.Length; j++)
                {
                    var promptCol = 1 << j;
                    if ((colorIndex & promptCol) == 0 || !CN2EN.TryGetValue(colorConfigStr[j], out var col_str)) continue;
                    switch (colNum)
                    {
                        case 0:
                            colPrompt = col_str;
                            break;
                        case 1:
                            colPrompt = "[" + colPrompt + "|" + col_str;
                            break;
                        default:
                            colPrompt += "|" + col_str;
                            break;
                    }
                    colNum++;
                }

                if (!string.IsNullOrEmpty(colPrompt))
                {
                    Positive += colPrompt;
                    if (colNum > 1)
                        Positive += "]";
                    Positive += " ";
                    
                }
                // merge color---------------------------------------
                
                // main word---------------------------------------
                Positive += en_word;

                // weight---------------------------------------
                if (useWeight)
                {
                    Positive += $":{(1 + weight):F2}";
                    Positive += ")";
                }
                

                // Process---------------------------------------
                if (process is > 0.01f and < 0.99f)
                {
                    Positive += $"{(end ? "::" : ":")}{process:F2}]";
                }
                
                // weight---------------------------------------
                if (i != count - 1)
                    Positive += ",";
            }
            return Positive;
        }
        
        private int cur_positive_idx;
        private int cur_negative_idx;
        private int positiveMenusIndex;
        private int negativeMenusIndex;
        private int selected_idx = -1;
        private int insert_idx = -1;
        private Vector2 position;
        private List<Rect> promptDataRects;

        /// <summary>
        /// 提示词越前权重越高 :
        ///     画面质量 → 主要元素 → 细节 / 画面质量 → 风格 → 元素 → 细节 (综述 [图像质量+画风+镜头效果+光照效果+主题+构图], 主体 [人物/对象+姿势+服装+道具], 细节 [场景+环境+饰品+特征])
        /// 连接词 :
        ///     AND 初始权重一致
        ///     + | _ 融合/并列
        ///     , 对象一致时，逗号有连接的功能
        /// 起始进度 : [Prompt: float]/[Prompt: int]
        /// 结束进度 : [Prompt:: float]/[Prompt:: int]
        /// </summary>
        /// <param name="currentIdx"></param>
        /// <param name="startIndex"></param>
        /// <param name="promptDatas"></param>
        private void DrawPromptWords(int currentIdx, int startIndex, List<PromptData> promptDatas)
        {
            var wordWidth = 0f;
            var nextIndex = -1;
            var len = promptDatas.Count;
            if (len <= startIndex) return;
            EditorGUILayout.BeginHorizontal();
            for (var i = startIndex; i < len; i++)
            {
                var promptData = promptDatas[i];
                var weight = promptData.weight;
                var realWord = TryGetENWord(promptData.word) + (weight > 0 ? $"[{weight}]":"");
                var showWord = realWord;
                GetShowWord(ref showWord, 12);
                var guiContent = new GUIContent(showWord, realWord);
                var width = GUI.skin.button.CalcSize(guiContent);
                wordWidth += width.x + 4;
                if (wordWidth > 250)
                {
                    nextIndex = i;
                    break;
                }
                
                var btn_rect = GUILayoutUtility.GetRect(width.x + 2, width.y + 6);
                var bak_rect = btn_rect;

                promptDataRects.Add(btn_rect);
                var idx = i;
                GenerateDrag(btn_rect,
                    dragTitle: "sort words",
                    onStartDrag: _ => { selected_idx = idx; },
                    onDragUpdated: v2 => { position = v2; },
                    onDragExited:_=>
                    {
                        insert_idx = -1;
                        selected_idx = -1;
                    });
                var col = GUI.color;
                var show_col = GUI.color;
                if (selected_idx == i)
                    show_col.a *= 0.3f;
                GUI.color = show_col;
                btn_rect.height = 22;
                btn_rect.y += 2;
                btn_rect.width -= 2;
                if (GUI.Button(btn_rect, guiContent))
                {
                    SetCurrentWord(i);
                }
                GUI.color = col;

                var colorIndex = promptData.color;
                var colorNum = 0;
                for (var j = 0; j < colorConfig.Length; j++)
                {
                    var promptCol = 1 << j;
                    if ((colorIndex & promptCol) != 0)
                    {
                        colorNum++;
                    }
                }

                bak_rect.height = 1;
                bak_rect.x += 3;
                bak_rect.y += 5;
                bak_rect.width -= 6;
                bak_rect.width /= colorNum;
                for (var j = 0; j < colorConfig.Length; j++)
                {
                    var promptCol = 1 << j;
                    if ((colorIndex & promptCol) != 0)
                    {
                        var bak_col = colorConfig[j];
                        EditorGUI.DrawRect(bak_rect, bak_col);
                        bak_rect.x += bak_rect.width;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            if (nextIndex != -1 && nextIndex < len)
            {
                DrawPromptWords(currentIdx, nextIndex, promptDatas);
            }
            else
            {
                if (selected_idx != -1)
                {
                    var idx = -1;
                    for (var i = 0; i < promptDataRects.Count; i++)
                    {
                        if (!promptDataRects[i].Contains(position)) continue;
                        idx = i;
                        break;
                    }

                    var promptData = promptDatas[selected_idx];
                    if (idx != -1 && idx != insert_idx)
                    {
                        insert_idx = idx;
                        if (currentIdx < selected_idx && currentIdx > insert_idx)
                            SetCurrentWord(currentIdx + 1);
                        if (currentIdx > selected_idx && currentIdx <= insert_idx)
                            SetCurrentWord(currentIdx - 1);
                        if (currentIdx == selected_idx)
                            SetCurrentWord(insert_idx);
                        
                        RegisterUndo("prompt sort");
                        promptDatas.RemoveAt(selected_idx);
                        promptDatas.Insert(insert_idx, promptData);
                        selected_idx = insert_idx;
                    }

                    var guiContent = new GUIContent(promptData.word);
                    var calcSize = GUI.skin.box.CalcSize(guiContent);
                    GUI.Box(new Rect(position - calcSize/2, calcSize), guiContent);
                }
            }
        }

        public void RegisterUndo(string tag)
        {
            // undo is to slow
            // Undo.RecordObject(cur_register.RegisterData, tag);
        }
        public void SetCurrentWord(int idx)
        {
            if (toolBarIndex == 0)
                cur_positive_idx = idx;
            else
                cur_negative_idx = idx;
        }
        public void SetMenusIndex(int idx)
        {
            if (toolBarIndex == 0) positiveMenusIndex = idx;
            else negativeMenusIndex = idx;
        }
        public static void DrawSplitter(bool isBoxed = false, bool isFullWidth = false, float width = 1f, float height = 1f)
        {
            var rect = GUILayoutUtility.GetRect(width, height);
            float xMin = rect.xMin;
            
            // Splitter rect should be full-width
            if (isFullWidth)
            {
                rect.xMin = 0f;
                rect.width += 4f;
            }

            if (isBoxed)
            {
                rect.xMin = xMin == 7.0 ? 4.0f : EditorGUIUtility.singleLineHeight;
                rect.width -= 1;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, !EditorGUIUtility.isProSkin
                ? new Color(0.6f, 0.6f, 0.6f, 1.333f)
                : new Color(0.12f, 0.12f, 0.12f, 1.333f));
        }
    }
}