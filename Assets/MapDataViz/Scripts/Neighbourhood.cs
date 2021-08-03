// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class Consumption
{
	public string name;
	public string units;
	public Color color;
	public List<float> values = new List<float>();
}

public class Neighbourhood : MonoBehaviour
{
	//[Header("UI References")]
	//[Header("Prefabs")]

	[SerializeField] private List<Consumption> consumptions = new List<Consumption>();
	[SerializeField] private Material buildingMat = default;
	[SerializeField] private int consumptionIndex = 0;

	private MeshRenderer[] buildings = null;

	//
	// Unity Methods
	//

	private void Awake()
	{
		buildings = Array.ConvertAll(gameObject.GetComponentsInChildren(typeof(MeshRenderer)), item => item as MeshRenderer);
		InitConsumptions();
	}

	private void Start()
	{
		int length = buildings.Length;
		var consumption = consumptions[consumptionIndex];

		// Normalized buffer
		var buffer = NormalizeBuffer(consumption.values.ToArray(), GetMaxValue(consumption.values.ToArray()));
		for (int j = 0; j < length; ++j)
		{
			buildings[j].material.color = consumption.color * buffer[j];
		}
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//



	//
	// Private Methods
	//

	private void InitConsumptions()
	{
		using (StreamReader sr = new StreamReader($"Data{Path.DirectorySeparatorChar}Consumption.csv"))
		{
			// Read first row (consumptions)
			var consumptionNames = sr.ReadLine();
			var splitConsumptionNames = consumptionNames.Split(',');

			// Read second row (colours)
			var colours = sr.ReadLine();
			var splitColours = colours.Split(',');

			var namesLength = splitConsumptionNames.Length;
			// Create consumption and initialize properties
			for (int i = 0; i < namesLength; ++i)
			{
				int index = splitConsumptionNames[i].IndexOf('(');
				var consumption = new Consumption
				{
					name = splitConsumptionNames[i].Substring(0, index - 1),
					units = splitConsumptionNames[i].Substring(index + 1, splitConsumptionNames[i].Length - index - 2),
					color = new Color(float.Parse(splitColours[i].Split('-')[0]), float.Parse(splitColours[i].Split('-')[1]), float.Parse(splitColours[i].Split('-')[2]))
				};
				consumptions.Add(consumption);
			}

			// Initialize values buffer
			while (!sr.EndOfStream)
			{
				var values = sr.ReadLine();
				var splitValues = values.Split(',');
				var valLength = splitValues.Length;

				for (int j = 0; j < valLength; ++j)
				{
					consumptions[j].values.Add(float.Parse(splitValues[j]));
				}
			}
		}
	}

	private float GetMaxValue(float[] array)
	{
		float maxVal = array[0];
		foreach (var value in array)
		{
			if (value > maxVal)
				maxVal = value;
		}

		return maxVal;
	}

	private float[] NormalizeBuffer(float[] array, float maxVal)
	{
		int length = array.Length;
		var buffer = new float[length];

		for (int i = 0; i < length; ++i)
		{
			buffer[i] = array[i] / maxVal;
		}

		return buffer;
	}
}