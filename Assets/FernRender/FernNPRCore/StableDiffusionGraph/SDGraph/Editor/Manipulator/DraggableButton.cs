using UnityEngine;
using UnityEngine.UIElements;

public class DraggableButton : TextElement, IManipulator
{
    private bool m_Active;
    private Vector2 m_StartPosition;
    private VisualElement m_TargetElement;
    private float m_DownTime;
    private float m_LongPressDuration = 1f;
    
    public new static readonly string ussClassName = "unity-button";

    public DraggableButton()
    {
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
        
        this.AddToClassList(Button.ussClassName);
        this.focusable = true;
        this.tabIndex = 0;
    }

    public void OnMouseDown(MouseDownEvent evt)
    {

        if (evt.button == (int)MouseButton.LeftMouse)
        {
            Debug.Log("Down");
            m_Active = true;
            m_StartPosition = evt.localMousePosition;
            m_TargetElement = evt.target as VisualElement;
            m_DownTime = Time.realtimeSinceStartup;
            evt.StopPropagation();
        }
    }

    public void OnMouseMove(MouseMoveEvent evt)
    {
        if (m_Active && Time.realtimeSinceStartup - m_DownTime >= m_LongPressDuration)
        {
            Debug.Log("Move");
            var delta = evt.localMousePosition - m_StartPosition;
            m_TargetElement.style.left = m_TargetElement.layout.x + delta.x;
            m_TargetElement.style.top = m_TargetElement.layout.y + delta.y;
            evt.StopPropagation();
        }
    }

    public void OnMouseUp(MouseUpEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            Debug.Log("Up");
            m_Active = false;
            evt.StopPropagation();
        }
    }

    public VisualElement target { get; set; }
}