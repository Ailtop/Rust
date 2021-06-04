using ConVar;
using UnityEngine;
using UnityEngine.UI;

public class UIRootScaled : UIRoot
{
	private static UIRootScaled Instance;

	public CanvasScaler scaler;

	public static Canvas DragOverlayCanvas => Instance.overlayCanvas;

	protected override void Awake()
	{
		Instance = this;
		base.Awake();
	}

	protected override void Refresh()
	{
		Vector2 vector = new Vector2(1280f / ConVar.Graphics.uiscale, 720f / ConVar.Graphics.uiscale);
		if (scaler.referenceResolution != vector)
		{
			scaler.referenceResolution = vector;
		}
	}
}
