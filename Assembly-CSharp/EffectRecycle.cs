using UnityEngine.Serialization;

public class EffectRecycle : BaseMonoBehaviour, IClientComponent, IRagdollInhert, IEffectRecycle
{
	public enum PlayMode
	{
		Once,
		Looped
	}

	public enum ParentDestroyBehaviour
	{
		Detach,
		Destroy,
		DetachWaitDestroy
	}

	[FormerlySerializedAs("lifeTime")]
	[ReadOnly]
	public float detachTime;

	[ReadOnly]
	[FormerlySerializedAs("lifeTime")]
	public float recycleTime;

	public PlayMode playMode;

	public ParentDestroyBehaviour onParentDestroyed;
}
