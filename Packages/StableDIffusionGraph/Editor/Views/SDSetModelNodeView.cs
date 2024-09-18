using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using Newtonsoft.Json;
using UnityEngine.SDGraph;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Events;

namespace UnityEditor.SDGraph
{
	[NodeCustomEditor(typeof(SDSetModelNode))]
	public class SDSetModelNodeView : SDNodeView
	{
		private string[] modelNames;
		private Button getModelListButton;
		SDSetModelNode setModelNode;

		private bool isDebug = true;

		public override void Enable()
		{
			base.Enable();
			setModelNode = nodeTarget as SDSetModelNode;
				
			getModelListButton = new Button(GetModelList);
			getModelListButton.text = "Refresh Model List";
			extensionContainer.Add(getModelListButton);
			if (setModelNode!=null && setModelNode.modelNames!=null)
			{
				extensionContainer.Clear();
				// Create a VisualElement with a popup field
				var listContainer = new VisualElement();
				listContainer.style.flexDirection = FlexDirection.Row;
				listContainer.style.alignItems = Align.Center;
				listContainer.style.justifyContent = Justify.Center;

				List<string> stringList = new List<string>();
				stringList.AddRange(setModelNode.modelNames); 
				var popup = new PopupField<string>(stringList, setModelNode.currentIndex);

				// Add a callback to perform additional actions on value change
				popup.RegisterValueChangedCallback(evt =>
				{
					SDUtil.Log("Selected item: " + evt.newValue, isDebug);
					setModelNode.Model = evt.newValue;
					setModelNode.currentIndex = stringList.IndexOf(evt.newValue);
				});
				listContainer.Add(popup);
				extensionContainer.Add(getModelListButton);
				extensionContainer.Add(listContainer);
			}
			RefreshExpandedState();
		}
		
		public void GetModelList(UnityAction action = null)
        {
            //GetPort(nameof(setModelNode.ServerURL), null).PushData();
            EditorCoroutineUtility.StartCoroutine(ListModelsAsync(action), owner);
		}
		 
		/// <summary>
        /// Get the list of available Stable Diffusion models.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListModelsAsync(UnityAction unityAction = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                string serverUrl = "http://127.0.0.1:7860";
                serverUrl = SDGraphResource.SdGraphDataHandle.GetServerURL();
                string url = serverUrl + SDGraphResource.SdGraphDataHandle.ModelsAPI;

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                NetAuthorizationUtil.SetRequestAuthorization(httpWebRequest);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n\n" + e.StackTrace);
            }
            if (httpWebRequest != null)
            {
                // Wait that the generation is complete before procedding
                Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();
                while (!webResponse.IsCompleted)
                {           
#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
                    yield return new WaitForSeconds(100);
                }
                // Stream the result from the server
                var httpResponse = webResponse.Result;
                

                try
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {

                        // SDUtil.Log(request.downloadHandler.text);
                        // Decode the response as a JSON string
                        string result = streamReader.ReadToEnd();
                        // Deserialize the response to a class
                        SDModel[] ms = JsonConvert.DeserializeObject<SDModel[]>(result);

                        // Keep only the names of the models
                        List<string> modelsNames = new List<string>();

                        foreach (SDModel m in ms) 
                            modelsNames.Add(m.model_name);

                        // Convert the list into an array and store it for futur use
                        setModelNode.modelNames = modelsNames.ToArray();
                        SDUtil.Log($"models load success, Count: {setModelNode.modelNames.Length}");
                    }
                }
                catch (Exception)
                {
                    SDUtil.Log("Server needs and API key authentication. Please check your settings!");
                }
            }
            unityAction?.Invoke();
        }

		private void GetModelList()
		{
			var setModelNode = nodeTarget as SDSetModelNode;
			if (setModelNode != null)
			{
				GetModelList(() =>
				{
					modelNames = setModelNode.modelNames;
					if (modelNames != null && modelNames.Length > 0)
					{
						extensionContainer.Clear();
						// Create a VisualElement with a popup field
						var listContainer = new VisualElement();
						listContainer.style.flexDirection = FlexDirection.Row;
						listContainer.style.alignItems = Align.Center;
						listContainer.style.justifyContent = Justify.Center;

						List<string> stringList = new List<string>();
						stringList.AddRange(setModelNode.modelNames);
						var popup = new PopupField<string>(stringList, setModelNode.currentIndex);

						// Add a callback to perform additional actions on value change
						popup.RegisterValueChangedCallback(evt =>
						{
							SDUtil.Log("Selected item: " + evt.newValue, isDebug);
							setModelNode.Model = evt.newValue;
							setModelNode.currentIndex = stringList.IndexOf(evt.newValue);
						});

						listContainer.Add(popup);
						extensionContainer.Add(getModelListButton);
						extensionContainer.Add(listContainer);
						RefreshExpandedState();
					}
				});
			}
		}
	}
}

