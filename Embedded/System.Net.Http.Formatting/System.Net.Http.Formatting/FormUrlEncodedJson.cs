using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Properties;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting;

internal static class FormUrlEncodedJson
{
	private class ArrayCandidate
	{
		public int Key { get; set; }

		public string Value { get; set; }

		public ArrayCandidate(int key, string value)
		{
			Key = key;
			Value = value;
		}
	}

	private const string ApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

	private const int MinDepth = 0;

	private static readonly string[] _emptyPath = new string[1] { string.Empty };

	public static JObject Parse(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
	{
		return ParseInternal(nameValuePairs, int.MaxValue, throwOnError: true);
	}

	public static JObject Parse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth)
	{
		return ParseInternal(nameValuePairs, maxDepth, throwOnError: true);
	}

	public static bool TryParse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, out JObject value)
	{
		return (value = ParseInternal(nameValuePairs, int.MaxValue, throwOnError: false)) != null;
	}

	public static bool TryParse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth, out JObject value)
	{
		return (value = ParseInternal(nameValuePairs, maxDepth, throwOnError: false)) != null;
	}

	private static JObject ParseInternal(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth, bool throwOnError)
	{
		if (nameValuePairs == null)
		{
			throw Error.ArgumentNull("nameValuePairs");
		}
		if (maxDepth <= 0)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxDepth", maxDepth, 1);
		}
		JObject jObject = new JObject();
		foreach (KeyValuePair<string, string> nameValuePair in nameValuePairs)
		{
			string key = nameValuePair.Key;
			string value = nameValuePair.Value;
			if (key == null)
			{
				if (string.IsNullOrEmpty(value))
				{
					if (throwOnError)
					{
						throw Error.Argument("nameValuePairs", System.Net.Http.Properties.Resources.QueryStringNameShouldNotNull);
					}
					return null;
				}
				string[] path = new string[1] { value };
				if (!Insert(jObject, path, null, throwOnError))
				{
					return null;
				}
			}
			else
			{
				string[] path2 = GetPath(key, maxDepth, throwOnError);
				if (path2 == null || !Insert(jObject, path2, value, throwOnError))
				{
					return null;
				}
			}
		}
		FixContiguousArrays(jObject);
		return jObject;
	}

	private static string[] GetPath(string key, int maxDepth, bool throwOnError)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return _emptyPath;
		}
		if (!ValidateQueryString(key, throwOnError))
		{
			return null;
		}
		string[] array = key.Split('[');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].EndsWith("]", StringComparison.Ordinal))
			{
				array[i] = array[i].Substring(0, array[i].Length - 1);
			}
		}
		if (array.Length >= maxDepth)
		{
			if (throwOnError)
			{
				throw Error.Argument(System.Net.Http.Properties.Resources.MaxDepthExceeded, maxDepth);
			}
			return null;
		}
		return array;
	}

	private static bool ValidateQueryString(string key, bool throwOnError)
	{
		bool flag = false;
		for (int i = 0; i < key.Length; i++)
		{
			switch (key[i])
			{
			case '[':
				if (!flag)
				{
					flag = true;
					break;
				}
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.NestedBracketNotValid, "application/x-www-form-urlencoded", i);
				}
				return false;
			case ']':
				if (flag)
				{
					flag = false;
					break;
				}
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.UnMatchedBracketNotValid, "application/x-www-form-urlencoded", i);
				}
				return false;
			}
		}
		if (flag)
		{
			if (throwOnError)
			{
				throw Error.Argument(System.Net.Http.Properties.Resources.NestedBracketNotValid, "application/x-www-form-urlencoded", key.LastIndexOf('['));
			}
			return false;
		}
		return true;
	}

	private static bool Insert(JObject root, string[] path, string value, bool throwOnError)
	{
		JObject jObject = root;
		JObject parent = null;
		for (int i = 0; i < path.Length - 1; i++)
		{
			if (string.IsNullOrEmpty(path[i]))
			{
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.InvalidArrayInsert, BuildPathString(path, i));
				}
				return false;
			}
			if (!((IDictionary<string, JToken>)jObject).ContainsKey(path[i]))
			{
				jObject[path[i]] = new JObject();
			}
			else if (jObject[path[i]] == null || jObject[path[i]] is JValue)
			{
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, i));
				}
				return false;
			}
			parent = jObject;
			jObject = jObject[path[i]] as JObject;
		}
		if (string.IsNullOrEmpty(path[path.Length - 1]) && path.Length > 1)
		{
			if (!AddToArray(parent, path, value, throwOnError))
			{
				return false;
			}
		}
		else
		{
			if (jObject == null)
			{
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, path.Length - 1));
				}
				return false;
			}
			if (!AddToObject(jObject, path, value, throwOnError))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AddToObject(JObject obj, string[] path, string value, bool throwOnError)
	{
		int num = path.Length - 1;
		string text = path[num];
		if (((IDictionary<string, JToken>)obj).ContainsKey(text))
		{
			if (obj[text] == null || obj[text].Type == JTokenType.Null)
			{
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, num));
				}
				return false;
			}
			if (path.Length != 1)
			{
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.JQuery13CompatModeNotSupportNestedJson, BuildPathString(path, num));
				}
				return false;
			}
			if (obj[text].Type == JTokenType.String)
			{
				string text2 = obj[text].ToObject<string>();
				JObject jObject = new JObject();
				jObject.Add("0", text2);
				jObject.Add("1", value);
				obj[text] = jObject;
			}
			else if (obj[text] is JObject)
			{
				JObject jObject2 = obj[text] as JObject;
				string index = GetIndex(jObject2, throwOnError);
				if (index == null)
				{
					return false;
				}
				jObject2.Add(index, value);
			}
		}
		else if (value == null)
		{
			obj[text] = null;
		}
		else
		{
			obj[text] = value;
		}
		return true;
	}

	private static bool AddToArray(JObject parent, string[] path, string value, bool throwOnError)
	{
		string propertyName = path[path.Length - 2];
		if (!(parent[propertyName] is JObject jObject))
		{
			if (throwOnError)
			{
				throw Error.Argument(System.Net.Http.Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, path.Length - 1));
			}
			return false;
		}
		string index = GetIndex(jObject, throwOnError);
		if (index == null)
		{
			return false;
		}
		jObject.Add(index, value);
		return true;
	}

	private static string GetIndex(JObject jsonObject, bool throwOnError)
	{
		int num = -1;
		if (jsonObject.Count > 0)
		{
			foreach (string key in ((IDictionary<string, JToken>)jsonObject).Keys)
			{
				if (int.TryParse(key, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result) && result > num)
				{
					num = result;
					continue;
				}
				if (throwOnError)
				{
					throw Error.Argument(System.Net.Http.Properties.Resources.FormUrlEncodedMismatchingTypes, key);
				}
				return null;
			}
		}
		return (num + 1).ToString(CultureInfo.InvariantCulture);
	}

	private static void FixContiguousArrays(JToken jv)
	{
		if (jv is JArray jArray)
		{
			for (int i = 0; i < jArray.Count; i++)
			{
				if (jArray[i] != null)
				{
					jArray[i] = FixSingleContiguousArray(jArray[i]);
					FixContiguousArrays(jArray[i]);
				}
			}
		}
		else
		{
			if (!(jv is JObject { Count: >0 } jObject))
			{
				return;
			}
			foreach (string item in new List<string>(((IDictionary<string, JToken>)jObject).Keys))
			{
				if (jObject[item] != null)
				{
					jObject[item] = FixSingleContiguousArray(jObject[item]);
					FixContiguousArrays(jObject[item]);
				}
			}
		}
	}

	private static JToken FixSingleContiguousArray(JToken original)
	{
		if (original is JObject { Count: >0 } jObject && CanBecomeArray(new List<string>(((IDictionary<string, JToken>)jObject).Keys), out var sortedKeys))
		{
			JArray jArray = new JArray();
			{
				foreach (string item in sortedKeys)
				{
					jArray.Add(jObject[item]);
				}
				return jArray;
			}
		}
		return original;
	}

	private static bool CanBecomeArray(List<string> keys, out List<string> sortedKeys)
	{
		List<ArrayCandidate> list = new List<ArrayCandidate>();
		sortedKeys = null;
		bool flag = true;
		foreach (string key in keys)
		{
			if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
			{
				flag = false;
				break;
			}
			string text = result.ToString(CultureInfo.InvariantCulture);
			if (!text.Equals(key, StringComparison.Ordinal))
			{
				flag = false;
				break;
			}
			list.Add(new ArrayCandidate(result, text));
		}
		if (flag)
		{
			list.Sort((ArrayCandidate x, ArrayCandidate y) => x.Key - y.Key);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Key != i)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			sortedKeys = new List<string>(list.Select((ArrayCandidate x) => x.Value));
		}
		return flag;
	}

	private static string BuildPathString(string[] path, int i)
	{
		StringBuilder stringBuilder = new StringBuilder(path[0]);
		for (int j = 1; j <= i; j++)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", new object[1] { path[j] });
		}
		return stringBuilder.ToString();
	}
}
