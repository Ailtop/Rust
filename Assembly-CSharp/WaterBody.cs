using UnityEngine;

[ExecuteInEditMode]
public class WaterBody : MonoBehaviour
{
	public WaterBodyType Type = WaterBodyType.Lake;

	public Renderer Renderer;

	public Collider[] Triggers;

	public Transform Transform
	{
		get;
		private set;
	}

	private void Awake()
	{
		Transform = base.transform;
	}

	private void OnEnable()
	{
		WaterSystem.RegisterBody(this);
	}

	private void OnDisable()
	{
		WaterSystem.UnregisterBody(this);
	}
}
