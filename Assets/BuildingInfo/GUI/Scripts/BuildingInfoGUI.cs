// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BuildingInfoGUI : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private TMP_Dropdown infoDropdown = default;
	[SerializeField] private FilterRange filterRange = default;

	[Header("Reference")]
	[SerializeField] private Neighbourhood neighbourhood = default;

	private BuildingInfo selectedBuildingInfo = null;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(infoDropdown != null, "BuildingInfo: Missing infoDropdown");
		Debug.Assert(filterRange != null, "BuildingInfo: Missing filterRange");
		Debug.Assert(neighbourhood != null, "BuildingInfo: Missing neighbourhood");
	}

	private void Start()
	{
		InitDropdown();

		// Initialize color in range shader
		filterRange.SetRangeMaterialColor("_Color3", neighbourhood.OutOfRangeColor);

		// Initilaize listeners
		infoDropdown.onValueChanged.AddListener(OnOptionChanged);
		filterRange.MinSlider.onValueChanged.AddListener(OnMinValueChanged);
		filterRange.MaxSlider.onValueChanged.AddListener(OnMaxValueChanged);
	}

	private void OnApplicationQuit()
	{
		// Initilaize listeners
		infoDropdown.onValueChanged.RemoveListener(OnOptionChanged);
		filterRange.MinSlider.onValueChanged.RemoveListener(OnMinValueChanged);
		filterRange.MaxSlider.onValueChanged.RemoveListener(OnMaxValueChanged);

		UpdateFilterScale(0);
	}

	//
	// Event Methods
	//

	private void OnOptionChanged(int option)
	{
		neighbourhood.UpdateNeighbourhoodInfos(option);
		UpdateFilterScale(option);
	}

	private void OnMinValueChanged(float normalizedMin)
	{
		float normalizedMax = filterRange.MaxSlider.value;

		// Ensure min slider does not go past max slider
		if (normalizedMin > normalizedMax)
			filterRange.MinSlider.value = normalizedMax;

		// Update min value label
		float minValue = (normalizedMin * (selectedBuildingInfo.maxVal - selectedBuildingInfo.minVal) + selectedBuildingInfo.minVal);
		filterRange.MinValue.text = minValue.ToString("0.##");

		// Update min property in RangeGradient shader
		filterRange.SetRangeMaterialFloat("_Min", normalizedMin);

		neighbourhood.UpdateSelectedNeighbourhoodColors(normalizedMin, normalizedMax);
		neighbourhood.UpdateBuildingActives(normalizedMin, normalizedMax);
	}

	private void OnMaxValueChanged(float normalizedMax)
	{
		float normalizedMin = filterRange.MinSlider.value;

		// Ensure max slider does not go past min slider
		if (normalizedMax < normalizedMin)
			filterRange.MaxSlider.value = normalizedMin;

		// Update max value label
		float maxValue = (normalizedMax * (selectedBuildingInfo.maxVal - selectedBuildingInfo.minVal) + selectedBuildingInfo.minVal);
		filterRange.MaxValue.text = maxValue.ToString("0.##");

		// Update max property in RangeGradient shader
		filterRange.SetRangeMaterialFloat("_Max", normalizedMax);

		neighbourhood.UpdateSelectedNeighbourhoodColors(normalizedMin, normalizedMax);
		neighbourhood.UpdateBuildingActives(normalizedMin, normalizedMax);
	}

	//
	// Public Methods
	//

	//
	// Private Methods
	//

	private void InitDropdown()
	{
		infoDropdown.ClearOptions();

		List<string> infoNames = new List<string>();
		foreach (var item in neighbourhood.BuildingInfos)
		{
			infoNames.Add($"{item.name} ({item.units})");
		}

		infoDropdown.AddOptions(infoNames);
		OnOptionChanged(0);
	}

	private void UpdateFilterScale(int option)
	{
		selectedBuildingInfo = neighbourhood.BuildingInfos[option];

		// Reset slider values
		filterRange.MinSlider.value = 0.0f;
		filterRange.MaxSlider.value = 1.0f;

		// Update min max value labels
		filterRange.MinValue.text = selectedBuildingInfo.minVal.ToString("0.##");
		filterRange.MaxValue.text = selectedBuildingInfo.maxVal.ToString("0.##");

		// Update range gradient colour and min, max values
		var defaultColor = neighbourhood.DefaultColor;
		var tint = selectedBuildingInfo.color;
		filterRange.SetRangeMaterialColor("_Color1", Color.Lerp(defaultColor, tint, filterRange.MinSlider.value));
		filterRange.SetRangeMaterialColor("_Color2", Color.Lerp(defaultColor, tint, filterRange.MaxSlider.value));
		filterRange.SetRangeMaterialFloat("_Min", filterRange.MinSlider.value);
		filterRange.SetRangeMaterialFloat("_Max", filterRange.MaxSlider.value);
	}
}