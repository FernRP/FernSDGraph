using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LongPressButton : RepeatButton, IManipulator
{
    private bool m_Active;
    private float m_PressStartTime;
    private float m_LongPressDuration = 0.5f; // 长按持续时间
    private Vector2 m_StartPosition;
    public VisualElement target { get; set; }
    private VisualElement m_TargetElement;

    public event Action OnLongPress;

    public LongPressButton()
    {
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    public void OnMouseDown(MouseDownEvent evt)
    {
        Debug.Log("OnMouseDown");
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            m_Active = true;
            m_PressStartTime = Time.realtimeSinceStartup;
            m_StartPosition = evt.localMousePosition;
            m_TargetElement = evt.target as VisualElement;
            evt.StopPropagation();
        }
    }

    public void OnMouseMove(MouseMoveEvent evt)
    {
        if (m_Active)
        {
            Debug.Log("OnMouseMove");

            if (Time.realtimeSinceStartup - m_PressStartTime >= m_LongPressDuration)
            {
                m_Active = false;
                var delta = evt.localMousePosition - m_StartPosition;
                m_TargetElement.style.left = m_TargetElement.layout.x + delta.x;
                m_TargetElement.style.top = m_TargetElement.layout.y + delta.y;
                OnLongPress?.Invoke();
            }
            evt.StopPropagation();
        }
    }

    public void OnMouseUp(MouseUpEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            Debug.Log("OnMouseUp");

            m_Active = false;
            evt.StopPropagation();
        }
    }

}