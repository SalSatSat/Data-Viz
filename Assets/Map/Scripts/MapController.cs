// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;

public class MapController : MonoBehaviour
{
	//[Header("UI References")]
	//[Header("Prefabs")]

	public const float MinZoomLevel = 0;
	public const float MaxZoomLevel = 20;

	[Header("Settings")]
	[Tooltip("When enabled, the longitude/latitude/zoom settings will be ignored")]
	public bool startWithCenteredWorld = true;

	[Header("Location")]
	[SerializeField]
	private double longitude;
	[SerializeField]
	private double latitude;

	[Header("Zoom")]
	public float mapScale = 1f;
	[Range(MinZoomLevel, MaxZoomLevel)]
	public float minZoomLevel = 0f;
	[Range(MinZoomLevel, MaxZoomLevel)]
	public float maxZoomLevel = 20f;
	[Range(MinZoomLevel, MaxZoomLevel)]
	public float zoom = 10f;

	//
	// Unity Methods
	//

	private void Start()
	{
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void MoveInUnits(float x, float y)
	{

	}

	public void ChangeZoom(float change, float offsetX, float offsetY)
	{

	}

	//
	// Private Methods
	//



}