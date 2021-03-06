using System;
using UnityEngine;

[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	public enum ApplicationMethod
	{
		PostEffect,
		Deferred,
		Debug
	}

	public enum PerPixelNormalSource
	{
		None,
		Camera,
		GBuffer,
		GBufferOctaEncoded
	}

	public enum SampleCountLevel
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	[Header("Ambient Occlusion")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per pass.")]
	public SampleCountLevel SampleCount = SampleCountLevel.Medium;

	public PerPixelNormalSource PerPixelNormals = PerPixelNormalSource.Camera;

	[Range(0f, 1f)]
	[Tooltip("Final applied intensity of the occlusion effect.")]
	public float Intensity = 1f;

	public Color Tint = Color.black;

	[Range(0f, 32f)]
	[Tooltip("Radius spread of the occlusion.")]
	public float Radius = 2f;

	[NonSerialized]
	[Range(32f, 1024f)]
	[Tooltip("Max sampling range in pixels.")]
	public int PixelRadiusLimit = 512;

	[NonSerialized]
	[Range(0f, 2f)]
	[Tooltip("Occlusion contribution amount on relation to radius.")]
	public float RadiusIntensity = 1f;

	[Range(0f, 16f)]
	[Tooltip("Power exponent attenuation of the occlusion.")]
	public float PowerExponent = 1.8f;

	[Range(0f, 0.99f)]
	[Tooltip("Controls the initial occlusion contribution offset.")]
	public float Bias = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("Controls the thickness occlusion contribution.")]
	public float Thickness = 1f;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample = true;

	[Tooltip("Control parameters at faraway.")]
	[Header("Distance Fade")]
	public bool FadeEnabled;

	[Tooltip("Distance in Unity unities that start to fade.")]
	public float FadeStart = 100f;

	[Tooltip("Length distance to performe the transition.")]
	public float FadeLength = 50f;

	[Tooltip("Final Intensity parameter.")]
	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint = Color.black;

	[Tooltip("Final Radius parameter.")]
	[Range(0f, 32f)]
	public float FadeToRadius = 2f;

	[Tooltip("Final PowerExponent parameter.")]
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	[Range(0f, 1f)]
	[Tooltip("Final Thickness parameter.")]
	public float FadeToThickness = 1f;

	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	[Range(1f, 4f)]
	[Tooltip("Radius in screen pixels.")]
	public int BlurRadius = 3;

	[Tooltip("Number of times that the Blur will repeat.")]
	[Range(1f, 4f)]
	public int BlurPasses = 1;

	[Range(0f, 20f)]
	[Tooltip("0 - Blured, 1 - Sharpened.")]
	public float BlurSharpness = 10f;

	[Tooltip("Accumulates the effect over the time.")]
	[Header("Temporal Filter")]
	public bool FilterEnabled = true;

	[Range(0f, 1f)]
	[Tooltip("Controls the accumulation decayment. 0 - Faster update, more flicker. 1 - Slow update (ghosting on moving objects), less flicker.")]
	public float FilterBlending = 0.5f;

	[Tooltip("Controls the discard sensibility based on the motion of the scene and objects. 0 - Discard less, reuse more (more ghost effect). 1 - Discard more, reuse less (less ghost effect).")]
	[Range(0f, 1f)]
	public float FilterResponse = 0.5f;

	[NonSerialized]
	[Tooltip("Enables directional variations.")]
	public bool TemporalDirections = true;

	[NonSerialized]
	[Tooltip("Enables offset variations.")]
	public bool TemporalOffsets = true;

	[NonSerialized]
	[Tooltip("Reduces ghosting effect near the objects's edges while moving.")]
	public bool TemporalDilation;

	[NonSerialized]
	[Tooltip("Uses the object movement information for calc new areas of occlusion.")]
	public bool UseMotionVectors = true;
}
