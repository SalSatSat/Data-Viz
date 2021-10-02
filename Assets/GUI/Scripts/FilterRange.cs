// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FilterRange : MonoBehaviour
{
	[Header("UI References")]
	public Image range;
	public Slider minSlider;
	public Slider maxSlider;
	public TMP_Text minValue;
	public TMP_Text maxValue;

	//[Header("Prefabs")]

	//
	// Unity Methods
	//

	private void Awake()
	{
		Debug.Assert(range != null, "FilterScale: Missing range");
		Debug.Assert(range.material != null, "FilterScale: Missing material for range");
		Debug.Assert(minSlider != null, "FilterScale: Missing minSlider");
		Debug.Assert(maxSlider != null, "FilterScale: Missing maxSlider");
		Debug.Assert(minValue != null, "FilterScale: Missing maxValue");
		Debug.Assert(maxValue != null, "FilterScale: Missing maxValue");
	}

	private void Start()
	{
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//



	//
	// Private Methods
	//



}