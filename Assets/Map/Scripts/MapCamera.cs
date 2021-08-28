// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private InputHandler inputHandler = default;
	[SerializeField] private Transform map = default;
	
	[Header("Camera")]
	[SerializeField] private Transform pivot = default;
	public Transform Pivot { get { return pivot; } }
	[SerializeField] private float minDistanceToMap = 50.0f;
	[SerializeField] private float maxDistanceToMap = 250.0f;
	[SerializeField] private float currDistanceToMap = 150f;

	[Header("Rotation Constraints")]
	[SerializeField] private float minPitchAngle = 30.0f;
	[SerializeField] private float maxPitchAngle = 90.0f;

	[SerializeField] private float minYawAngle = -135.0f;
	[SerializeField] private float maxYawAngle = 135.0f;

	[Header("Rotation, Zoom & Pan Scales")]
	[Range(0.1f, 1.0f)]
	[SerializeField] private float horizontalOrbitScale = 0.5f;
	[Range(0.1f, 1.0f)]
	[SerializeField] private float verticalOrbitScale = 0.5f;
	[Range(5.0f, 25.0f)]
	[SerializeField] private float zoomScale = 15.0f;
	[Range(0.1f, 1.0f)]
	[SerializeField] private float panScale = 0.5f;

	// Reference to Camera component
	private Camera cam;
	private Transform currentPivot;

	// Dragging
	private enum DragType
	{
		None,
		Pan,
		Orbit
	}
	private DragType dragType = DragType.None;
	private Vector3 dragWorldOrigin;

	private bool needToUpdateBounds = false;

	public delegate void OnBoundsChangeDelegate();
	public event OnBoundsChangeDelegate OnBoundsChange;

	private const float MaxDistanceToMapCentre = 100;

	//
	// Unity Methods
	//

	private void OnEnable()
	{
		cam = GetComponent<Camera>();

		// Initialize variables
		WrapAngle(ref minYawAngle);
		WrapAngle(ref maxYawAngle);

		currentPivot = pivot ?? transform;

		// Initialize camera position
		UpdatePosition();

		needToUpdateBounds = true;
	}

	private void Awake()
	{
		Debug.Assert(inputHandler != null, "MapCamera: Missing inputHandler");
	}

	private void Start()
	{
		// Initialize Input
		if (inputHandler)
		{
			inputHandler.OnLeftMouseDragStart += OnLeftMouseDragStart;
			inputHandler.OnRightMouseDragStart += OnRightMouseDragStart;
			inputHandler.OnLeftMouseDrag += OnLeftMouseDrag;
			inputHandler.OnRightMouseDrag += OnRightMouseDrag;
			inputHandler.OnLeftMouseDragEnd += OnLeftMouseDragEnd;
			inputHandler.OnRightMouseDragEnd += OnRightMouseDragEnd;
			inputHandler.OnMouseWheel += OnMouseWheel;
		}

		// Adjust camera position for map-view-area
		UpdatePosition();
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (needToUpdateBounds || !Application.isPlaying)
#else
		if (needToUpdateBounds)
#endif
		{
			needToUpdateBounds = false;
			UpdateMapBounds();
		}
	}

	//
	// Event Methods
	//

	private void OnLeftMouseDragStart()
	{
		if (dragType == DragType.None)
		{
			dragType = DragType.Pan;
			StartPan();
		}
	}

	private void OnRightMouseDragStart()
	{
		if (dragType == DragType.None)
		{
			dragType = DragType.Orbit;
		}
	}

	private void OnLeftMouseDrag()
	{
		if (dragType == DragType.Pan)
		{
			float deltaX = inputHandler.MouseDelta.x;
			float deltaY = inputHandler.MouseDelta.y;
			if (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0)
			{
				Pan();
			}
		}
	}

	private void OnRightMouseDrag()
	{
		if (dragType == DragType.Orbit)
		{
			float deltaX = inputHandler.MouseDelta.x;
			float deltaY = inputHandler.MouseDelta.y;
			if (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0)
			{
				Orbit(deltaY * verticalOrbitScale, deltaX * horizontalOrbitScale);
			}
		}
	}

	private void OnLeftMouseDragEnd()
	{
		if (dragType == DragType.Pan)
		{
			dragType = DragType.None;
		}
	}

	private void OnRightMouseDragEnd()
	{
		if (dragType == DragType.Orbit)
		{
			dragType = DragType.None;
		}
	}

	private void OnMouseWheel(float delta)
	{
		if (dragType == DragType.None)
		{
			Zoom(delta);
		}
	}

	//
	// Public Methods
	//

	public void ResetRotation()
	{
		currentPivot.rotation = Quaternion.Euler(90, 0, 0);
		UpdatePosition();
	}

	public void ResetCameraOffset()
	{
		cam.transform.localPosition = Vector3.zero;
	}

	public void Zoom(float change)
	{
		currDistanceToMap = Mathf.Clamp(currDistanceToMap - change * zoomScale, minDistanceToMap, maxDistanceToMap);
		UpdatePosition();
	}

	//
	// Private Methods
	//

	private static void WrapAngle(ref float angle)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
	}

	private void StartPan()
	{
		inputHandler.GetWorldPoint(Input.mousePosition, out dragWorldOrigin);
	}

	private void Pan()
	{
		if (inputHandler.GetWorldPoint(Input.mousePosition, out Vector3 point))
		{
			PanMap(dragWorldOrigin - point);
			dragWorldOrigin = point;
		}
	}

	private void PanMap(Vector3 offset)
	{
		Vector3 current = cam.transform.position;
		Vector3 target = cam.transform.position + new Vector3(offset.x, 0.0f, offset.z);

		Vector3 updatedCamPosition = Vector3.MoveTowards(current, target, panScale);
		cam.transform.position = new Vector3(Mathf.Clamp(updatedCamPosition.x, map.position.x - MaxDistanceToMapCentre, map.position.x + MaxDistanceToMapCentre),
											 updatedCamPosition.y,
											 Mathf.Clamp(updatedCamPosition.z, map.position.z - MaxDistanceToMapCentre, map.position.z + MaxDistanceToMapCentre));
	}

	private void Orbit(float pitch, float yaw)
	{
		Vector3 euler = currentPivot.eulerAngles + new Vector3(-pitch, yaw, 0);
		if (euler.y > 180f)
			euler.y -= 360f;

		currentPivot.rotation = Quaternion.Euler(
			Mathf.Clamp(euler.x, minPitchAngle, maxPitchAngle),
			Mathf.Clamp(euler.y, minYawAngle, maxYawAngle),
			0);

		UpdatePosition();
	}

	private void UpdatePosition()
	{
		currentPivot.localPosition = currentPivot.forward * -currDistanceToMap;
		needToUpdateBounds = true;
	}

	private void UpdateMapBounds()
	{
		OnBoundsChange?.Invoke();
	}
}