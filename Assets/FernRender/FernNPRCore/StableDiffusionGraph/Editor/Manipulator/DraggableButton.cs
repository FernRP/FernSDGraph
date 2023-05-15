using System;
using FernNPRCore.StableDiffusionGraph;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableButton : TextElement, IManipulator
{
    private bool m_Active;
    private Vector2 m_StartPosition;
    private VisualElement m_TargetElement;
    private float m_DownTime;
    private float m_LongPressDuration = 0.333f;

    private Action<MouseUpEvent> m_OnClickAction;
    public event Action<MouseUpEvent> OnClickAction
    {
        add => m_OnClickAction += value;
        remove => m_OnClickAction -= value;
    }
    
    private Action<MouseMoveEvent> m_OnMoveAction;
    public event Action<MouseMoveEvent> OnMoveAction
    {
        add => m_OnMoveAction += value;
        remove => m_OnMoveAction -= value;
    }
    
    private Action<MouseUpEvent> m_OnMoveUpAction;
    public event Action<MouseUpEvent> OnMoveUpAction
    {
        add => m_OnMoveUpAction += value;
        remove => m_OnMoveUpAction -= value;
    }
    
    public new static readonly string ussClassName = "unity-button";

    public DraggableButton()
    {
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
        this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        this.AddToClassList(ussClassName);
        this.focusable = true;
        this.tabIndex = 0;
    }

    public DraggableButton(Action<MouseUpEvent> mOnClickAction)
    {
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
        this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        m_OnClickAction = mOnClickAction;
        this.AddToClassList(ussClassName);
        this.focusable = true;
        this.tabIndex = 0;
    }
    
    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        m_Active = false;
        m_DownTime = 0;
    }

    public void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
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
            SDUtil.Log("DraggableButton: Move", false);
            var delta = evt.localMousePosition - m_StartPosition;
            m_TargetElement.style.left = m_TargetElement.layout.x + delta.x;
            m_TargetElement.style.top = m_TargetElement.layout.y + delta.y;
            m_OnMoveAction?.Invoke(evt);
            evt.StopPropagation();
        }
    }

    public void OnMouseUp(MouseUpEvent evt)
    {
        if (m_Active && Time.realtimeSinceStartup - m_DownTime < m_LongPressDuration)
        {
            SDUtil.Log("DraggableButton: Click Action", false);
            m_OnClickAction?.Invoke(evt);
        }
        else
        {
            SDUtil.Log("DraggableButton: Move Up Action", false);
            m_OnMoveUpAction?.Invoke(evt);
        }
        m_Active = false;
        m_DownTime = 0;
        evt.StopPropagation();
    }

    public VisualElement target { get; set; }
}