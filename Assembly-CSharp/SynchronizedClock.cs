using System;
using System.Collections.Generic;

public class SynchronizedClock
{
	public struct TimedEvent
	{
		public long ticks;

		public float delta;

		public float variance;

		public Action<uint> action;
	}

	public List<TimedEvent> events = new List<TimedEvent>();

	private static long Ticks
	{
		get
		{
			if (!TOD_Sky.Instance)
			{
				return DateTime.Now.Ticks;
			}
			return TOD_Sky.Instance.Cycle.Ticks;
		}
	}

	private static float DayLengthInMinutes
	{
		get
		{
			if (!TOD_Sky.Instance)
			{
				return 30f;
			}
			return TOD_Sky.Instance.Components.Time.DayLengthInMinutes;
		}
	}

	public void Add(float delta, float variance, Action<uint> action)
	{
		TimedEvent item = default(TimedEvent);
		item.ticks = Ticks;
		item.delta = delta;
		item.variance = variance;
		item.action = action;
		events.Add(item);
	}

	public void Tick()
	{
		long num = 10000000L;
		double num2 = 1440.0 / (double)DayLengthInMinutes;
		double num3 = (double)num * num2;
		for (int i = 0; i < events.Count; i++)
		{
			TimedEvent value = events[i];
			long ticks = value.ticks;
			long ticks2 = Ticks;
			long num4 = (long)((double)value.delta * num3);
			long num5 = ticks / num4 * num4;
			uint x = (uint)(num5 % 4294967295L);
			SeedRandom.Wanghash(ref x);
			long num6 = (long)((double)SeedRandom.Range(ref x, 0f - value.variance, value.variance) * num3);
			long num7 = num5 + num4 + num6;
			if (ticks < num7 && ticks2 >= num7)
			{
				value.action(x);
				value.ticks = ticks2;
			}
			else if (ticks2 > ticks || ticks2 < num5)
			{
				value.ticks = ticks2;
			}
			events[i] = value;
		}
	}
}
