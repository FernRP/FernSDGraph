using UnityEngine;
using UnityEngine.UIElements;

public class DragManipulator : PointerManipulator
{
    private bool m_Active;
    private Vector2 m_StartPosition;
    private VisualElement m_TargetElement;

    public DragManipulator()
    {
        activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse});
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<MouseOutEvent>(OnMouseOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<MouseOutEvent>(OnMouseOut);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        Debug.Log("111");
        if (m_Active)
            return;

        m_TargetElement = evt.target as VisualElement;
        if (m_TargetElement == null)
            return;

        m_Active = true;
        m_StartPosition = evt.localMousePosition;

        target.CaptureMouse();
        evt.StopPropagation();
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!m_Active)
            return;

        var delta = evt.localMousePosition - m_StartPosition;
        m_TargetElement.style.left = m_TargetElement.layout.x + delta.x;
        m_TargetElement.style.top = m_TargetElement.layout.y + delta.y;

        evt.StopPropagation();
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (!m_Active)
            return;

        m_Active = false;
        target.ReleaseMouse();
        evt.StopPropagation();
    }
    
    private void OnMouseOut(MouseOutEvent evt)
    {
        Debug.Log("Out");
        if (!m_Active)
            return;

        m_Active = false;
        target.ReleaseMouse();
        evt.StopPropagation();
    }
}