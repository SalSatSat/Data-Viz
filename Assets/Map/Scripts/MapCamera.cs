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
	[SerializeField] private MapController mapController = default;
	[SerializeField] private InputHandler inputHandler = default;
	
	[Header("UI References")]
	[SerializeField] private Canvas canvas = default;

	//[Header("Prefabs")]

	[Header("Camera")]
	[SerializeField] private Transform pivot = default;
	public Transform Pivot { get { return pivot; } }
	[Range(50.0f, 200.0f)]
	[SerializeField] private float distanceToMap = 100f;

	[Header("Rotation Constraints")]
	[SerializeField] private float MinPitchAngle = 30.0f;
	[SerializeField] private float MaxPitchAngle = 90.0f;

	[SerializeField] private float MinYawAngle = -135.0f;
	[SerializeField] private float MaxYawAngle = 135.0f;

	[Header("Rotation & Zoom Scales")]
	[SerializeField] private float HorizontalOrbitScale = 3.0f;
	[SerializeField] private float VerticalOrbitScale = 3.0f;

	// Reference to Camera component
	private Camera cam;
	private Transform currentPivot;

	// Reference to other components

	// Dragging
	private enum DragType
	{
		None,
		Pan,
		Orbit
	}
	private DragType dragType = DragType.None;
	private Vector3 dragWorldOrigin;

	// Ground plane (for raycasting)
	private readonly Plane plane = new Plane(Vector3.up, Vector3.zero);

	public Vector2[] BoundaryPoints { get; private set; } = new Vector2[4] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };

	private bool needToUpdateBounds = false;
	private bool needToUpdateViewport = false;
	private bool needToAdjustZoom = false;
	private Vector2 mapViewCenter = Vector2.zero;
	private int lastPixelHeight = 0;

	// Touch
	private const float Log2 = 0.30103f;
	private float initialZoom;
	private float invTouchDistance;
	private Vector2 initialTouchVector;
	private float initialCameraYaw;
	private Vector3 prevTouchCenterWorld;

	// Bottom-left, top-left, top-right and bottom-right corners of the viewport
	private static readonly Vector3[] FullViewportPoints = {
		new Vector3(0, 0, 0),
		new Vector3(0, 1, 0),
		new Vector3(1, 1, 0),
		new Vector3(1, 0, 0),
	};
	private readonly Vector3[] viewportPoints = new Vector3[4];

	public delegate void OnBoundsChangeDelegate();
	public event OnBoundsChangeDelegate OnBoundsChange;

	//
	// Unity Methods
	//

	private void OnEnable()
	{
		cam = GetComponent<Camera>();

		// Initialize variables
		WrapAngle(ref MinYawAngle);
		WrapAngle(ref MaxYawAngle);

		currentPivot = pivot ?? transform;

		// Initialize camera position
		UpdateDistanceToMap();

		needToUpdateBounds = true;
		needToUpdateViewport = true;
	}

	private void Awake()
	{
		Debug.Assert(inputHandler != null, "MapCamera: Missing inputHandler");
		Debug.Assert(mapController != null, "MapCamera: Missing mapController");
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
		UpdateMapViewCenter();
	}

	private void Update()
	{
		if (Input.touchCount == 2)
		{
			Touch t1 = Input.GetTouch(0);
			Touch t2 = Input.GetTouch(1);

			var touchCenter = (t1.position + t2.position) * 0.5f;

			inputHandler.GetWorldPoint(touchCenter, out Vector3 touchCenterWorld);

			if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
			{
				initialTouchVector = t2.position - t1.position;
				initialCameraYaw = currentPivot.rotation.eulerAngles.y;

				initialZoom = mapController.zoom;
				invTouchDistance = 1f / (t1.position - t2.position).magnitude;
			}
			else
			{
				float distance = (t1.position - t2.position).magnitude;
				float newZoom = initialZoom + Mathf.Log10(distance * invTouchDistance) / Log2;

				var yawDiff = Angle(t2.position - t1.position, initialTouchVector) - currentPivot.rotation.eulerAngles.y + initialCameraYaw;

				mapController.ChangeZoom(newZoom - mapController.zoom, touchCenterWorld.x, touchCenterWorld.z);
				PanMap(prevTouchCenterWorld);
				Orbit(0, yawDiff);
				PanMap(-touchCenterWorld);
			}

			prevTouchCenterWorld = touchCenterWorld;
		}

#if UNITY_EDITOR
		if (needToUpdateBounds || !Application.isPlaying)
#else
		if (needToUpdateBounds)
#endif
		{
			needToUpdateBounds = false;

			if (needToUpdateViewport)
			{
				needToUpdateViewport = false;
				UpdateCameraOffset();
			}

			UpdateMapBounds();
			needToAdjustZoom = false;
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
				Orbit(deltaY * VerticalOrbitScale, deltaX * HorizontalOrbitScale);
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



	//
	// Private Methods
	//

	private static float Angle(Vector2 a, Vector2 b)
	{
		var an = a.normalized;
		var bn = b.normalized;
		var x = an.x * bn.x + an.y * bn.y;
		var y = an.y * bn.x - an.x * bn.y;
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

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
		mapController.MoveInUnits(offset.x, offset.z);
	}

	public void ResetRotation()
	{
		currentPivot.rotation = Quaternion.Euler(90, 0, 0);
		UpdatePosition();
	}

	private void Orbit(float pitch, float yaw)
	{
		Vector3 euler = currentPivot.eulerAngles + new Vector3(-pitch, yaw, 0);
		if (euler.y > 180f)
			euler.y -= 360f;

		currentPivot.rotation = Quaternion.Euler(
			Mathf.Clamp(euler.x, MinPitchAngle, MaxPitchAngle),
			Mathf.Clamp(euler.y, MinYawAngle, MaxYawAngle),
			0);

		UpdatePosition();
	}

	private void Zoom(float change)
	{
		inputHandler.GetWorldPoint(Input.mousePosition, out Vector3 zoomPoint);
		mapController.ChangeZoom(change, zoomPoint.x, zoomPoint.z);
	}

	private void UpdatePosition()
	{
		currentPivot.localPosition = currentPivot.forward * -distanceToMap;
		needToUpdateBounds = true;
	}

	private void UpdateDistanceToMap()
	{
		// Set camera distance so that MapTile.Size pixels represent 1 unit in world space
		//distanceToMap = DistanceFromPixelsAndUnits(MapTile.Size, 1f / mapController.mapScale);
		UpdatePosition();
	}

	private void UpdateMapViewCenter()
	{
		UpdateDistanceToMap();

		needToUpdateBounds = true;
		needToUpdateViewport = true;

		if (lastPixelHeight != cam.pixelHeight)
		{
			lastPixelHeight = cam.pixelHeight;
			needToAdjustZoom = true;
		}
	}

	private void UpdateCameraOffset()
	{
		cam.transform.localPosition = Vector3.zero;

		UpdateViewportBounds();
	}

	private void UpdateViewportBounds()
	{
		for (int i = 0; i < 4; i++)
		{
			viewportPoints[i] = FullViewportPoints[i];
		}
	}

	private void UpdateMapBounds()
	{
		if (OnBoundsChange != null)
			OnBoundsChange();
	}
}