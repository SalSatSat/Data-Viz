// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class BuildingConsumptionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text title = default;
    [SerializeField] private TMP_Text value = default;
    [SerializeField] private TMP_Text units = default;

    private RectTransform rectTransform;

    private readonly Vector3 Offset = new Vector3(10.0f, 10.0f, 0.0f);
    private readonly Color ColorOffset = new Color(0.5f, 0.5f, 0.5f);

    //[Header("Prefabs")]

    //
    // Unity Methods
    //

    private void Awake()
    {
        Debug.Assert(title != null, "BuildingConsumptionPanel: Missing title");
        Debug.Assert(value != null, "BuildingConsumptionPanel: Missing value");
        Debug.Assert(units != null, "BuildingConsumptionPanel: Missing units");

        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

	private void Update()
	{
		Vector3 inputPos = Input.mousePosition;
		Vector2 pos = inputPos + Offset;
        Vector2 topRightCorner = new Vector2(pos.x + rectTransform.rect.width, pos.y + rectTransform.rect.height);
        Vector3 viewportPos = Camera.main.ScreenToViewportPoint(topRightCorner);

		if (viewportPos.y > 1)
		{
			pos.y = inputPos.y - Offset.y - rectTransform.rect.height;
		}

		if (viewportPos.x > 1)
		{
			pos.x = inputPos.x - Offset.x - rectTransform.rect.width;
		}

		rectTransform.position = pos;
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void SetTitle(string consumptionType)
	{
        title.text = $"{consumptionType} Consumption";
	}

    public void SetValue(float newValue)
    {
        value.text = newValue.ToString();
    }

    public void SetUnits(string newUnits)
    {
        units.text = newUnits;
    }

    public void SetTextColor(Color color)
	{
        Color newColor = color + ColorOffset;

        title.color = newColor;
        value.color = newColor;
        units.color = newColor;
	}

    //
    // Private Methods
    //



}