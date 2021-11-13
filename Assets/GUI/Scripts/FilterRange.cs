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
	[SerializeField] private Image range = default;
	[SerializeField] private Slider minSlider = default;
	[SerializeField] private Slider maxSlider = default;
	[SerializeField] private TMP_Text minValue = default;
	[SerializeField] private TMP_Text maxValue = default;

	public Image Range { get { return range; } }
	public Slider MinSlider { get { return minSlider; } }
	public Slider MaxSlider { get { return maxSlider; } }
	public TMP_Text MinValue { get { return minValue; } }
	public TMP_Text MaxValue { get { return maxValue; } }

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

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void SetMinColor(Color value)
	{
		range.material.SetColor("MinColor", value);
	}

	public void SetMaxColor(Color value)
	{
		range.material.SetColor("MaxColor", value);
	}

	public void SetOutOfRangeColor(Color value)
	{
		range.material.SetColor("OutOfRangeColor", value);
	}

	public void SetMinFilter(float min)
	{
		range.material.SetFloat("MinFilter", min);
	}

	public void SetMaxFilter(float max)
	{
		range.material.SetFloat("MaxFilter", max);
	}

	//
	// Private Methods
	//



}