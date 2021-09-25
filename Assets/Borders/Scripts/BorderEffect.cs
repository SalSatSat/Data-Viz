﻿// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)
#define DEBUG_BORDER

using System;
using System.Collections.Generic;
using UnityEngine;

public class BorderEffect : MonoBehaviour
{
	[SerializeField]
	private enum BlurSize
	{
		_3_Steps,
		_5_Steps,
		_7_Steps,
		_9_Steps,
	}

	[Header("Shader")]
	[SerializeField] private Shader idsShader = default;
	[SerializeField] private Shader postShader = default;

	[Header("Border")]
	[SerializeField] private Color color = Color.green;
	[Range(1.0f, 5.0f)]
	[SerializeField] private float intensity = 1.0f;

	[Header("Blur")]
	[SerializeField] private bool blur = true;
    [SerializeField] private BlurSize blurSize = BlurSize._9_Steps;
    [SerializeField] private float depthOffset = 0.001f;

	private Camera mainCamera;
	private Camera secondCamera;

	private RenderTexture idsRT;
	private RenderTexture borderRT;
	private RenderTexture tempRT;

	private Material postMaterial;
	private MaterialPropertyBlock properties;

	private int layerId;
	private static readonly int objectId_ShaderPropertyId = Shader.PropertyToID("_ObjectId");
	private static readonly List<Renderer> renderers = new List<Renderer>();
	private static readonly List<int> layers = new List<int>();
	private static readonly Color BorderColorOffset = new Color(0.25f, 0.25f, 0.25f);

#if DEBUG_BORDER
	public enum Stage
	{
		Final,
		Blur2,
		Blur1,
		Edges,
		ObjectIds
	}
	public enum Channel
	{
		None = 0,
		Red = 1,
		Green = 2,
		Blue = 4,
		Alpha = 8,
		All = 15,
	}

	[Header("Debug")]
	[SerializeField] private Stage stage = Stage.Final;
	[SerializeField] private Channel channels = Channel.All;
	[Range(1, 10)]
	[SerializeField] private float multiplier = 1;
	[Range(1, 10)]
	[SerializeField] private float divider = 1;
	[SerializeField]
	private Color[] colors = {
		Color.red,
		Color.green,
		Color.blue,
		Color.yellow,
		Color.magenta,
		Color.cyan,
		Color.white,
	};
#endif

	//
	// Unity Methods
	//

