using System;
using System.Collections.Generic;
using FernGraph;
using FernGraph.Editor;
using FernNPRCore.StableDiffusionGraph;
using UnityEditor;
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
        private List<List<string>> config_positive = new List<List<string>>();
        private List<List<string>> config_negative = new List<List<string>>();
        private Dictionary<string, string> CN2EN = new Dictionary<string, string>();
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
            
            TextAsset danbooru_Asset_tag = Resources.Load<TextAsset>("SDTag/danbooru");
            
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
            
            var button = new Button(RefreshPrompt);
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

        Vector2 s1;
        Vector2 s2;
        Vector2 s3;
        List<bool> foldouts = new List<bool>();
        public List<string> SearchWords(string searchingText, List<List<string>> config)
        {
            var result = new List<string>();
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
                        result.Add(line[i]);
                    }
                }
            }
            return result;
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
                    EditorStyles.label.Draw(position1, $"{searchModeMenus[0]}" , false, false, false, false);
                    EditorStyles.label.fontSize = fontSize;
                }
            }
			
            return isHasChanged;
        }

        void DrawWord(List<string> Words, List<float> Weights, List<int> Colors, string word)
        {
            var index = Words.IndexOf(word);
            var select = index != -1;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!select);
            EditorGUI.EndDisabledGroup();
            var color = GUI.color;
            GUI.color = select ? colorConfig[0].SkinColor() : color;
            if (GUILayout.Button(TryGetENWord(word)))
            {
                if (select)
                {
                    Words.RemoveAt(index);
                    Weights.RemoveAt(index);
                    Colors.RemoveAt(index);
                }
                else
                {
                    Words.Add(word);
                    Weights.Add(0);
                    Colors.Add(0);
                    SetCurrentWord(word);
                }
            }
            GUI.color = color;
            EditorGUI.BeginDisabledGroup(!select);
            EditorGUI.EndDisabledGroup();
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

        void ClearWords(List<string> Words, List<float> Weights, List<int> Colors)
        {
            Words.Clear();
            Weights.Clear();
            Colors.Clear();
        }
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
            var Words = toolBarIndex == 0 ? register.PositiveWords : register.NegativeWords;
            var Weights = toolBarIndex == 0 ? register.PositiveWordsWeights : register.NegativeWordsWeights;
            var Colors = toolBarIndex == 0 ? register.PositiveWordsColors : register.NegativeWordsColors;
            
            
            if (Words.Count != Weights.Count)
            {
                Weights.Clear();
                for (int i = 0; i < Words.Count; i++)
                {
                    Weights.Add(0);
                }
            }
            if (Words.Count != Colors.Count)
            {
                Colors.Clear();
                for (int i = 0; i < Words.Count; i++)
                {
                    Colors.Add(0);
                }
            }
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();
            
            
            // ----------------------------------------------DrawSearchField---------------------------------------
            EditorGUILayout.BeginHorizontal();
            DrawSearchField(ref searchingText, modeMenus, menusIndex, (o, strings, arg3) =>
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
                var words = SearchWords(searchingText, config);
                foreach (var word in words)
                {
                    DrawWord(Words, Weights, Colors, word);
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
                            DrawWord(Words, Weights, Colors, line[j]);
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
            DrawPromptWords(0, Words, Weights, Colors);
            EditorGUILayout.EndScrollView();
            var currentWord = toolBarIndex == 0 ? currentPositiveWord : currentNegativeWord;
            
            EditorGUILayout.BeginHorizontal();
            var index = Words.IndexOf(currentWord);
            var select = index != -1;

            var showCurrentWord = !string.IsNullOrEmpty(currentWord) && select;
            EditorGUILayout.LabelField(showCurrentWord ? TryGetENWord(currentWord) : "no selected word");

            if (GUILayout.Button(_iconDiscard, GUILayout.Width(26)))
                ClearWords(Words, Weights, Colors);
            
                    
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            // ------------------------------------------------------------------------------------------------
            
            
            // ----------------------------------------------3 ---------------------------------------
            if (showCurrentWord)
            {
                EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(80));
                EditorGUILayout.BeginHorizontal("helpbox");
                var color = GUI.color;
                GUI.color = Colors[index] > 0 ? colorConfig[Colors[index]].SkinColor() : color;
                EditorGUILayout.LabelField(TryGetENWord(currentWord));
                GUI.color = color;

                if (GUILayout.Button(_iconDiscard, GUILayout.Width(26)))
                {
                    Words.RemoveAt(index);
                    Weights.RemoveAt(index);
                    Colors.RemoveAt(index);
                    SetCurrentWord(null);
                    index = -1;
                }
                EditorGUILayout.EndHorizontal();
                if (index != -1)
                {
                    s3 = EditorGUILayout.BeginScrollView(s3, GUILayout.Height(268));
                    DrawColorBar(0, index, Colors); 
                    EditorGUILayout.BeginHorizontal("helpbox");
                    EditorGUILayout.LabelField("Weight");
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.BeginChangeCheck();
                    var Weight = EditorGUILayout.Slider(Weights[index], -0.6f, 0.6f);
                    if (EditorGUI.EndChangeCheck())
                        Weights[index] = Weight;
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
                register.Prompt.positive = RegisterPrompt(register.PositiveWords, register.PositiveWordsWeights, register.PositiveWordsColors);
                register.Prompt.negative = RegisterPrompt(register.NegativeWords, register.NegativeWordsWeights, register.NegativeWordsColors);
            }
        }
        private void DrawColorBar(int startIndex, int wordIdx, List<int> colors)
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
                    colors[wordIdx] = i;
                }
            }
            GUI.color = color;
            EditorGUILayout.EndHorizontal();
            if (nextIndex < colorConfigStr.Length)
            {
                DrawColorBar(nextIndex, wordIdx, colors);
            }
        }
        public string RegisterPrompt(List<string> Words, List<float> Weights, List<int> Colors)
        {
            var Positive = "";
            var count = Words.Count;
            for (var i = 0; i < count; i++)
            {
                var word = Words[i];
                if (!CN2EN.TryGetValue(word, out var en_word)) continue;
                var weight = Weights[i];

                var useWeight = Math.Abs(weight) > 0.01f;

                if (useWeight)
                    Positive += "(";
                var col_idx = Colors[i];
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
        
        private string currentPositiveWord;
        private string currentNegativeWord;
        private void DrawPromptWords(int startIndex, List<string> Words, List<float> Weights, List<int> Colors)
        {
            var wordLength = 0;
            var nextIndex = -1;
            var len = Words.Count;
            if (len <= startIndex) return;
            EditorGUILayout.BeginHorizontal();
            for (var i = startIndex; i < len; i++)
            {
                var word = Words[i];
                var weight = Weights[i];
                var realWord = TryGetENWord(word) + (weight > 0 ? $"[{weight}]":"");
                wordLength += realWord.Length + 1;
                if (wordLength > (showInEn ? 44 : 22))
                {
                    nextIndex = i;
                    break;
                }
                var color = GUI.color;
                var colorIndex = Colors[i];
                GUI.color = colorIndex > 0 ? colorConfig[colorIndex].SkinColor() : color;
                if (GUILayout.Button(realWord))
                {
                    SetCurrentWord(word);
                }
                GUI.color = color;
            }
            EditorGUILayout.EndHorizontal();
            if (nextIndex != -1 && nextIndex < len)
            {
                DrawPromptWords(nextIndex, Words, Weights, Colors);
            }
        }

        public void SetCurrentWord(string word)
        {
            if (toolBarIndex == 0)
                currentPositiveWord = word;
            else
                currentNegativeWord = word;
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