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
using UnityEngine.EventSystems;

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
	[Header("References")]
	[SerializeField] private InputHandler inputHandler = default;
	[SerializeField] private BorderEffect borderEffect = default;

	[Header("UI References")]
	[SerializeField] private BuildingConsumptionPanel bcPanel = default;

	//[Header("Prefabs")]

	private MeshRenderer[] buildings = null;
	private Consumption selectedConsumption;

	public List<Consumption> Consumptions { private set; get; }
	// Building properties
	public bool[] BuildingActives { private set; get; }

	private Ray ray;
	private RaycastHit hit;
	private int currBuildingIndex = -1;
	private int prevBuildingIndex = -1;

	public readonly Color DefaultColor = Color.white;
	public readonly Color OutOfRangeColor = Color.grey;
	private const float InvMaxColorVal = 1.0f / 255.0f;

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(inputHandler != null, "Neighbourhood: Missing inputHandler");
		Debug.Assert(borderEffect != null, "Neighbourhood: Missing borderEffect");
        Debug.Assert(bcPanel != null, "Neighbourhood: Missing bcPanel");

		buildings = Array.ConvertAll(gameObject.GetComponentsInChildren(typeof(MeshRenderer)), item => item as MeshRenderer);

		InitConsumptions();
		InitBuildingActives();
	}

	private void Start()
	{
		UpdateNeighbourhoodConsumption(0);
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
				borderEffect.Add(buildings[currBuildingIndex]);

				bcPanel.SetValue(selectedConsumption.values[currBuildingIndex]);
				bcPanel.gameObject.SetActive(BuildingActives[currBuildingIndex]);
			}
		}
		else
		{
			if (IsIndexWithinRange(currBuildingIndex, buildings.Length))
				borderEffect.Remove(buildings[currBuildingIndex]);
			
			bcPanel.gameObject.SetActive(false);
		}
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
		selectedConsumption = Consumptions[option];

		// Update building consumption panel
		bcPanel.SetTitle(selectedConsumption.name);
		bcPanel.SetUnits(selectedConsumption.units);
		bcPanel.SetTextColor(selectedConsumption.color);

		borderEffect.SetColor(selectedConsumption.color);

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

	public void UpdateBuildingActives(float normalizedMin, float normalizedMax)
	{
		int length = buildings.Length;
		// Normalized buffer
		var buffer = NormalizeBuffer(selectedConsumption.values.ToArray());
		for (int j = 0; j < length; ++j)
		{
			BuildingActives[j] = ((buffer[j] >= normalizedMin) && (buffer[j] <= normalizedMax));
		}
	}

	//
	// Private Methods
	//

	private void InitConsumptions()
	{
		Consumptions = new List<Consumption>();

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
				if (valuesLength != buildings.Length)
					Debug.LogError($"Neighbourhood: Mismatch in buildings count. Check Data{Path.DirectorySeparatorChar}Consumption.csv.");
				
				string[] valuesStr = new string[valuesLength];
				Array.Copy(splitLine, 2, valuesStr, 0, valuesLength);
				consumption.values = Array.ConvertAll(valuesStr, new Converter<string, float>((str) => { return float.Parse(str); }));
				
				Consumptions.Add(consumption);
			}
		}

		// Initialize min and max value for each consumption
		foreach (var consumption in Consumptions)
		{
			consumption.minVal = GetMinValue(consumption.values.ToArray());
			consumption.maxVal = GetMaxValue(consumption.values.ToArray());
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
		{
			return false;
			//bcPanel.gameObject.SetActive(false);
			//return;
		}

		// Check if mouse hovers over a building within the neighbourhood
		ray = Camera.main.ScreenPointToRay(mousePos);
		//if (Physics.Raycast(ray, out hit))
		//{
		//	int index = hit.collider.transform.GetSiblingIndex();

		//	bcPanel.SetValue(selectedConsumption.values[index]);
		//	bcPanel.gameObject.SetActive(BuildingActives[index]);
		//}
		//else
		//{
		//	bcPanel.gameObject.SetActive(false);
		//}

		return Physics.Raycast(ray, out hit);
	}
}