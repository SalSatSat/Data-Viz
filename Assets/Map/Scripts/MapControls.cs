// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using UnityEngine.UI;

public class MapControls : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Button zoomInButton = default;
	[SerializeField] private Button zoomOutButton = default;
	[SerializeField] private Button compassButton = default;

	//[Header("Prefabs")]
	[Header("References")]
	[SerializeField] private MapCamera mapCamera = default;

	[Header("Scales")]
	[Range(0.1f, 1.0f)]
	[SerializeField] private float zoomScale = 0.5f;

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(zoomInButton != null, "MapControls: Missing zoomInButton");
		Debug.Assert(zoomOutButton != null, "MapControls: Missing zoomOutButton");
		Debug.Assert(compassButton != null, "MapControls: Missing compassButton");
		Debug.Assert(mapCamera != null, "MapControls: Missing mapCamera");
	}

	private void Start()
	{
		mapCamera.OnBoundsChange += OnBoundsChange;

		// Initialize listeners
		zoomInButton.onClick.AddListener(OnZoomInButtonClicked);
		zoomOutButton.onClick.AddListener(OnZoomOutButtonClicked);
		compassButton.onClick.AddListener(OnCompassButtonClicked);

		compassButton.transform.parent.gameObject.SetActive(false);
	}

	//
	// Event Methods
	//

	private void OnBoundsChange()
	{
		float cameraAngleY = WrapAngle(mapCamera.Pivot.transform.rotation.eulerAngles.y);
		Quaternion quat = Quaternion.Euler(0, 0, cameraAngleY);
		compassButton.transform.rotation = quat;
		compassButton.transform.parent.gameObject.SetActive(cameraAngleY != 0.0f);
	}

	private void OnZoomInButtonClicked()
	{
		mapCamera.Zoom(zoomScale);
	}

	private void OnZoomOutButtonClicked()
	{
		mapCamera.Zoom(-zoomScale);
	}

	private void OnCompassButtonClicked()
	{
		mapCamera.ResetRotation();
		//mapCamera.ResetCameraOffset();
		OnBoundsChange();
	}

	//
	// Public Methods
	//



	//
	// Private Methods
	//

	private static float WrapAngle(float angle)
	{
		angle %= 360;
		if (angle > 180)
			return angle - 360;

		return angle;
	}

}