#define UNITY_ASSERTIONS
using ConVar;
using Facepunch.Sqlite;
using Ionic.Crc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class FileStorage : IDisposable
{
	private class CacheData
	{
		public byte[] data;

		public uint entityID;

		public uint numID;
	}

	public enum Type
	{
		png,
		jpg
	}

	private Database db;

	private CRC32 crc = new CRC32();

	private Dictionary<uint, CacheData> _cache = new Dictionary<uint, CacheData>();

	public static FileStorage server = new FileStorage("sv.files." + 198, true);

	protected FileStorage(string name, bool server)
	{
		if (server)
		{
			string path = Server.rootFolder + "/" + name + ".db";
			db = new Database();
			db.Open(path);
			if (!db.TableExists("data"))
			{
				db.Execute("CREATE TABLE data ( crc INTEGER PRIMARY KEY, data BLOB, updated INTEGER, entid INTEGER, filetype INTEGER, part INTEGER )");
				db.Execute("CREATE INDEX IF NOT EXISTS entindex ON data ( entid )");
			}
		}
	}

	~FileStorage()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (db != null)
		{
			db.Close();
			db = null;
		}
	}

	private uint GetCRC(byte[] data, Type type)
	{
		crc.Reset();
		crc.SlurpBlock(data, 0, data.Length);
		crc.UpdateCRC((byte)type);
		return (uint)crc.Crc32Result;
	}

	public uint Store(byte[] data, Type type, uint entityID, uint numID = 0u)
	{
		using (TimeWarning.New("FileStorage.Store"))
		{
			uint cRC = GetCRC(data, type);
			if (db != null)
			{
				db.Execute("INSERT OR REPLACE INTO data ( crc, data, entid, filetype, part ) VALUES ( ?, ?, ?, ?, ? )", (int)cRC, data, (int)entityID, (int)type, (int)numID);
			}
			_cache.Remove(cRC);
			_cache.Add(cRC, new CacheData
			{
				data = data,
				entityID = entityID,
				numID = numID
			});
			return cRC;
		}
	}

	public byte[] Get(uint crc, Type type, uint entityID)
	{
		using (TimeWarning.New("FileStorage.Get"))
		{
			CacheData value;
			if (_cache.TryGetValue(crc, out value))
			{
				Assert.IsTrue(value.data != null, "FileStorage cache contains a null texture");
				return value.data;
			}
			if (db == null)
			{
				return null;
			}
			byte[] array = db.QueryBlob("SELECT data FROM data WHERE crc = ? AND filetype = ? AND entid = ? LIMIT 1", (int)crc, (int)type, (int)entityID);
			if (array == null)
			{
				return null;
			}
			_cache.Remove(crc);
			_cache.Add(crc, new CacheData
			{
				data = array,
				entityID = entityID,
				numID = 0u
			});
			return array;
		}
	}

	public void Remove(uint crc, Type type, uint entityID)
	{
		using (TimeWarning.New("FileStorage.Remove"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE crc = ? AND filetype = ? AND entid = ?", (int)crc, (int)type, (int)entityID);
			}
			if (_cache.ContainsKey(crc))
			{
				_cache.Remove(crc);
			}
		}
	}

	public void RemoveEntityNum(uint entityid, uint numid)
	{
		using (TimeWarning.New("FileStorage.RemoveEntityNum"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE entid = ? AND part = ?", (int)entityid, (int)numid);
			}
			uint[] array = (from x in _cache
				where x.Value.entityID == entityid && x.Value.numID == numid
				select x.Key).ToArray();
			foreach (uint key in array)
			{
				_cache.Remove(key);
			}
		}
	}

	internal void RemoveAllByEntity(uint entityid)
	{
		using (TimeWarning.New("FileStorage.RemoveAllByEntity"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE entid = ?", (int)entityid);
			}
		}
	}
}