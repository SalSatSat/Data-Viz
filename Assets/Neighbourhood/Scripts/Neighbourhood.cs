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
	public float[] values = null;
	public float maxVal;
	public float minVal;
}

public class Neighbourhood : MonoBehaviour
{
	//[Header("UI References")]
	//[Header("Prefabs")]

	[SerializeField] private Material buildingMat = default;
	[SerializeField] private int consumptionIndex = 0;

	private MeshRenderer[] buildings = null;
	private Consumption selectedConsumption;

	private readonly List<Consumption> consumptions = new List<Consumption>();
	public List<Consumption> Consumptions { get { return consumptions; } }
	public readonly Color DefaultColor = Color.white;
	public readonly Color OutOfRangeColor = Color.grey;
	private const float InvMaxColorVal = 1.0f / 255.0f;

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(buildingMat != null, "Neighbourhood: Missing buildingMat");

		buildings = Array.ConvertAll(gameObject.GetComponentsInChildren(typeof(MeshRenderer)), item => item as MeshRenderer);
		InitConsumptions();
	}

	private void Start()
	{
		UpdateNeighbourhoodConsumption(0);
		UpdateSelectedNeighbourhoodColors(0.0f, 1.0f);
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void UpdateNeighbourhoodConsumption(int option)
	{
		int length = buildings.Length;
		selectedConsumption = consumptions[option];
		// Normalized buffer
		var buffer = NormalizeBuffer(selectedConsumption.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			buildings[j].material.color = Color.Lerp(DefaultColor, selectedConsumption.color, buffer[j]);
		}
	}

	public void UpdateSelectedNeighbourhoodColors(float normalizedMin, float normalizedMax)
	{
		int length = buildings.Length;
		// Normalized buffer
		var buffer = NormalizeBuffer(selectedConsumption.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			if ((buffer[j] < normalizedMin) || (buffer[j] > normalizedMax))
				buildings[j].material.color = OutOfRangeColor;
			else
				buildings[j].material.color = Color.Lerp(DefaultColor, selectedConsumption.color, buffer[j]);
		}
	}

	//
	// Private Methods
	//

	private void InitConsumptions()
	{
		using (StreamReader sr = new StreamReader($"Data{Path.DirectorySeparatorChar}Consumption.csv"))
		{
			// Read and skip first row (headers)
			sr.ReadLine();

			// Read the rest of lines and initialize consumption properties
			while (!sr.EndOfStream)
			{
				var line = sr.ReadLine();
				var splitLine = line.Split(',');
				var length = splitLine.Length;

				var consumptionName = splitLine[0];
				int index = consumptionName.IndexOf('(');

				var color = splitLine[1];
				var colorComponents = color.Split('-');
				var r = int.Parse(colorComponents[0]) * InvMaxColorVal;
				var g = int.Parse(colorComponents[1]) * InvMaxColorVal;
				var b = int.Parse(colorComponents[2]) * InvMaxColorVal;

				var consumption = new Consumption
				{
					name = consumptionName.Substring(0, index - 1),
					units = consumptionName.Substring(index + 1, consumptionName.Length - index - 2),
					color = new Color(r, g, b),
				};

				int valuesLength = length - 2;
				string[] valuesStr = new string[valuesLength];
				Array.Copy(splitLine, 2, valuesStr, 0, valuesLength);
				consumption.values = Array.ConvertAll(valuesStr, new Converter<string, float>((str) => { return float.Parse(str); }));
				
				consumptions.Add(consumption);
			}
		}

		// Initialize min and max value for each consumption
		foreach (var consumption in consumptions)
		{
			consumption.minVal = GetMinValue(consumption.values.ToArray());
			consumption.maxVal = GetMaxValue(consumption.values.ToArray());
		}
	}

	private float GetMinValue(float[] array)
	{
		float minVal = array[0];
		foreach (var value in array)
		{
			if (value < minVal)
				minVal = value;
		}

		return minVal;
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

	private float[] NormalizeBuffer(float[] array)
	{
		int length = array.Length;
		var buffer = new float[length];

		var minVal = GetMinValue(array);
		var maxVal = GetMaxValue(array);
		var diffVal = maxVal - minVal;
		if (diffVal > 0.0f)
		{
			var invDiffVal = 1.0f / diffVal;
			for (int i = 0; i < length; ++i)
			{
				buffer[i] = (array[i] - minVal) * invDiffVal;
			}
		}

		return buffer;
	}
}