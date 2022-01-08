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

public class BuildingInfo
{
	public string name;
	public string units;
	public Color color;
	public float[] values = null;
	public float maxVal;
	public float minVal;

	public Dictionary<float, int> ComputeDistribution()
	{
		int length = values.Length;
		float[] sortedValues = new float[length];
		Array.Copy(values, sortedValues, length);
		Array.Sort(sortedValues);

		var distributionValues = new Dictionary<float, int>();
		foreach (var value in sortedValues)
		{
			if (distributionValues.ContainsKey(value))
				++distributionValues[value];
			else
				distributionValues.Add(value, 1);
		}

		return distributionValues;
	}
}

public class Neighbourhood : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private InputHandler inputHandler = default;
	[SerializeField] private BorderEffect borderEffect = default;

	[Header("UI References")]
	[SerializeField] private BuildingInfoPanel biPanel = default;

	//[Header("Prefabs")]

	private MeshRenderer[] buildings = null;
	private BuildingInfo selectedBuildingInfo;

	public List<BuildingInfo> BuildingInfos { private set; get; }
	// Building properties
	public bool[] BuildingActives { private set; get; }

	private Ray ray;
	private RaycastHit hit;
	private int currBuildingIndex = -1;

	public readonly Color DefaultColor = Color.white;
	public readonly Color OutOfRangeColor = Color.grey;
	private const float InvMaxColorVal = 1.0f / 255.0f;
	private readonly string filename = $"Data{Path.DirectorySeparatorChar}BuildingInfo.csv";

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(inputHandler != null, "Neighbourhood: Missing inputHandler");
		Debug.Assert(borderEffect != null, "Neighbourhood: Missing borderEffect");
		Debug.Assert(biPanel != null, "Neighbourhood: Missing biPanel");

		buildings = Array.ConvertAll(gameObject.GetComponentsInChildren(typeof(MeshRenderer)), item => item as MeshRenderer);

		InitBuildingInfos();
		InitBuildingActives();
	}

	private void Start()
	{
		UpdateNeighbourhoodInfos(0);
		UpdateSelectedNeighbourhoodColors(0.0f, 1.0f);
		UpdateBuildingActives(0.0f, 1.0f);
	}

	private void LateUpdate()
	{
		// Only highlight 1 building at a time
		// Only show 1 building info at a time
		if (IsHoverBuilding())
		{
			// Remove old hovered building
			if (IsIndexWithinRange(currBuildingIndex, buildings.Length))
				borderEffect.Remove(buildings[currBuildingIndex]);

			// Add current hovered building
			currBuildingIndex = hit.collider.transform.GetSiblingIndex();
			if (IsIndexWithinRange(currBuildingIndex, buildings.Length))
			{
				if (BuildingActives[currBuildingIndex])
					borderEffect.Add(buildings[currBuildingIndex]);

				biPanel.SetValue(selectedBuildingInfo.values[currBuildingIndex]);
				biPanel.gameObject.SetActive(BuildingActives[currBuildingIndex]);
			}
		}
		else
		{
			if (IsIndexWithinRange(currBuildingIndex, buildings.Length))
				borderEffect.Remove(buildings[currBuildingIndex]);
			
			biPanel.gameObject.SetActive(false);
		}
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void UpdateNeighbourhoodInfos(int option)
	{
		int length = buildings.Length;
		selectedBuildingInfo = BuildingInfos[option];

		// Update building info panel
		biPanel.SetTitle(selectedBuildingInfo.name);
		biPanel.SetUnits(selectedBuildingInfo.units);
		biPanel.SetTextColor(selectedBuildingInfo.color);

		borderEffect.SetColor(selectedBuildingInfo.color);

		// Normalized buffer
		var buffer = NormalizeBuffer(selectedBuildingInfo.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			buildings[j].material.color = Color.Lerp(DefaultColor, selectedBuildingInfo.color, buffer[j]);
		}
	}

	public void UpdateSelectedNeighbourhoodColors(float normalizedMin, float normalizedMax)
	{
		int length = buildings.Length;
		// Normalized buffer
		var buffer = NormalizeBuffer(selectedBuildingInfo.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			if ((buffer[j] < normalizedMin) || (buffer[j] > normalizedMax))
				buildings[j].material.color = OutOfRangeColor;
			else
				buildings[j].material.color = Color.Lerp(DefaultColor, selectedBuildingInfo.color, buffer[j]);
		}
	}

	public void UpdateBuildingActives(float normalizedMin, float normalizedMax)
	{
		int length = buildings.Length;
		// Normalized buffer
		var buffer = NormalizeBuffer(selectedBuildingInfo.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			BuildingActives[j] = ((buffer[j] >= normalizedMin) && (buffer[j] <= normalizedMax));
		}
	}

	//
	// Private Methods
	//

	private void InitBuildingInfos()
	{
		BuildingInfos = new List<BuildingInfo>();

		using (StreamReader sr = new StreamReader(filename))
		{
			// Read and skip first row (headers)
			sr.ReadLine();

			// Read the rest of lines and initialize info properties
			while (!sr.EndOfStream)
			{
				var line = sr.ReadLine();
				var splitLine = line.Split(',');
				var length = splitLine.Length;

				var color = splitLine[2];
				var colorComponents = color.Split('-');
				var r = int.Parse(colorComponents[0]) * InvMaxColorVal;
				var g = int.Parse(colorComponents[1]) * InvMaxColorVal;
				var b = int.Parse(colorComponents[2]) * InvMaxColorVal;

				var info = new BuildingInfo
				{
					name = splitLine[0],
					units = splitLine[1],
					color = new Color(r, g, b),
				};

				int valuesLength = length - 3;
				if (valuesLength != buildings.Length)
					Debug.LogError($"Neighbourhood: Mismatch in buildings count. Check {filename}.");

				string[] valuesStr = new string[valuesLength];
				Array.Copy(splitLine, 3, valuesStr, 0, valuesLength);
				info.values = Array.ConvertAll(valuesStr, new Converter<string, float>((str) => { return float.Parse(str); }));

				BuildingInfos.Add(info);
			}
		}

		// Initialize min and max value for each info
		foreach (var info in BuildingInfos)
		{
			info.minVal = GetMinValue(info.values.ToArray());
			info.maxVal = GetMaxValue(info.values.ToArray());
		}
	}

	private void InitBuildingActives()
	{
		int length = buildings.Length;
		BuildingActives = new bool[length];
		for (int i = 0; i < length; ++i)
		{
			BuildingActives[i] = true;
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

	private bool IsIndexWithinRange(int index, int length)
	{
		return (index >= 0 && index < length);
	}

	private bool IsHoverBuilding()
	{
		Vector3 mousePos = Input.mousePosition;
		Vector3 viewportMousePos = Camera.main.ScreenToViewportPoint(mousePos);

		bool isMouseOutsideCamView = (viewportMousePos.x < 0 || viewportMousePos.x > 1) ||
									 (viewportMousePos.y < 0 || viewportMousePos.y > 1);

		if (isMouseOutsideCamView || inputHandler.IsPointerInUI)
			return false;

		// Check if mouse hovers over a building within the neighbourhood
		ray = Camera.main.ScreenPointToRay(mousePos);
		return Physics.Raycast(ray, out hit);
	}
}