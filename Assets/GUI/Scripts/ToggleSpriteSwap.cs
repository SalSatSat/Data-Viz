// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSpriteSwap : MonoBehaviour
{
	[Header("Sprites")]
	[SerializeField] private Sprite imageOn;
	[SerializeField] private Sprite imageOff;

	private Image image;

	//
	// Unity Methods
	//

	void OnEnable()
	{
		var toggle = GetComponent<Toggle>();
		image = toggle.image;

		OnToggleValueChanged(toggle.isOn);

		if (Application.isPlaying)
			toggle.onValueChanged.AddListener(OnToggleValueChanged);
	}

	void OnDisable()
	{
		GetComponent<Toggle>().onValueChanged.RemoveListener(OnToggleValueChanged);
	}


	//
	// Private Methods
	//

	private void OnToggleValueChanged(bool on)
	{
		image.sprite = on ? imageOn : imageOff;
	}
}