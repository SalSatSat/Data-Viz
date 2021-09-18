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

public class ConsumptionsGUI : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private TMP_Dropdown consumptionDropdown = default;
	[SerializeField] private FilterRange filterRange = default;

	[Header("Reference")]
	[SerializeField] private Neighbourhood neighbourhood = default;

	private Consumption selectedConsumption = null;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(consumptionDropdown != null, "ConsumptionsGUI: Missing consumptionDropdown");
		Debug.Assert(filterRange != null, "ConsumptionsGUI: Missing filterRange");
		Debug.Assert(neighbourhood != null, "ConsumptionsGUI: Missing neighbourhood");
	}

	private void Start()
	{
		InitDropdown();
		UpdateFilterScale(0);

		// Initialize color in range shader
		filterRange.range.material.SetColor("_Color3", neighbourhood.OutOfRangeColor);

		// Initilaize listeners
		consumptionDropdown.onValueChanged.AddListener(OnOptionChanged);
		filterRange.minSlider.onValueChanged.AddListener(OnMinValueChanged);
		filterRange.maxSlider.onValueChanged.AddListener(OnMaxValueChanged);
	}

	private void OnApplicationQuit()
	{
		// Initilaize listeners
		consumptionDropdown.onValueChanged.RemoveListener(OnOptionChanged);
		filterRange.minSlider.onValueChanged.RemoveListener(OnMinValueChanged);
		filterRange.maxSlider.onValueChanged.RemoveListener(OnMaxValueChanged);

		UpdateFilterScale(0);
	}

	//
	// Event Methods
	//

	private void OnOptionChanged(int option)
	{
		neighbourhood.UpdateNeighbourhoodConsumption(option);
		UpdateFilterScale(option);
	}

	private void OnMinValueChanged(float normalizedMin)
	{
		float normalizedMax = filterRange.maxSlider.value;

		// Ensure min slider does not go past max slider
		if (normalizedMin > normalizedMax)
			filterRange.minSlider.value = normalizedMax;

		// Update min value label
		float minValue = (normalizedMin * (selectedConsumption.maxVal - selectedConsumption.minVal) + selectedConsumption.minVal);
		filterRange.minValue.text = minValue.ToString("0.##");

		// Update min property in RangeGradient shader
		if (filterRange.range.material != null)
			filterRange.range.material.SetFloat("_Min", normalizedMin);

		neighbourhood.UpdateSelectedNeighbourhoodColors(normalizedMin, normalizedMax);
		neighbourhood.UpdateBuildingActives(normalizedMin, normalizedMax);
	}

	private void OnMaxValueChanged(float normalizedMax)
	{
		float normalizedMin = filterRange.minSlider.value;

		// Ensure max slider does not go past min slider
		if (normalizedMax < normalizedMin)
			filterRange.maxSlider.value = normalizedMin;

		// Update max value label
		float maxValue = (normalizedMax * (selectedConsumption.maxVal - selectedConsumption.minVal) + selectedConsumption.minVal);
		filterRange.maxValue.text = maxValue.ToString("0.##");

		// Update max property in RangeGradient shader
		if (filterRange.range.material != null)
			filterRange.range.material.SetFloat("_Max", normalizedMax);

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
		consumptionDropdown.ClearOptions();

		List<string> consumptionNames = new List<string>();
		foreach (var item in neighbourhood.Consumptions)
		{
			consumptionNames.Add($"{item.name} ({item.units})");
		}

		consumptionDropdown.AddOptions(consumptionNames);
		OnOptionChanged(0);
	}

	private void UpdateFilterScale(int option)
	{
		selectedConsumption = neighbourhood.Consumptions[option];

		// Reset slider values
		filterRange.minSlider.value = 0.0f;
		filterRange.maxSlider.value = 1.0f;

		// Update min max value labels
		filterRange.minValue.text = selectedConsumption.minVal.ToString("0.##");
		filterRange.maxValue.text = selectedConsumption.maxVal.ToString("0.##");

		// Update range gradient colour and min, max values
		var defaultColor = neighbourhood.DefaultColor;
		var tint = selectedConsumption.color;
		filterRange.range.material.SetColor("_Color1", Color.Lerp(defaultColor, tint, filterRange.minSlider.value));
		filterRange.range.material.SetColor("_Color2", Color.Lerp(defaultColor, tint, filterRange.maxSlider.value));
		filterRange.range.material.SetFloat("_Min", filterRange.minSlider.value);
		filterRange.range.material.SetFloat("_Max", filterRange.maxSlider.value);
	}
}