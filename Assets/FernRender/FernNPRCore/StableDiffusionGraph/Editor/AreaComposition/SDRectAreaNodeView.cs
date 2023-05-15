using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDRectAreaNode))]
    public class SDRectAreaNodeView : NodeView
    {
        private int currentColorIndex = 0;
        private Rect currentRegion;
        private bool isDragging;
        private bool isHoldMouse;
        private bool forceUpdateTexture;
        private int selectedIndex = -1;
        private Vector2 dragOffset;
        Rect gridRect;
        SDRectAreaNode rectAreaNode;

        private VisualElement areaElement;

        float currentScale, previewScale;
        float cellWidth, cellHeight;

        Texture2D previewTexture;
        protected override void OnInitialize()
        {
            base.OnInitialize();

            rectAreaNode = Target as SDRectAreaNode;

            var root = extensionContainer;
            currentScale = previewScale = rectAreaNode.scale;

            var previewWidthField = new Slider("Preview Width",0.5f,1);
            previewWidthField.value = previewScale;
            previewWidthField.RegisterValueChangedCallback(e =>
            {
                currentScale = previewScale;
                rectAreaNode.scale = previewScale = e.newValue;
                UpdateGridRect();
                UpdateAreaTexture();
            });
            root.Add(previewWidthField);

            var colCountField = new IntegerField("Column Count");
            colCountField.value = rectAreaNode.colCount;
            colCountField.RegisterValueChangedCallback(e => { rectAreaNode.colCount = e.newValue; UpdateAreaTexture(); });
            root.Add(colCountField);

            var rowCountField = new IntegerField("Row Count");
            rowCountField.value = rectAreaNode.rowCount;
            rowCountField.RegisterValueChangedCallback(e => { rectAreaNode.rowCount  = e.newValue; UpdateAreaTexture(); });
            root.Add(rowCountField);

            var isSnapGridToggle = new Toggle("Snap to Grid");
            isSnapGridToggle.value = rectAreaNode.isSnapGrid;
            isSnapGridToggle.RegisterValueChangedCallback(e => { rectAreaNode.isSnapGrid  = e.newValue; UpdateAreaTexture(); });
            root.Add(isSnapGridToggle);

            var showGridToggle = new Toggle("Show Grid");
            showGridToggle.value = rectAreaNode.showGrid;
            showGridToggle.RegisterValueChangedCallback(e => { rectAreaNode.showGrid = e.newValue; UpdateAreaTexture(); });
            root.Add(showGridToggle);

            var alwaysUpdateTextureToggle = new Toggle("AlwaysUpdateTexture");
            alwaysUpdateTextureToggle.value = rectAreaNode.alwaysUpdateTexture;
            alwaysUpdateTextureToggle.RegisterValueChangedCallback(e => { rectAreaNode.alwaysUpdateTexture = e.newValue; UpdateAreaTexture(); });
            root.Add(alwaysUpdateTextureToggle);
            

            var deleteButton = new Button(DeleteRegion);
            deleteButton.text = "Delete Selected Region";
            root.Add(deleteButton);
           
            areaElement = new VisualElement();
            areaElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            areaElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
            areaElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            areaElement.RegisterCallback<MouseOutEvent>(OnMouseOut);
            UpdateGridRect();
            root.Add(areaElement);
            UpdateAreaTexture();
            RefreshExpandedState();
        }
        int displayWidth, displayHeight;
        void UpdateGridRect()
        {
            for (int i = 0; i < rectAreaNode.regions.Count; i++)
            {
                rectAreaNode.regions[i].rect = new Rect(rectAreaNode.regions[i].rect.x / currentScale * previewScale,
                    rectAreaNode.regions[i].rect.y / currentScale * previewScale,
                    rectAreaNode.regions[i].rect.width / currentScale * previewScale,
                    rectAreaNode.regions[i].rect.height / currentScale * previewScale
                    );
            }
            displayWidth = (int)(rectAreaNode.Width * previewScale);
            displayHeight = (int)(rectAreaNode.Height * previewScale);
            cellWidth = displayWidth / rectAreaNode.colCount;
            cellHeight = displayHeight / rectAreaNode.rowCount;

            areaElement.style.width = displayWidth;
            areaElement.style.height = displayHeight;
            gridRect = new Rect(0, 0, displayWidth, displayHeight);
            previewTexture = new Texture2D(displayWidth, displayHeight);
        }

        void DeleteRegion()
        {
            if (selectedIndex != -1)
            {
                rectAreaNode.regions.RemoveAt(selectedIndex);
                selectedIndex = -1;
            }
            else
            {
                if(rectAreaNode.regions.Count>0)
                    rectAreaNode.regions.RemoveAt(rectAreaNode.regions.Count-1);
            }
            UpdateAreaTexture();
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            Vector2 mousePos = e.localMousePosition;
            selectedIndex = -1;
            isHoldMouse = true;

            for (int i = 0; i < rectAreaNode.regions.Count; i++)
            {
                Rect displayRegion = new Rect(rectAreaNode.regions[i].rect.position  + gridRect.position, rectAreaNode.regions[i].rect.size );

                if (displayRegion.Contains(e.localMousePosition))
                {
                    selectedIndex = i;
                    dragOffset = e.localMousePosition - new Vector2(displayRegion.x, displayRegion.y);
                    forceUpdateTexture = true;
                    break;
                }
            }

            if (selectedIndex == -1)
            {
                isDragging = true;

                if (rectAreaNode.isSnapGrid)
                {
                    Vector2 snappedPosition = SnapToGrid(mousePos, gridRect.position, cellWidth, cellHeight, displayWidth, displayHeight);
                    currentRegion = new Rect(snappedPosition.x, snappedPosition.y, 0, 0);
                }
                else
                {
                    currentRegion = new Rect(mousePos.x, mousePos.y, 0, 0);
                }
            }
            e.StopPropagation();

        }

        private void OnMouseOut(MouseOutEvent e)
        {
            OnMouseRelease(e.localMousePosition);
            e.StopPropagation();
        }

        void OnMouseRelease(Vector2 mousePos)
        {
            isHoldMouse = false;
            if (isDragging)
            {
                isDragging = false;

                if (rectAreaNode.isSnapGrid)
                {
                    Vector2 snappedPosition = SnapToGrid(mousePos, gridRect.position, cellWidth, cellHeight, displayWidth, displayHeight);
                    currentRegion.width = snappedPosition.x - currentRegion.x;
                    currentRegion.height = snappedPosition.y - currentRegion.y;
                }
                else
                {
                    float mouseX = Mathf.Clamp(mousePos.x, gridRect.xMin, gridRect.xMax);
                    float mouseY = Mathf.Clamp(mousePos.y, gridRect.yMin, gridRect.yMax);
                    currentRegion.width = mouseX - currentRegion.x;
                    currentRegion.height = mouseY - currentRegion.y;
                }

                Rect actualRegion = new Rect(currentRegion.x - gridRect.xMin, currentRegion.y - gridRect.yMin, currentRegion.width, currentRegion.height);
                actualRegion = new Rect(actualRegion.position, actualRegion.size);

                rectAreaNode.regions.Add(new RectRegion(actualRegion, SDDataHandle.Instance.areaColors[currentColorIndex]));
                currentColorIndex = (currentColorIndex + 1) % SDDataHandle.Instance.areaColors.Length;
                currentRegion = new Rect();
                UpdateAreaTexture();
            }
            else if (forceUpdateTexture)
            {
                UpdateAreaTexture();
                forceUpdateTexture = false;
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            OnMouseRelease(e.localMousePosition);

            e.StopPropagation();

        }
        private void OnMouseMove(MouseMoveEvent e)
        {
            Vector2 mousePos = e.localMousePosition;

            if (selectedIndex != -1)
            {
                if (isHoldMouse)
                {
                    Vector2 newPosition = e.localMousePosition - dragOffset;

                    if (rectAreaNode.isSnapGrid)
                    {
                        newPosition = SnapToGrid(newPosition,gridRect.position,cellWidth, cellHeight, displayWidth, displayHeight,false);
                    }

                    newPosition.x = Mathf.Clamp(newPosition.x, gridRect.xMin, gridRect.xMax - rectAreaNode.regions[selectedIndex].rect.width);
                    newPosition.y = Mathf.Clamp(newPosition.y, gridRect.yMin, gridRect.yMax - rectAreaNode.regions[selectedIndex].rect.height);

                    Rect actualRegion = new Rect((newPosition - gridRect.position) , rectAreaNode.regions[selectedIndex].rect.size);

                    rectAreaNode.regions[selectedIndex] = new RectRegion(actualRegion, rectAreaNode.regions[selectedIndex].color);
                    forceUpdateTexture = true;
                    if (rectAreaNode.alwaysUpdateTexture)
                    {
                        UpdateAreaTexture();
                    }
                }
            }
            else if (isDragging)
            {
                if (rectAreaNode.isSnapGrid)
                {
                    Vector2 snappedPosition = SnapToGrid(mousePos, gridRect.position, cellWidth, cellHeight, displayWidth, displayHeight);
                    currentRegion.width = snappedPosition.x - currentRegion.x;
                    currentRegion.height = snappedPosition.y - currentRegion.y;
                }
                else
                {
                    float mouseX = Mathf.Clamp(mousePos.x, gridRect.xMin, gridRect.xMax);
                    float mouseY = Mathf.Clamp(mousePos.y, gridRect.yMin, gridRect.yMax);
                    currentRegion.width = mouseX - currentRegion.x;
                    currentRegion.height = mouseY - currentRegion.y;
                }
            }

            e.StopPropagation();

        }

        private void UpdateAreaTexture()
        {
            //width = rectAreaNode.Width;
            //height = rectAreaNode.Height;
            //if (rectAreaNode.areaTexture.width != width || rectAreaNode.areaTexture.height != height)
            //{
            //    rectAreaNode.areaTexture.Reinitialize(width, height);
            //}

            Color[] pixels = new Color[displayWidth * displayHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            previewTexture.SetPixels(pixels);

            if (rectAreaNode.showGrid)
            {
                DrawGrid();
            }
            //rectAreaNode.areaTexture = new Texture2D(rectAreaNode.Width, rectAreaNode.Height);

            for (int i = 0; i < rectAreaNode.regions.Count; i++)
            {
                if (selectedIndex != -1 && selectedIndex == i)
                {
                    DrawRect(previewTexture, rectAreaNode.regions[i].rect, rectAreaNode.regions[i].color * 0.5f, displayWidth, displayHeight);
                }
                else
                {
                    DrawRect(previewTexture, rectAreaNode.regions[i].rect, rectAreaNode.regions[i].color, displayWidth, displayHeight);
                }
            }

            previewTexture.Apply();
            //rectAreaNode.areaTexture.Apply();


            areaElement.style.backgroundImage = Background.FromTexture2D(previewTexture);
        }

        private void DrawGrid()
        {
            float cellWidth = displayWidth / rectAreaNode.colCount;
            float cellHeight = displayHeight / rectAreaNode.rowCount;

            for (int i = 0; i < rectAreaNode.colCount + 1; i++)
            {
                float x = i * cellWidth;
                DrawLine(new Vector2(x, displayHeight), new Vector2(x, 0), Color.gray);
            }

            for (int i = 0; i < rectAreaNode.rowCount + 1; i++)
            {
                float y = i * cellHeight;
                DrawLine(new Vector2(0, displayHeight - y), new Vector2(displayWidth, displayHeight - y), Color.gray);
            }
        }

        private void DrawRect(Texture2D tex, Rect rect, Color color, int width, int height)
        {
            for (int y = (int)rect.yMin; y < rect.yMax; y++)
            {
                for (int x = (int)rect.xMin; x < rect.xMax; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {

                        tex.SetPixel(x, height - 1 - y, color);
                    }
                }
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            float dx = Mathf.Abs(end.x - start.x);
            float dy = Mathf.Abs(end.y - start.y);

            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;

            float err = dx - dy;

            while (true)
            {
                if (start.x >= 0 && start.x < displayWidth && start.y >= 0 && start.y < displayHeight)
                {
                    previewTexture.SetPixel((int)start.x, (int)(displayHeight - start.y - 1), color);
                }

                if (start.x == end.x && start.y == end.y) break;

                float e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    start.x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    start.y += sy;
                }
            }
        }

        private Vector2 SnapToGrid(Vector2 position, Vector2 origin, float cellWidth, float cellHeight, float rectWidth, float rectHeight, bool round = true)
        {
            float x, y;
            if (round)
            {
                x = Mathf.Round((position.x - origin.x) / cellWidth) * cellWidth + origin.x;
                y = Mathf.Round((position.y - origin.y) / cellHeight) * cellHeight + origin.y;
            }
            else
            {
                x = (int)((position.x - origin.x) / cellWidth) * cellWidth + origin.x;
                y = (int)((position.y - origin.y) / cellHeight) * cellHeight + origin.y;
            }

            x = Mathf.Clamp(x, origin.x, origin.x + rectWidth);
            y = Mathf.Clamp(y, origin.y, origin.y + rectHeight);

            return new Vector2(x, y);
        }
    }
}