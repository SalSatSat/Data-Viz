// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Help : MonoBehaviour
{
	[Header("UI References")]
    [SerializeField] private Transform helpPopup = default;
    [SerializeField] private Button helpPopupCloseButton = default;

	//[Header("Prefabs")]

    private Button helpButton = null;

	//
	// Unity Methods
	//

	private void Awake()
	{
        Debug.Assert(helpPopup != null, "Help: Missing helpPopup");
        Debug.Assert(helpPopupCloseButton != null, "Help: Missing helpPopup");

        helpButton = GetComponent<Button>();
    }

	private void Start()
    {
        helpButton.onClick.AddListener(OnHelpButtonClicked);
        helpPopupCloseButton.onClick.AddListener(OnCloseButtonClicked);
    }

    //
    // Event Methods
    //

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