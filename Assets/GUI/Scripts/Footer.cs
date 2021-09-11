// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using UnityEngine.UI;

public class Footer : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Button mapboxButton = default;
    [SerializeField] private Transform background = default;
    [SerializeField] private Transform mapboxPopup = default;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(mapboxButton != null, "Footer: Missing mapboxButton");
		Debug.Assert(background != null, "Footer: Missing background");
		Debug.Assert(mapboxPopup != null, "Footer: Missing mapboxPopup");
    }

    private void Start()
    {
        mapboxButton.onClick.AddListener(OnMapboxButtonClicked);
	}

	//
	// Event Methods
	//

	private void OnMapboxButtonClicked()
	{
		background.gameObject.SetActive(true);
		mapboxPopup.gameObject.SetActive(true);
	}

	//
	// Public Methods
	//



	//
	// Private Methods
	//



}