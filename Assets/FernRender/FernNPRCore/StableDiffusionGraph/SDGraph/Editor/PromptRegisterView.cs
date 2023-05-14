using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Codice.CM.Client.Differences;
using FernGraph;
using FernGraph.Editor;
using FernNPRCore.StableDiffusionGraph;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernRender.FernNPRCore.StableDiffusionGraph.SDGraph.Editor
{
    [CustomNodeView(typeof(PromptRegister))]
    public class PromptRegisterView : NodeView
    {

        public static readonly GUIContent
            _iconAdd = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Add"),
            _iconMinus = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Minus").image, "Minus"),
            _iconEdit = new GUIContent(EditorGUIUtility.IconContent("editicon.sml").image, "Edit"),
            _iconDiscard = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh").image, "Discard"),
            _iconSave = new GUIContent(EditorGUIUtility.IconContent("SaveActive").image, "Save");
        private List<List<string>> config_positive;
        private List<List<string>> config_negative;
        private Dictionary<string, string> CN2EN;
        private List<TagData> tags;
        
        
        public struct TagData
        {
            public string tag;
            public int col;
            public int priority;
            public string other;
        }

        public PromptRegisterView()
        {
            config_positive = new List<List<string>>();
            config_negative = new List<List<string>>();
            searchwords = new List<TagData>();
            CN2EN = new Dictionary<string, string>();
            tags = new List<TagData>();
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
            
            var button = new DraggableButton(RefreshPrompt); // TODO: DraggableButton For Test, Should use Button(RefreshPrompt).
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.FlexEnd;
            button.style.bottom = 0;
            button.style.right = 0;
            // button.OnMoveAction += (evt) =>
            // {
            //     Debug.Log("On Move ");
            // };
            // button.OnMoveUpAction += (evt) =>
            // {
            //     Debug.Log("On Move Up");
            // };
            
            titleButtonContainer.Add(button);


            var label = new Label("title");
            label.style.fontSize = 20;
            label.RegisterCallback<DragPerformEvent>(evt =>
            {
                Debug.Log("DragPerformEvent " + evt.mousePosition);
            });
            label.RegisterCallback<DragEnterEvent>(evt =>
            {
                Debug.Log("DragEnterEvent " + evt.mousePosition);
            });
            label.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                Debug.Log("DragUpdatedEvent " + evt.mousePosition);
            });
            label.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0) // Left mouse button
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.StartDrag("Dragging button");
                    evt.StopPropagation();
                }
            });
            
            extensionContainer.Add(label);
           
            LoadConfigTxt();
            RefreshExpandedState();
        }

        private void RefreshPrompt(MouseUpEvent evt)
        {
            LoadConfigTxt();
        }

        Vector2 s1;
        Vector2 s2;
        Vector2 s3;
        List<bool> foldouts = new List<bool>();
        private int step = 0;
        private int maxSearchWord = 100;
        private EditorCoroutine search_coroutine;
        public IEnumerator SearchWords(string searchingText, List<List<string>> config)
        {
            searchwords.Clear();
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
        public int menusIndex;
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
    
        void DrawWord(List<PromptData> promptDatas, TagData word)
        {
            EditorGUILayout.BeginHorizontal();
            var color = GUI.color;
            var enWord = TryGetENWord(word.tag);
            var showWord = enWord;
            GetShowWord(ref showWord, 130);
            if (word.col != -1)
            {
                GUI.color = (word.col < 7 ? colorConfigSearch[word.col] : colorConfigSearch[0]).SkinColor();
            }
            if (GUILayout.Button(new GUIContent(showWord, enWord)))
            {
                SetCurrentWord(promptDatas.Count);
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
            GetShowWord(ref showWord, 130);

            if (GUILayout.Button(new GUIContent(showWord, enWord)))
            {
                SetCurrentWord(promptDatas.Count);
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

        private bool showInEn = false;
        private int toolBarIndex = 0;
        private List<TagData> searchwords;
        private string[] toolBarOptions = {"Positive Prompt", "Negative Prompt"};
        private string[] colorConfigStr = {"无","红","棕","橙","黄","緑","蓝","紫","粉","黑","灰","白","金","银","透明"};
        public static Color[] colorConfig =
        {
            new Color(0.4f, 0.67f, 0.54f),
            new Color(1.0f, 0.3f, 0.3f),
            new Color(0.65f, 0.49f, 0.24f),
            new Color(1.0f, 0.5f, 0f),
            new Color(1.0f, 1.0f, 0.3f),
            new Color(0.3f, 1.0f, 0.3f),
            new Color(0.3f, 0.7f, 1.0f),
            new Color(1.0f, 0.3f, 1.0f),
            new Color(1.0f, 0.4f, 0.7f),
            new Color(0.3f, 0.3f, 0.3f),
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
            new Color(1.0f, 0.3f, 1.0f),
            new Color(0.3f, 1.0f, 0.3f),
            new Color(1.0f, 0.5f, 0f),
            new Color(0.65f, 0.49f, 0.24f),
        };

        void OnGUI()
        {
            if(Target is not PromptRegister register) return;

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
            var promptDatas = toolBarIndex == 0 ? register.positiveDatas : register.negativeDatas;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            // ----------------------------------------------DrawSearchField---------------------------------------
            EditorGUILayout.BeginHorizontal();
            var DoSearch = DrawSearchField(ref searchingText, modeMenus, menusIndex, (o, strings, arg3) =>
            {
                menusIndex = arg3;
            });
            if (GUILayout.Button(showInEn ? "EN" : "CN", GUILayout.Width(30)))
            {
                showInEn = !showInEn;
            }
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
                    search_coroutine = EditorCoroutineUtility.StartCoroutine(SearchWords(searchingText, config), this);
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
            EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(310));
            toolBarIndex = GUILayout.Toolbar(toolBarIndex, toolBarOptions);
            DrawSplitter();
            s2 = EditorGUILayout.BeginScrollView(s2, GUILayout.Height(250));
            if (refresh)
                DrawPromptWords(0, promptDatas);
            EditorGUILayout.EndScrollView();
            var current_idx = toolBarIndex == 0 ? cur_positive_idx : cur_negative_idx;
            
            EditorGUILayout.BeginHorizontal();
            var showCurrentWord = current_idx != -1 && current_idx < promptDatas.Count;
            EditorGUILayout.LabelField(showCurrentWord ? TryGetENWord(promptDatas[current_idx].word) : "no selected word");

            if (GUILayout.Button(_iconDiscard, GUILayout.Width(26)))
            {
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
                EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(80));
                EditorGUILayout.BeginHorizontal("helpbox");
                var color = GUI.color;
                var color_index = promptDatas[current_idx].color;
                GUI.color = color_index > 0 ? colorConfig[color_index].SkinColor() : color;
                EditorGUILayout.LabelField(TryGetENWord(promptDatas[current_idx].word));
                GUI.color = color;

                if (GUILayout.Button(_iconDiscard, GUILayout.Width(26)))
                {
                    promptDatas.RemoveAt(current_idx);
                    refresh = true;
                    SetCurrentWord(-1);
                    current_idx = -1;
                }
                EditorGUILayout.EndHorizontal();
                if (current_idx != -1)
                {
                    s3 = EditorGUILayout.BeginScrollView(s3, GUILayout.Height(268));
                    DrawColorBar(0, current_idx, promptDatas); 
                    EditorGUILayout.BeginHorizontal("helpbox");
                    EditorGUILayout.LabelField("Weight");
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.BeginChangeCheck();
                    var Weight = EditorGUILayout.Slider(promptDatas[current_idx].weight, -0.6f, 0.6f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        promptDatas[current_idx].SetData(Weight);
                        refresh = true;
                    }
                    EditorGUILayout.EndScrollView();
                }
                
                EditorGUILayout.EndVertical();
                
            }
            EditorGUILayout.EndHorizontal();
            // ----------------------------------------------draw Prompt end---------------------------------------
            
            
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
                register.Prompt.positive = RegisterPrompt(register.positiveDatas);
                register.Prompt.negative = RegisterPrompt(register.negativeDatas);
            }
        }

        void GetShowWord(ref string word, ref int width)
        {
            var real_width = 0;
            var lastIndex = word.Length;
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] > 127)
                {
                    width -= 14;
                    if (width <= 0)
                    {
                        lastIndex = width < -7 ? i - 2 : i - 1;
                        break;
                    }
                    real_width += 14;
                }
                else
                {
                    width -= 7;
                    if (width <= 0)
                    {
                        lastIndex = i - 3;
                        break;
                    }
                    real_width += 7;
                }
            }
            width = real_width;
            word = lastIndex < word.Length ? word.Substring(0, lastIndex) + "..." : word;
        }
        void GetShowWord(ref string word, int width)
        {
            GetShowWord(ref word, ref width);
        }
        private void DrawColorBar(int startIndex, int wordIdx, List<PromptData> promptDatas)
        {
            EditorGUILayout.BeginHorizontal();
            var color = GUI.color;
            var nextIndex = Mathf.Min(startIndex + 9, colorConfigStr.Length);
            for (var i = startIndex; i < nextIndex; i++)
            {
                GUI.color = i > 0 ? colorConfig[i].SkinColor() : color;
                var col_s = colorConfigStr[i];
                if (GUILayout.Button(new GUIContent("", col_s), GUILayout.Width(20)))
                {
                    promptDatas[wordIdx].SetData(i);
                    refresh = true;
                }
            }
            GUI.color = color;
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

                var useWeight = Math.Abs(weight) > 0.01f;

                if (useWeight)
                    Positive += "(";
                var col_idx = promptData.color;
                if (col_idx > 0 && CN2EN.TryGetValue(colorConfigStr[col_idx], out var col_str))
                    Positive += col_str + " ";
                
                Positive += en_word;

                if (useWeight)
                {
                    Positive += $":{(1 + weight):F2}";
                    Positive += ")";
                }
                    
                if (i != count - 1)
                    Positive += ",";
            }
            return Positive;
        }
        
        private int cur_positive_idx;
        private int cur_negative_idx;
        private void DrawPromptWords(int startIndex, List<PromptData> promptDatas)
        {
            var wordWidth = 0;
            var nextIndex = -1;
            var len = promptDatas.Count;
            if (len <= startIndex) return;
            EditorGUILayout.BeginHorizontal();
            for (var i = startIndex; i < len; i++)
            {
                var promptData = promptDatas[i];
                var weight = promptData.weight;
                var realWord = TryGetENWord(promptData.word) + (weight > 0 ? $"[{weight}]":"");
                var width = 80;
                var showWord = realWord;
                GetShowWord(ref showWord, ref width);
                wordWidth += width + 4;
                if (wordWidth > 250)
                {
                    nextIndex = i;
                    break;
                }
                
                var color = GUI.color;
                var colorIndex = promptData.color;
                GUI.color = colorIndex > 0 ? colorConfig[colorIndex].SkinColor() : color;
                if (GUILayout.Button(new GUIContent(showWord, realWord)))
                {
                    SetCurrentWord(i);
                }
                GUI.color = color;
            }

            EditorGUILayout.EndHorizontal();
            if (nextIndex != -1 && nextIndex < len)
            {
                DrawPromptWords(nextIndex, promptDatas);
            }
        }

        public void SetCurrentWord(int idx)
        {
            if (toolBarIndex == 0)
                cur_positive_idx = idx;
            else
                cur_negative_idx = idx;
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