using System;
using UnityEngine;

public class Climate : SingletonComponent<Climate>
{
	[Serializable]
	public class ClimateParameters
	{
		public AnimationCurve Temperature;

		[Horizontal(4, -1)]
		public Float4 AerialDensity;

		[Horizontal(4, -1)]
		public Float4 FogDensity;

		[Horizontal(4, -1)]
		public Texture2D4 LUT;
	}

	[Serializable]
	public class WeatherParameters
	{
		[Range(0f, 1f)]
		public float RainChance = 0.5f;

		[Range(0f, 1f)]
		public float FogChance = 0.5f;

		[Range(0f, 1f)]
		public float CloudChance = 0.5f;

		[Range(0f, 1f)]
		public float StormChance = 0.5f;
	}

	public struct WeatherState
	{
		public float Clouds;

		public float Fog;

		public float Wind;

		public float Rain;

		public static WeatherState Fade(WeatherState a, WeatherState b, float t)
		{
			WeatherState result = default(WeatherState);
			result.Clouds = Mathf.SmoothStep(a.Clouds, b.Clouds, t);
			result.Fog = Mathf.SmoothStep(a.Fog, b.Fog, t);
			result.Wind = Mathf.SmoothStep(a.Wind, b.Wind, t);
			result.Rain = Mathf.SmoothStep(a.Rain, b.Rain, t);
			return result;
		}

		public void Override(WeatherState other)
		{
			if (other.Clouds >= 0f)
			{
				Clouds = Mathf.Clamp01(other.Clouds);
			}
			if (other.Fog >= 0f)
			{
				Fog = Mathf.Clamp01(other.Fog);
			}
			if (other.Wind >= 0f)
			{
				Wind = Mathf.Clamp01(other.Wind);
			}
			if (other.Rain >= 0f)
			{
				Rain = Mathf.Clamp01(other.Rain);
			}
		}

		public void Max(WeatherState other)
		{
			Clouds = Mathf.Max(Clouds, other.Clouds);
			Fog = Mathf.Max(Fog, other.Fog);
			Wind = Mathf.Max(Wind, other.Wind);
			Rain = Mathf.Max(Rain, other.Rain);
		}
	}

	public class Value4<T>
	{
		public T Dawn;

		public T Noon;

		public T Dusk;

		public T Night;

		public float FindBlendParameters(TOD_Sky sky, out T src, out T dst)
		{
			float num = Mathf.Abs(sky.SunriseTime - sky.Cycle.Hour);
			float num2 = Mathf.Abs(sky.SunsetTime - sky.Cycle.Hour);
			float num3 = (180f - sky.SunZenith) / 180f;
			float num4 = 0.111111112f;
			if (num < num2)
			{
				if (num3 < 0.5f)
				{
					src = Night;
					dst = Dawn;
					return Mathf.InverseLerp(0.5f - num4, 0.5f, num3);
				}
				src = Dawn;
				dst = Noon;
				return Mathf.InverseLerp(0.5f, 0.5f + num4, num3);
			}
			if (num3 > 0.5f)
			{
				src = Noon;
				dst = Dusk;
				return Mathf.InverseLerp(0.5f + num4, 0.5f, num3);
			}
			src = Dusk;
			dst = Night;
			return Mathf.InverseLerp(0.5f, 0.5f - num4, num3);
		}
	}

	[Serializable]
	public class Float4 : Value4<float>
	{
	}

	[Serializable]
	public class Color4 : Value4<Color>
	{
	}

	[Serializable]
	public class Texture2D4 : Value4<Texture2D>
	{
	}

	private const float fadeAngle = 20f;

	private const float defaultTemp = 15f;

	private const int weatherDurationHours = 18;

	private const int weatherFadeHours = 6;

	[Range(0f, 1f)]
	public float BlendingSpeed = 1f;

	[Range(1f, 9f)]
	public float FogMultiplier = 5f;

	public float FogDarknessDistance = 200f;

	public bool DebugLUTBlending;

	public WeatherParameters Weather;

	public ClimateParameters Arid;

	public ClimateParameters Temperate;

	public ClimateParameters Tundra;

	public ClimateParameters Arctic;

	private ClimateParameters[] climates;

	private WeatherState state;

	private WeatherState clamps;

	public WeatherState Overrides;

	protected void Update()
	{
		if ((bool)TerrainMeta.BiomeMap && (bool)TOD_Sky.Instance)
		{
			TOD_Sky instance = TOD_Sky.Instance;
			long num = 36000000000L;
			long num2 = World.Seed + instance.Cycle.Ticks;
			long num3 = 18 * num;
			long num4 = 6 * num;
			long num5 = num2 / num3;
			float t = Mathf.InverseLerp(0f, num4, num2 % num3);
			WeatherState weatherState = GetWeatherState((uint)(num5 % 4294967295L));
			WeatherState weatherState2 = GetWeatherState((uint)((num5 + 1) % 4294967295L));
			state = WeatherState.Fade(weatherState, weatherState2, t);
			state.Override(Overrides);
		}
	}

