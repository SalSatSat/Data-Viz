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
	[SerializeField] private FilterRange filterScale = default;

	[Header("Reference")]
	[SerializeField] private Neighbourhood neighbourhood = default;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(consumptionDropdown != null, "ConsumptionsGUI: Missing consumptionDropdown");
		Debug.Assert(filterScale != null, "ConsumptionsGUI: Missing filterScale");
		Debug.Assert(neighbourhood != null, "ConsumptionsGUI: Missing neighbourhood");
	}

	private void Start()
	{
		InitDropdown();
		UpdateFilterScale(0);

		// Initilaize listeners
		consumptionDropdown.onValueChanged.AddListener(OnOptionChanged);
	}

	//
	// Event Methods
	//

	private void OnOptionChanged(int option)
	{
		neighbourhood.UpdateNeighbourhoodConsumption(option);
		UpdateFilterScale(option);
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
	}

	private void UpdateFilterScale(int option)
	{
		var consumption = neighbourhood.Consumptions[option];

		// Update min max value labels
		filterScale.minValue.text = consumption.minVal.ToString();
		filterScale.maxValue.text = consumption.maxVal.ToString();

		if (!filterScale.range.material)
			Debug.LogError("ConsumptionsGUI: Missing material for Range");

		// Update range gradient colour
		var defaultColor = neighbourhood.DefaultColor;
		var tint = consumption.color;
		filterScale.range.material?.SetColor("_Color1", Color.Lerp(defaultColor, tint, 0));
		filterScale.range.material?.SetColor("_Color2", Color.Lerp(defaultColor, tint, 1));
	}
}