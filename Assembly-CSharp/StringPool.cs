using System;
using System.Collections.Generic;
using UnityEngine;

public class StringPool
{
	public static Dictionary<uint, string> toString;

	public static Dictionary<string, uint> toNumber;

	private static bool initialized;

	public static uint closest;

	private static void Init()
	{
		if (!initialized)
		{
			toString = new Dictionary<uint, string>();
			toNumber = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
			GameManifest gameManifest = FileSystem.Load<GameManifest>("Assets/manifest.asset");
			for (uint num = 0u; num < gameManifest.pooledStrings.Length; num++)
			{
				toString.Add(gameManifest.pooledStrings[num].hash, gameManifest.pooledStrings[num].str);
				toNumber.Add(gameManifest.pooledStrings[num].str, gameManifest.pooledStrings[num].hash);
			}
			initialized = true;
			closest = Get("closest");
		}
	}

	public static string Get(uint i)
	{
		if (i == 0)
		{
			return string.Empty;
		}
		Init();
		string value;
		if (toString.TryGetValue(i, out value))
		{
			return value;
		}
		Debug.LogWarning("StringPool.GetString - no string for ID" + i);
		return "";
	}

	public static uint Get(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0u;
		}
		Init();
		uint value;
		if (toNumber.TryGetValue(str, out value))
		{
			return value;
		}
		Debug.LogWarning("StringPool.GetNumber - no number for string " + str);
		return 0u;
	}

	public static uint Add(string str)
	{
		uint value = 0u;
		if (!toNumber.TryGetValue(str, out value))
		{
			value = str.ManifestHash();
			toString.Add(value, str);
			toNumber.Add(str, value);
		}
		return value;
	}
}
