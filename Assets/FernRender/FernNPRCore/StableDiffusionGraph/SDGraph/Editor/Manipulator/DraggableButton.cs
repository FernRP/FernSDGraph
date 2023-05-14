using UnityEngine;
using UnityEngine.UIElements;

public class DraggableButton : Button, IManipulator
{
    private bool m_Active;
    private Vector2 m_StartPosition;
    private VisualElement m_TargetElement;

    public DraggableButton()
    {
        this.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    public void OnMouseDown(MouseDownEvent evt)
    {
        Debug.Log("Down");

        if (evt.button == (int)MouseButton.LeftMouse)
        {
            m_Active = true;
            m_StartPosition = evt.localMousePosition;
            m_TargetElement = evt.target as VisualElement;
            evt.StopPropagation();
        }
    }

    public void OnMouseMove(MouseMoveEvent evt)
    {

        if (m_Active)
        {
            var delta = evt.localMousePosition - m_StartPosition;
            m_TargetElement.style.left = m_TargetElement.layout.x + delta.x;
            m_TargetElement.style.top = m_TargetElement.layout.y + delta.y;
            evt.StopPropagation();
        }
    }

    public void OnMouseUp(MouseUpEvent evt)
    {
        Debug.Log("Up");
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            m_Active = false;
            evt.StopPropagation();
        }
    }

    public VisualElement target { get; set; }
}