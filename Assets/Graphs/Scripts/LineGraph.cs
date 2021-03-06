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

public class LineGraph : Graph
{
	[SerializeField] private Image dotPrefab = default;
	[SerializeField] private Image dotConnectionPrefab = default;

	//
	// Unity Methods
	//

	private void Awake()
	{
        CheckMissingReferences();
        Debug.Assert(dotPrefab != null, "LineGraph: Missing dotPrefab");
        Debug.Assert(dotConnectionPrefab != null, "LineGraph: Missing dotConnectionPrefab");
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
        GameObject lastCircleGameObject = null;
        Color dotConnectionColor = color;
        dotConnectionColor.a = 0.5f;

        // Horizontal axis
        for (int i = 0; i < valueCount; ++i)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            GameObject dot = CreateDot($"Dot{i}", new Vector2(xPosition, yPosition), color);
            gameObjectList.Add(dot);

            // Connect 2 dots
            if (lastCircleGameObject != null)
            {
                var lastCircleRT = lastCircleGameObject.GetComponent<RectTransform>();
                var dotRT = dot.GetComponent<RectTransform>();

                GameObject dotConnectionGameObject = CreateDotConnection($"DotConnection{i}", lastCircleRT.anchoredPosition, dotRT.anchoredPosition, dotConnectionColor);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = dot;

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

    private float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        if (angle < 0)
            angle += 360;

        return angle;
    }

    private GameObject CreateDot(string name, Vector2 anchoredPosition, Color color)
    {
		var dot = Instantiate(dotPrefab, graphContainer);
		dot.name = name;
        dot.rectTransform.anchoredPosition = anchoredPosition;
        dot.color = color;

		return dot.gameObject;
	}

    private GameObject CreateDotConnection(string name, Vector2 dotPositionA, Vector2 dotPositionB, Color color)
    {
        var dotConnection = Instantiate(dotConnectionPrefab, graphContainer);
        dotConnection.name = name;
        dotConnection.color = color;

        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        
        dotConnection.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
        dotConnection.rectTransform.anchorMax = new Vector2(0.0f, 0.0f);
        dotConnection.rectTransform.sizeDelta = new Vector2(distance, 3.0f);
        dotConnection.rectTransform.anchoredPosition = dotPositionA + 0.5f * distance * dir;
        dotConnection.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, GetAngleFromVectorFloat(dir));
        
        return dotConnection.gameObject;
    }
}