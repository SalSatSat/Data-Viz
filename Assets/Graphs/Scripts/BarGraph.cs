// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarGraph : Graph
{
    [SerializeField] private Image barPrefab = default;

    private const float BarWidthScale = 0.9f;

    //
    // Unity Methods
    //

    private void Awake()
    {
        CheckMissingReferences();
        Debug.Assert(barPrefab != null, "LineGraph: Missing barPrefab");
    }

    //
    // Event Methods
    //



    //
    // Public Methods
    //

    public override void CreateGraph(List<float> valueList, Color color, List<string> labelsList = null)
    {
        ClearGameObjectList();

        int valueCount = valueList.Count;
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        // Find min and max value
        for (int i = 0; i < valueCount; ++i)
        {
            float value = valueList[i];

            if (value > yMaximum)
                yMaximum = value;

            if (value < yMinimum)
                yMinimum = value;
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0.0f)
            yDifference = DefaultYDifference;

        yMaximum += (yDifference * YDifferenceOffset);   // Add offset to max value for the graph
        yMinimum = 0f;  // Start the graph at zero

        float xSize = graphWidth / (valueCount + 1);

        // Horizontal axis
        for (int i = 0; i < valueCount; ++i)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            GameObject bar = CreateBar($"Bar{i}", new Vector2(xPosition, yPosition), xSize * BarWidthScale, color);
            gameObjectList.Add(bar);

            // LabelX
            var labelX = CreateLabel(labelXPrefab, labelXContainer, $"LabelX{i}", new Vector2(xPosition, -7f), GetAxisLabelX(i, labelsList));
            gameObjectList.Add(labelX);

            // DashX
            var dashX = CreateDash(dashXPrefab, dashXContainer, $"DashX{i}", new Vector2(xPosition, 0.0f));
            gameObjectList.Add(dashX);
        }

        // Vertical axis
        for (int i = 0; i <= SeparatorCount; ++i)
        {
            // LabelY
            float normalizedValue = i * 1f / SeparatorCount;
            string labelText = GetAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            var labelY = CreateLabel(labelYPrefab, labelYContainer, $"LabelY{i}", new Vector2(-7f, normalizedValue * graphHeight), labelText);
            gameObjectList.Add(labelY);

            // DashY
            var dashY = CreateDash(dashYPrefab, dashYContainer, $"DashY{i}", new Vector2(0.0f, normalizedValue * graphHeight));
            gameObjectList.Add(dashY);
        }
    }

    //
    // Private Methods
    //

    private GameObject CreateBar(string name, Vector2 graphPosition, float barWidth, Color color)
	{
        var bar = Instantiate(barPrefab, graphContainer);
        bar.name = name;

        var barRT = bar.rectTransform;
        barRT.anchoredPosition = new Vector2(graphPosition.x, 0.0f);
        barRT.sizeDelta = new Vector2(barWidth, graphPosition.y);

        bar.color = color;

        return bar.gameObject;
	}
}