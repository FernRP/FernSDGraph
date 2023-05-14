using UnityEngine;
using UnityEngine.UIElements;

public class LongPressManipulator : MouseManipulator
{
    private bool m_Active;
    private VisualElement m_TargetElement;

    public LongPressManipulator()
    {
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
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

        target.schedule.Execute(LongPressCallback).StartingIn(1);

        evt.StopPropagation();
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        Debug.Log("222");
        if (!m_Active)
            return;

        m_Active = false;

       // target.schedule.;

        evt.StopPropagation();
    }

    private void LongPressCallback()
    {
        if (!m_Active)
            return;

        // Handle long press event here
        Debug.Log("Long press detected!");

        m_Active = false;
    }
}