	private void Start()
	{
		// Create RenderTextures
		// 16 bits depth, RHalf = 16 bit floating point
		idsRT = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.RHalf);
		// 0 bit depth, Default = Default color render texture
		borderRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
		tempRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);

		// Prepare cameras
		mainCamera = GetComponent<Camera>();
		secondCamera = new GameObject("MaskCamera").AddComponent<Camera>();
		secondCamera.enabled = false;
		layerId = LayerMask.NameToLayer("Border");

		// Prepare property block
		if (properties == null)
			properties = new MaterialPropertyBlock();

		// Prepare material for post-processing
		if (postMaterial == null)
			postMaterial = new Material(postShader);

		UpdatePostKeywords();
	}

	private void OnValidate()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			if (idsShader == null)
				idsShader = Shader.Find("Custom/BorderDrawIds");
			
			if (postShader == null)
				postShader = Shader.Find("Custom/BorderPost");
		}

		if (postMaterial != null)
			UpdatePostKeywords();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// Store each renderer's current layer and change it to the border layer
		layers.Clear();
		foreach (var renderer in renderers)
		{
			layers.Add(renderer.gameObject.layer);
			renderer.gameObject.layer = layerId;
		}

		DrawBorder(source, destination);

		// Restore each renderer's layer
		for (int i = 0; i < renderers.Count; i++)
			renderers[i].gameObject.layer = layers[i];

		// Clean up
		idsRT.DiscardContents();
		borderRT.DiscardContents();
		tempRT.DiscardContents();
	}

	private void OnDestroy()
	{
		ReleaseBuffer();
	}

	//
	// Event Methods
	//



	//
	// Public Methods
	//

	public void Add(Renderer r, int colorIndex = 0)
	{
		if (renderers.Contains(r))
			return;

		renderers.Add(r);
		layers.Add(r.gameObject.layer);

		ChangeColorIndex(r, colorIndex);
	}

	public void Remove(Renderer r)
	{
		int index = renderers.IndexOf(r);
		if (index >= 0)
		{
			if (r.gameObject.layer == layerId)
				r.gameObject.layer = layers[index];

			renderers.RemoveAt(index);
			layers.RemoveAt(index);
		}
	}

	public void SetColor(Color newColor)
	{
		color = newColor + BorderColorOffset;
		if (postMaterial != null)
			postMaterial.SetColor("_BorderColor", color);
	}

	//
	// Private Methods
	//

	private void UpdatePostKeywords()
	{
		foreach (var k in postMaterial.shaderKeywords)
			if (k.StartsWith("BLUR"))
				postMaterial.DisableKeyword(k);

		postMaterial.EnableKeyword("BLUR" + System.Enum.GetName(typeof(BlurSize), blurSize).ToUpper());
	}

	private void ChangeColorIndex(Renderer r, int colorIndex)
	{
		properties.Clear();

		// Get existing properties
		if (r.HasPropertyBlock())
			r.GetPropertyBlock(properties);

		properties.SetFloat(objectId_ShaderPropertyId, colorIndex + 1);   // +1 because 0 is 'no-color'

		r.SetPropertyBlock(properties);
	}

	private void DrawBorder(RenderTexture source, RenderTexture destination)
	{
		// Clear the IDs RT (only the color buffer)
		RenderTexture activeRT = RenderTexture.active;
		RenderTexture.active = idsRT;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = activeRT;

		// Update the second camera and render into the IDs RT
		secondCamera.CopyFrom(mainCamera);
		secondCamera.clearFlags = CameraClearFlags.Nothing;
		secondCamera.cullingMask = 1 << layerId;

		// Don't use .targetTexture, it will only use the colorBuffer and not the depthBuffer!
		secondCamera.SetTargetBuffers(idsRT.colorBuffer, idsRT.depthBuffer);
		// Render objects into the RT
		secondCamera.RenderWithShader(idsShader, "");

		// Prepare post-processing parameters
		postMaterial.SetColor("_BorderColor", color);
		postMaterial.SetFloat("_Intensity", intensity);
		postMaterial.SetFloat("_DepthOffset", depthOffset);
		postMaterial.SetTexture("_IdsTex", idsRT, UnityEngine.Rendering.RenderTextureSubElement.Color);
		postMaterial.SetTexture("_IdsDepth", idsRT, UnityEngine.Rendering.RenderTextureSubElement.Depth);

#if DEBUG_BORDER
		// Set DEBUG post-processing parameters
		postMaterial.SetFloat("_Multiplier", multiplier);
		postMaterial.SetFloat("_Divider", divider);
		postMaterial.SetColor("_ColorMask", new Color(
			channels.HasFlag(Channel.Red) ? 1 : 0,
			channels.HasFlag(Channel.Green) ? 1 : 0,
			channels.HasFlag(Channel.Blue) ? 1 : 0,
			channels.HasFlag(Channel.Alpha) ? 1 : 0
		));

		if (stage == Stage.ObjectIds) { Graphics.Blit(idsRT, destination, postMaterial, 3); return; }
#endif

		// Post-process pass 1: draw edge pixels (pixels whose neightbour has a different object id)
		Graphics.Blit(idsRT, borderRT, postMaterial, 0);

#if DEBUG_BORDER
		if (stage == Stage.Edges) { Graphics.Blit(borderRT, destination, postMaterial, 3); return; }
#endif

		// Post-process pass 2: blur edge pixels in two separable passes
		if (blur)
		{
			// First blur pass (horizontal)
			postMaterial.SetVector("_BlurDirection", Vector2.right);
			Graphics.Blit(borderRT, tempRT, postMaterial, 1);
#if DEBUG_BORDER
			if (stage == Stage.Blur1) { Graphics.Blit(tempRT, destination, postMaterial, 3); return; }
#endif
			// Second blur pass (vertical)
			postMaterial.SetVector("_BlurDirection", Vector2.up);
			Graphics.Blit(tempRT, borderRT, postMaterial, 1);
#if DEBUG_BORDER
			if (stage == Stage.Blur2) { Graphics.Blit(borderRT, destination, postMaterial, 3); return; }
#endif
		}

		// Copy rendered scene to screen
		Graphics.Blit(source, destination);

		// Post-process pass 3: overlay border with object colors over rendered scene
		Graphics.Blit(borderRT, destination, postMaterial, 2);
	}

	#region Buffer

	private ComputeBuffer buffer = null;

	private void CreateBuffer(int count, int stride)
	{
		ReleaseBuffer();

		buffer = new ComputeBuffer(count, stride, ComputeBufferType.Default);
	}

	private void ReleaseBuffer()
	{
		if (buffer != null)
		{
			buffer.Release();
			buffer = null;
		}
	}

	//public void SetData(Material material, string propName, Array data, int stride)
	//{
	//	// Check if new buffer needs to be created
	//	if (buffer == null ||
	//		buffer.count != data.Length)
	//	{
	//		CreateBuffer(data.Length, stride);
	//		material.SetInt(propName + "Length", data.Length);
	//		material.SetBuffer(propName, buffer);
	//	}

	//	buffer.SetData(data);
	//}

	#endregion
}