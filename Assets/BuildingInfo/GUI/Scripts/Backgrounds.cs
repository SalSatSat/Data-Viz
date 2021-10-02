// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System;
using UnityEngine;
using TMPro;
using Mapbox.Unity.Map;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Backgrounds : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Toggle backgroundToggle = default;
    [SerializeField] private GameObject backgroundPanel = default;
    [SerializeField] private TMP_Dropdown backgroundDropdown = default;

    [Header("References")]
    [SerializeField] private AbstractMap abstractMap = null;

    private readonly string DefaultMapOption = ImagerySourceType.MapboxDark.ToString();

    //[Header("Prefabs")]


	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(backgroundToggle != null, "Backgrounds: Missing backgroundToggle");
		Debug.Assert(backgroundPanel != null, "Backgrounds: Missing backgroundPanel");
		Debug.Assert(backgroundDropdown != null, "Backgrounds: Missing backgroundDropdown");
        Debug.Assert(abstractMap != null, "Backgrounds: Missing abstractMap");
    }

    private void Start()
    {
        InitDropdown();

        // Initialize listeners
        backgroundToggle.onValueChanged.AddListener(OnToggleChanged);
        backgroundDropdown.onValueChanged.AddListener(OnOptionChanged);
    }

    //
    // Event Methods
    //

    private void OnToggleChanged(bool isOn)
    {
        backgroundPanel.SetActive(isOn);
    }

    private void OnOptionChanged(int option)
    {
        abstractMap.ImageLayer.SetLayerSource((ImagerySourceType)option);
    }

    //
    // Public Methods
    //



    //
    // Private Methods
    //

    private void InitDropdown()
    {
        backgroundDropdown.ClearOptions();

        string[] backgroundTypes = new string[6];
        Array.Copy(Enum.GetNames(typeof(ImagerySourceType)), backgroundTypes, 6);   // Skip Custom and None types
        List<string> options = new List<string>();

        if (backgroundTypes == null)
		{
            Debug.LogError("Backgrounds: Failed to initialize backgroundDropdown");
            return;
		}

        foreach (var type in backgroundTypes)
        {
            string option = type.Contains("Mapbox") ? type.Replace("Mapbox", "") : type;
            options.Add(option);
		}

        backgroundDropdown.AddOptions(options);
        backgroundDropdown.value = options.FindIndex(option => DefaultMapOption.Contains(option));
    }
}