using System;
using UnityEngine;

public class AIThinkManager : BaseMonoBehaviour, IServerComponent
{
	public static ListHashSet<IThinker> _processQueue = new ListHashSet<IThinker>();

	public static ListHashSet<IThinker> _removalQueue = new ListHashSet<IThinker>();

	[Help("How many miliseconds to budget for processing AI entities per server frame")]
	[ServerVar]
	public static float framebudgetms = 2.5f;

	private static int lastIndex = 0;

	public static void ProcessQueue()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = framebudgetms / 1000f;
		if (_removalQueue.Count > 0)
		{
			foreach (IThinker item in _removalQueue)
			{
				_processQueue.Remove(item);
			}
			_removalQueue.Clear();
		}
		AIInformationZone.BudgetedTick();
		while (lastIndex < _processQueue.Count && Time.realtimeSinceStartup < realtimeSinceStartup + num)
		{
			IThinker thinker = _processQueue[lastIndex];
			if (thinker != null)
			{
				try
				{
					thinker.TryThink();
				}
				catch (Exception message)
				{
					Debug.LogWarning(message);
				}
			}
			lastIndex++;
		}
		if (lastIndex >= _processQueue.Count)
		{
			lastIndex = 0;
		}
	}

	public static void Add(IThinker toAdd)
	{
		_processQueue.Add(toAdd);
	}

	public static void Remove(IThinker toRemove)
	{
		_removalQueue.Add(toRemove);
	}
}
