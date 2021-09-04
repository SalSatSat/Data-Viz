// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using System;
using UnityEngine;
using UnityEngine.UI;

public class Footer : MonoBehaviour
{
    [Header("Mapbox")]
    [SerializeField] private Button mapboxButton = default;
    [SerializeField] private Transform background = default;
    [SerializeField] private Transform mapboxPopup = default;

	[Header("Help")]
    [SerializeField] private Button helpButton = default;
    [SerializeField] private Transform helpPopup = default;
	[SerializeField] private Button helpPopupCloseButton = default;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(mapboxButton != null, "Footer: Missing mapboxButton");
		Debug.Assert(background != null, "Footer: Missing background");
		Debug.Assert(mapboxPopup != null, "Footer: Missing mapboxPopup");
		
		Debug.Assert(helpButton != null, "Footer: Missing helpButton");
		Debug.Assert(helpPopup != null, "Footer: Missing helpPopup");
		Debug.Assert(helpPopupCloseButton != null, "Footer: Missing helpPopup");

    }

    private void Start()
    {
        mapboxButton.onClick.AddListener(OnMapboxButtonClicked);
        helpButton.onClick.AddListener(OnHelpButtonClicked);
		helpPopupCloseButton.onClick.AddListener(OnCloseButtonClicked);
	}

	//
	// Event Methods
	//

	private void OnMapboxButtonClicked()
	{
		background.gameObject.SetActive(true);
		mapboxPopup.gameObject.SetActive(true);
	}

	private void OnHelpButtonClicked()
	{
		helpPopup.gameObject.SetActive(true);
	}

	private void OnCloseButtonClicked()
	{
		helpPopup.gameObject.SetActive(false);
	}

	//
	// Public Methods
	//



	//
	// Private Methods
	//



}