	public static float GetClouds(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 0f;
		}
		return Mathf.Max(SingletonComponent<Climate>.Instance.clamps.Clouds, SingletonComponent<Climate>.Instance.state.Clouds);
	}

	public static float GetCloudOpacity(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 1f;
		}
		return Mathf.InverseLerp(0.9f, 0.8f, GetFog(position));
	}

	public static float GetFog(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 0f;
		}
		return Mathf.Max(SingletonComponent<Climate>.Instance.clamps.Fog, SingletonComponent<Climate>.Instance.state.Fog);
	}

	public static float GetWind(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 0f;
		}
		return Mathf.Max(SingletonComponent<Climate>.Instance.clamps.Wind, SingletonComponent<Climate>.Instance.state.Wind);
	}

	public static float GetRain(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 0f;
		}
		float t = TerrainMeta.BiomeMap ? TerrainMeta.BiomeMap.GetBiome(position, 1) : 0f;
		float num = TerrainMeta.BiomeMap ? TerrainMeta.BiomeMap.GetBiome(position, 8) : 0f;
		return Mathf.Max(SingletonComponent<Climate>.Instance.clamps.Rain, SingletonComponent<Climate>.Instance.state.Rain) * Mathf.Lerp(1f, 0.5f, t) * (1f - num);
	}

	public static float GetSnow(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 0f;
		}
		float num = TerrainMeta.BiomeMap ? TerrainMeta.BiomeMap.GetBiome(position, 8) : 0f;
		return Mathf.Max(SingletonComponent<Climate>.Instance.clamps.Rain, SingletonComponent<Climate>.Instance.state.Rain) * num;
	}

	public static float GetTemperature(Vector3 position)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return 15f;
		}
		if (!TOD_Sky.Instance)
		{
			return 15f;
		}
		ClimateParameters src;
		ClimateParameters dst;
		float t = SingletonComponent<Climate>.Instance.FindBlendParameters(position, out src, out dst);
		if (src == null || dst == null)
		{
			return 15f;
		}
		float hour = TOD_Sky.Instance.Cycle.Hour;
		float a = src.Temperature.Evaluate(hour);
		float b = dst.Temperature.Evaluate(hour);
		return Mathf.Lerp(a, b, t);
	}

	private WeatherState GetWeatherState(uint seed)
	{
		SeedRandom.Wanghash(ref seed);
		bool flag = SeedRandom.Value(ref seed) < Weather.CloudChance;
		bool flag2 = SeedRandom.Value(ref seed) < Weather.FogChance;
		bool flag3 = SeedRandom.Value(ref seed) < Weather.RainChance;
		bool num = SeedRandom.Value(ref seed) < Weather.StormChance;
		float num2 = flag ? SeedRandom.Value(ref seed) : 0f;
		float num3 = flag2 ? 1 : 0;
		float num4 = flag3 ? 1 : 0;
		float wind = num ? SeedRandom.Value(ref seed) : 0f;
		if (num4 > 0f)
		{
			num4 = Mathf.Max(num4, 0.5f);
			num3 = Mathf.Max(num3, num4);
			num2 = Mathf.Max(num2, num4);
		}
		WeatherState result = default(WeatherState);
		result.Clouds = num2;
		result.Fog = num3;
		result.Wind = wind;
		result.Rain = num4;
		return result;
	}

	private float FindBlendParameters(Vector3 pos, out ClimateParameters src, out ClimateParameters dst)
	{
		if (climates == null)
		{
			climates = new ClimateParameters[4]
			{
				Arid,
				Temperate,
				Tundra,
				Arctic
			};
		}
		if (TerrainMeta.BiomeMap == null)
		{
			src = null;
			dst = null;
			return 0.5f;
		}
		int biomeMaxType = TerrainMeta.BiomeMap.GetBiomeMaxType(pos);
		int biomeMaxType2 = TerrainMeta.BiomeMap.GetBiomeMaxType(pos, ~biomeMaxType);
		src = climates[TerrainBiome.TypeToIndex(biomeMaxType)];
		dst = climates[TerrainBiome.TypeToIndex(biomeMaxType2)];
		return TerrainMeta.BiomeMap.GetBiome(pos, biomeMaxType2);
	}

	public Climate()
	{
		WeatherState overrides = new WeatherState
		{
			Clouds = 0f,
			Fog = 0f,
			Wind = 0f,
			Rain = 0f
		};
		state = overrides;
		overrides = new WeatherState
		{
			Clouds = -1f,
			Fog = -1f,
			Wind = -1f,
			Rain = -1f
		};
		clamps = overrides;
		overrides = new WeatherState
		{
			Clouds = -1f,
			Fog = -1f,
			Wind = -1f,
			Rain = -1f
		};
		Overrides = overrides;
		base._002Ector();
	}
}
