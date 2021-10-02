// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputEvent<T>
{
    private readonly List<T> handlers = new List<T>();

    public T LastHandler
    {
        get { return handlers[handlers.Count - 1]; }
    }

    public static InputEvent<T> operator +(InputEvent<T> ie, T del)
    {
        ie.handlers.Add(del);
        return ie;
    }

    public static InputEvent<T> operator -(InputEvent<T> ie, T del)
    {
        ie.handlers.Remove(del);
        return ie;
    }
}

public class InputHandler : MonoBehaviour
{
    private bool ignoreUI;
    private EventSystem eventSystem;
    private Camera cam;
    // Ground plane (for raycasting)
    private readonly Plane plane = new Plane(Vector3.up, Vector3.zero);

    public bool LongPressAsRMB { private set; get; }
    public bool IsPointerInUI { private set; get; }
    public Vector3 MouseDelta { private set; get; }
    public Vector3 PreviousMousePos { private set; get; }

    // Dragging
    public bool IsLeftMouseDown { private set; get; }
    public bool IsRightMouseDown { private set; get; }
    public bool IsDraggingLeft { private set; get; }
    public bool IsDraggingRight { private set; get; }
    public Vector3 StartLeftDragPos { private set; get; }
    public Vector3 StartRightDragPos { private set; get; }
    public bool IsLongPress { private set; get; }

    public delegate void OnMouseEvent();
    public InputEvent<OnMouseEvent> OnLeftMouseDown = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnRightMouseDown = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnLeftMouseUp = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnRightMouseUp = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnLeftMouseDragStart = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnRightMouseDragStart = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnLeftMouseDrag = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnRightMouseDrag = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnLeftMouseDragEnd = new InputEvent<OnMouseEvent>();
    public InputEvent<OnMouseEvent> OnRightMouseDragEnd = new InputEvent<OnMouseEvent>();

    public delegate void OnMouseWheelEvent(float delta);
    public InputEvent<OnMouseWheelEvent> OnMouseWheel = new InputEvent<OnMouseWheelEvent>();

    //
    // Unity Methods
    //

    private void Awake()
    {
        eventSystem = FindObjectOfType<EventSystem>();

        OnLeftMouseDown += DoNothing;
        OnRightMouseDown += DoNothing;
        OnLeftMouseUp += DoNothing;
        OnRightMouseUp += DoNothing;
        OnLeftMouseDragStart += DoNothing;
        OnRightMouseDragStart += DoNothing;
        OnLeftMouseDrag += DoNothing;
        OnRightMouseDrag += DoNothing;
        OnLeftMouseDragEnd += DoNothing;
        OnRightMouseDragEnd += DoNothing;
        OnMouseWheel += DoNothing;
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        MouseDelta = Input.mousePosition - PreviousMousePos;

        IsPointerInUI = eventSystem.IsPointerOverGameObject() && !ignoreUI;

        // Can only click or start a drag when the cursor is outside the UI
        if (!IsPointerInUI)
        {
            if (IsLeftMouseDown)
            {
                HandleLeftMouseDown();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                HandleLeftMousePressed();
            }

            if (IsRightMouseDown)
            {
                HandleRightMouseDown();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                HandleRightMousePressed();
            }

            HandleMouseWheel();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (IsLeftMouseDown)
            {
                HandleLeftMouseUp();
            }
            else if (IsLongPress)
            {
                // Fake a RMB up event
                HandleRightMouseUp();
            }
        }
        if (Input.GetMouseButtonUp(1) && IsRightMouseDown)
        {
            HandleRightMouseUp();
        }

        PreviousMousePos = Input.mousePosition;
    }

    //
    // Public Methods
    //

    public int PixelDragThreshold
	{
        get { return eventSystem.pixelDragThreshold; }
	}

    public void IgnoreUI(bool ignore)
    {
        ignoreUI = ignore;
    }

    //
    // Private Methods
    //

    private void HandleMouseWheel()
    {
        float deltaZ = Input.GetAxis("Mouse ScrollWheel");
        if (deltaZ != 0 && OnMouseWheel != null)
        {
            OnMouseWheel.LastHandler(deltaZ);
        }
    }

    private void HandleLeftMousePressed()
    {
        IsLeftMouseDown = true;
        StartLeftDragPos = Input.mousePosition;
        OnLeftMouseDown.LastHandler();
    }

    private void HandleRightMousePressed()
    {
        IsRightMouseDown = true;
        StartRightDragPos = Input.mousePosition;
        OnRightMouseDown.LastHandler();
    }

    private void HandleLeftMouseDown()
    {
        if (IsDraggingLeft)
        {
            OnLeftMouseDrag.LastHandler();
        }
        else
        {
            var offset = Input.mousePosition - StartLeftDragPos;
            if (Math.Abs(offset.x) > eventSystem.pixelDragThreshold ||
                Math.Abs(offset.y) > eventSystem.pixelDragThreshold)
            {
                IsDraggingLeft = true;
                OnLeftMouseDragStart.LastHandler();
            }
		}
    }

    private void HandleRightMouseDown()
    {
        if (IsDraggingRight)
        {
            OnRightMouseDrag.LastHandler();
        }
        else
        {
            var offset = Input.mousePosition - StartRightDragPos;
            if (Math.Abs(offset.x) > eventSystem.pixelDragThreshold ||
                Math.Abs(offset.y) > eventSystem.pixelDragThreshold)
            {
                IsDraggingRight = true;
                OnRightMouseDragStart.LastHandler();
            }
        }
    }

    private void HandleLeftMouseUp()
    {
        if (!IsPointerInUI || IsDraggingLeft)
        {
            OnLeftMouseUp.LastHandler();
        }
        if (IsDraggingLeft)
        {
            OnLeftMouseDragEnd.LastHandler();
            IsDraggingLeft = false;
        }

        IsLeftMouseDown = false;
    }

    private void HandleRightMouseUp()
    {
        if (!IsPointerInUI || IsDraggingRight)
        {
            OnRightMouseUp.LastHandler();
        }
        if (IsDraggingRight)
        {
            OnRightMouseDragEnd.LastHandler();
            IsDraggingRight = false;
        }

        IsRightMouseDown = false;
	}

    public bool GetWorldPoint(Vector3 screenPos, out Vector3 pt)
    {
		Ray ray = cam.ScreenPointToRay(screenPos);
		if (plane.Raycast(ray, out float distance))
        {
            pt = ray.GetPoint(distance);
            return true;
        }

        pt = Vector3.zero;
        return false;
    }

    private void DoNothing()
    {
        // Specifically do nothing
    }
    private void DoNothing(float delta)
    {
        // Specifically do nothing
    }
}