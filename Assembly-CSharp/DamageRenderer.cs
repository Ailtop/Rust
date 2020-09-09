using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageRenderer : MonoBehaviour, IClientComponent
{
	[Serializable]
	private struct DamageShowingRenderer
	{
		public Renderer renderer;

		public int[] indices;

		public DamageShowingRenderer(Renderer renderer, int[] indices)
		{
			this.renderer = renderer;
			this.indices = indices;
		}
	}

	[SerializeField]
	private List<Material> damageShowingMats;

	[HideInInspector]
	[SerializeField]
	private List<DamageShowingRenderer> damageShowingRenderers;

	[HideInInspector]
	[SerializeField]
	private List<GlassPane> damageShowingGlassRenderers;
}