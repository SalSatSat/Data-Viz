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
using TMPro;

public abstract class Graph : MonoBehaviour
{
    [Header("UI References")]
    [Header("Graph")]
    [SerializeField] protected RectTransform graphContainer = default;
    [SerializeField] protected RectTransform labelXContainer = default;
    [SerializeField] protected RectTransform labelYContainer = default;
    [SerializeField] protected RectTransform dashXContainer = default;
    [SerializeField] protected RectTransform dashYContainer = default;
    [SerializeField] protected TMP_InputField title = default;

	[Header("Prefabs")]
    [SerializeField] protected RectTransform labelXPrefab = default;
    [SerializeField] protected RectTransform labelYPrefab = default;
    [SerializeField] protected RectTransform dashXPrefab = default;
    [SerializeField] protected RectTransform dashYPrefab = default;

    protected readonly List<GameObject> gameObjectList = new List<GameObject>();

    protected const float DefaultYDifference = 5.0f;
    protected const float YDifferenceOffset = 0.2f;
    protected const int SeparatorCount = 10;

    //
    // Unity Methods
    //

    protected void CheckMissingReferences()
    {
        Debug.Log("Graph");

        Debug.Assert(graphContainer != null, "Graph: Missing graphContainer");
        Debug.Assert(labelXContainer != null, "Graph: Missing labelXContainer");
        Debug.Assert(labelYContainer != null, "Graph: Missing labelYContainer");
        Debug.Assert(dashXContainer != null, "Graph: Missing dashXContainer");
        Debug.Assert(dashYContainer != null, "Graph: Missing dashYContainer");
        Debug.Assert(title != null, "Graph: Missing title");
        Debug.Assert(labelXPrefab != null, "Graph: Missing labelXPrefab");
        Debug.Assert(labelYPrefab != null, "Graph: Missing labelYPrefab");
        Debug.Assert(dashXPrefab != null, "Graph: Missing dashXPrefab");
        Debug.Assert(dashYPrefab != null, "Graph: Missing dashYPrefab");
    }

    //
    // Event Methods
    //



    //
    // Public Methods
    //

    public abstract void CreateGraph(List<float> valueList, Color color, List<string> labelsList = null);

    public void SetTitle(string titleText)
    {
        title.text = titleText;
    }

    //
    // Protected Methods
    //

    protected void ClearGameObjectList()
    {
        foreach (GameObject gameObject in gameObjectList)
            Destroy(gameObject);

        gameObjectList.Clear();
    }

    protected GameObject CreateLabel(RectTransform labelPrefab, RectTransform container, string name, Vector2 anchorPosition, string labelText)
    {
        RectTransform label = Instantiate(labelPrefab, container);
        label.name = name;
        label.anchoredPosition = anchorPosition;
        label.GetComponent<Text>().text = labelText;

        return label.gameObject;
    }

    protected GameObject CreateDash(RectTransform dashPrefab, RectTransform container, string name, Vector2 anchorPosition)
    {
        RectTransform dash = Instantiate(dashPrefab, container);
        dash.gameObject.name = name;
        dash.anchoredPosition = anchorPosition;

        return dash.gameObject;
    }

    protected string GetAxisLabelX(int index, List<string> labelsList = null) => (labelsList == null) ? $"{index + 1}" : labelsList[index];

    protected string GetAxisLabelY(float f) => $"{Mathf.RoundToInt(f)}";
}