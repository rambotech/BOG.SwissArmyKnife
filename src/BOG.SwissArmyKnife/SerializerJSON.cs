﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace BOG.SwissArmyKnife
{
	/// <summary>
	/// Provides JSON serialization/deserialization functionality, equivalent to its XML counterpart.
	///
	/// example usage:
	/// MyObject t = new MyObject();
	/// 
	/// t.MyProperty1 = this.txtTitle.Text;
	/// ....
	/// string Jsondoc = ObjectJsonSerializer&lt;MyObject&gt;.CreateDocumentFormat(t);
	/// ObjectJsonSerializer&lt;MyObject&gt;.SaveDocumentFormat(t, filename);
	/// string Jsondoc = ObjectJsonSerializer&lt;MyObject&gt;.LoadDocumentFormat(filename);
	/// t = ObjectJsonSerializer&lt;MyObject&gt;.CreateObjectFormat(Jsondoc);
	/// </summary>
	/// <typeparam name="T">Any serializable class</typeparam>
	public static class ObjectJsonSerializer<T> where T : class // Specify that T must be a class.
	{
		private const int _16M = 16 * 1024 * 1024;

		private static string ByteToString(byte[] b)
		{
			StringBuilder s = new StringBuilder();
			for (int x = 0; x < b.Length; ++x)
			{
				s.Append((char) b[x]);
			}
			return (s.ToString());
		}

		/// <summary>
		/// Creates a JSON document from an object of type T
		/// </summary>
		/// <param name="serializableObject">The object to be serialized into JSON</param>
		/// <returns>a string containing the JSON</returns>
		public static string CreateDocumentFormat(T serializableObject)
		{
			return JsonConvert.SerializeObject(serializableObject);
		}

		/// <summary>
		/// Creates an object from a string containing serialized Json of an instance of an object of that class type.
		/// </summary>
		/// <param name="json">the string containing the serialized Json</param>
		/// <returns>An object with the deserialized content</returns>
		public static T CreateObjectFormat(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>
		/// Persists JSON serialized from the object into a file.  This uses memory streams, so the serialized object
		/// can not exceeed 2Gb.
		/// </summary>
		/// <param name="serializableObject">The object to serialize</param>
		/// <param name="filename">The file in which to store the serialized content.</param>
		public static void SaveDocumentFormat(T serializableObject, string filename)
		{
			using (StreamWriter sw = File.CreateText(filename))
			{
				sw.Write(CreateDocumentFormat(serializableObject));
				sw.Close();
			}
		}

		/// <summary>
		/// Persists JSON serialized from the object into a gzip file.  This uses memory streams, so the serialized object
		/// can not exceeed 2Gb.
		/// </summary>
		/// <param name="serializableObject">The object to serialize</param>
		/// <param name="compressedFilename">The file in which to store the serialized content.</param>
		public static void SaveCompressedDocumentFormat(T serializableObject, string compressedFilename)
		{
			using (System.IO.Compression.GZipStream outGZipStream = new GZipStream(File.OpenWrite(compressedFilename), CompressionMode.Compress))
			{
				using (StreamWriter sw = new StreamWriter(outGZipStream))
				{
					sw.Write(CreateDocumentFormat(serializableObject));
					sw.Close();
				}
			}
		}

		/// <summary>
		/// Takes an object, serializes it to JSON, compresses it with GZip, encrypts it, then creates a Base64 string
		/// of its content. This string becomes the (secure) transit container for the receiver.
		/// </summary>
		/// <param name="serializableObject">The object to serialize</param>
		/// <param name="password">The password used for encryption.</param>
		/// <param name="salt">The salt used for encryption.</param>
		public static string CreateTransitContainerForObject(T serializableObject, string password, string salt)
		{
			return CreateTransitContainerForObject(serializableObject, password, salt, new AesManaged());
		}

		/// <summary>
		/// Takes an object, serializes it to JSON, compresses it with GZip, encrypts it, then creates a Base64 string
		/// of its content. This string becomes the (secure) transit container for the receiver.
		/// This overload allows a specific method to be used for the encryption.
		/// </summary>
		/// <param name="serializableObject">The object to serialize</param>
		/// <param name="password">The password used for encryption.</param>
		/// <param name="salt">The salt used for encryption.</param>
		/// <param name="algorithm">an instance of an inheriting class of SymmetricAlgorithm to do the encryption.</param>
		public static string CreateTransitContainerForObject(T serializableObject, string password, string salt, SymmetricAlgorithm algorithm)
		{
			using (MemoryStream m = new MemoryStream())
			{
				using (System.IO.Compression.GZipStream outGZipStream = new GZipStream(m, CompressionMode.Compress))
				{
					using (StreamWriter sw = new StreamWriter(outGZipStream))
					{
						sw.Write(CreateDocumentFormat(serializableObject));
					}
				}
				return new CipherUtility(algorithm).EncryptByteArray(m.ToArray(), password, salt, Base64FormattingOptions.InsertLineBreaks);
			}
		}

		/// <summary>
		/// Takes the (secure) transit container, a Base64 string, decrypts it, decompresses it with GZip (to JSON), 
		/// and returns the deserialized object.
		/// </summary>
		/// <param name="secureBase64">The string with the transit container content</param>
		/// <param name="password">The password used for encryption.</param>
		/// <param name="salt">The salt used for encryption.</param>
		public static T CreateObjectFromTransitContainer(string secureBase64, string password, string salt)
		{
			return CreateObjectFromTransitContainer(secureBase64, password, salt, new AesManaged());
		}

		/// <summary>
		/// Takes the (secure) transit container, a Base64 string, decrypts it, decompresses it with GZip (to JSON), 
		/// and returns the deserialized object.
		/// This overload allows a specific method to be used for the decryption.
		/// </summary>
		/// <param name="secureBase64">The string with the transit container content</param>
		/// <param name="password">The password used for encryption.</param>
		/// <param name="salt">The salt used for encryption.</param>
		/// <param name="algorithm">an instance of an inheriting class of SymmetricAlgorithm to do the encryption.</param>
		public static T CreateObjectFromTransitContainer(string secureBase64, string password, string salt, SymmetricAlgorithm algorithm)
		{
			T serializableObject = null;

			using (MemoryStream m = new MemoryStream(new CipherUtility(algorithm).DecryptByteArray(secureBase64, password, salt)))
			{
				using (System.IO.Compression.GZipStream inGZipStream = new GZipStream(m, CompressionMode.Decompress))
				{
					using (StreamReader sr = new StreamReader(inGZipStream))
					{
						serializableObject = CreateObjectFormat(sr.ReadToEnd());
					}
				}
				return serializableObject;
			}
		}

		/// <summary>
		/// Creates an object from a compressed file containing JSON representing an instance of an object of that class type.
		/// </summary>
		/// <param name="filename">the file containing JSON</param>
		/// <returns>An object with the deserialized content</returns>
		public static T LoadDocumentFormat(string filename)
		{
			T serializableObject = null;
			using (StreamReader o = new StreamReader(filename))
			{
				serializableObject = CreateObjectFormat(o.ReadToEnd());
				o.Close();
			}
			return serializableObject;
		}

		/// <summary>
		/// Creates an object from a gzip file, containing serialized JSON of an instance of an object of that class type.
		/// </summary>
		/// <param name="compressedFilename">the gzip file containing the serialized JSON</param>
		/// <returns>An object with the deserialized content</returns>
		public static T LoadCompressedDocumentFormat(string compressedFilename)
		{
			T serializableObject = null;
			using (GZipStream inGZipStream = new GZipStream(File.OpenRead(compressedFilename), CompressionMode.Decompress))
			{
				using (StreamReader o = new StreamReader(inGZipStream))
				{
					serializableObject = CreateObjectFormat(o.ReadToEnd());
					o.Close();
				}
			}
			return serializableObject;
		}
	}

	/// <summary>
	/// Static class for the BeautifyJSON method.
	/// </summary>
	public static class ObjectJsonUtility
	{
		/// <summary>
		/// BeautifyJSON :: make a JSON stream more human readable in a text editor.
		/// Adds line breaks after comma (,) right square bracket (]) and right squiggly bracket (}), and corrects indentation.
		/// Cleans out all unprotected white space and tabs.
		/// </summary>
		/// <param name="jsonSource">the ugly JSon, usually as one line.</param>
		/// <returns>text that's easy on the eyes.</returns>
		public static string BeautifyJSON(string jsonSource)
		{
			StringBuilder result = new StringBuilder();

			if (jsonSource.Length > 0)
			{
				bool inQuote = false;
				bool nextIsEscaped = false;
				int escapeBypass = 0;
				int indentLevel = 0;
				char prevChar = '\x00';

				for (int index = 0; index < jsonSource.Length; index++)
				{
					char thisChar = jsonSource[index];
					string prefix = string.Empty;
					string suffix = string.Empty;

					if (result.Length == 0 && (thisChar == ' ' || thisChar == '\t'))
					{
						continue;
					}
					else if (!inQuote && (thisChar == ' ' || thisChar == '\t'))
					{
						continue;
					}
					else if (!inQuote && (thisChar == '\r' || thisChar == '\n'))
					{
						continue;
					}

					if (!inQuote && thisChar == ':')
					{
						suffix = " ";  // put a space after the colon separator, which makes readability a bit better.
					}

					if (escapeBypass > 0)
					{
						if ("0123456789ABCDEF".IndexOf(char.ToUpper(thisChar)) < 0)
						{
							throw new ArgumentException("Invalid hex character in \\u##### escape sequence");
						}
						escapeBypass--;
					}
					else if (inQuote && !nextIsEscaped && thisChar == '\\')
					{
						// JSON encoding character is either "\uHHHH" or "\c", where HHHH is 4-digits of hex and c is a single character other than 'u'
						// Ref: https://tools.ietf.org/html/rfc7159#section-7
						nextIsEscaped = true;
					}
					else if (nextIsEscaped)
					{
						nextIsEscaped = false;
						if (thisChar == 'u')
						{
							escapeBypass = 4;
						}
						else
						{
							escapeBypass = 0;
							if ("\"\\/bfnrt".IndexOf(char.ToUpper(thisChar)) < 0)
							{
								throw new ArgumentException("Invalid character in single character escape (\\) sequence: must be one of { \" \\ / b f n r t }");
							}
						}
					}
					else if (thisChar == '"')
					{
						inQuote = !inQuote;
					}
					else if (!inQuote && (thisChar == '[' || thisChar == '{'))
					{
						prefix = (prevChar == ']' || prevChar == '}') ? "\r\n" : string.Empty;
						indentLevel++;
						suffix = "\r\n" + new string(' ', indentLevel * 2);
					}
					else if (!inQuote && thisChar == ',')
					{
						suffix = "\r\n" + new string(' ', indentLevel * 2);
					}
					else if (!inQuote && (thisChar == ']' || thisChar == '}'))
					{
						indentLevel--;
						prefix = (result.ToString().Length == 0 ? string.Empty : "\r\n") + new string(' ', indentLevel * 2);
					}
					if (inQuote || thisChar != ' ')
					{
						prevChar = thisChar;
					}
					result.Append(prefix);
					result.Append(thisChar);
					result.Append(suffix);
				}
			}
			return result.ToString();
		}
	}

    /// <summary>
    /// Newtonsoft serializer wrappers
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public static class Serializer<T> where T : class
    {
        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(json, Converter.Config);

        public static string ToJson(T obj) => JsonConvert.SerializeObject(obj, typeof(T), Converter.Config);
    }

    /// <summary>
    /// Standard serialization settings for Newtonsoft JSON.
    /// </summary>
    public static class Converter
    {
        public static readonly JsonSerializerSettings Config = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.DateTime,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Newtonsoft.Json.Formatting.Indented
        };
    }
}
