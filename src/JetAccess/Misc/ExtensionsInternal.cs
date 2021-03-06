﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace Jet.Misc
{
	internal static class ExtensionsInternal
	{
		#region Сommon
		public static string ToJson( this IEnumerable< string > enumerable )
		{
			var list = enumerable as IList< string > ?? enumerable.ToList();
			if( enumerable == null || list.Count == 0 )
				return PredefinedValues.EmptyJsonList;

			var values = list.Select( x => string.Format( "\"{0}\"", x ) ).ToList();
			var json = "[" + string.Join( ",", values ).ToList() + "]";
			return json;
		}

		public static string ToJson( this Dictionary< string, string > dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
				return PredefinedValues.EmptyJsonList;

			var values = dictionary.Select( x => x.ToJson() ).ToList();
			var json = "[" + string.Join( ",", values ).ToList() + "]";
			return json;
		}

		public static string ToJson( this KeyValuePair< string, string > dictionary )
		{
			var res = string.Format( "{{\"{0}\":\"{1}\"}}", dictionary.Key ?? "null", dictionary.Value ?? "null" );
			return res;
		}

		public static string ToStringUtcIso8601( this DateTime dateTime )
		{
			DateTime universalTime = dateTime.ToUniversalTime();
			string result = XmlConvert.ToString( universalTime, XmlDateTimeSerializationMode.RoundtripKind );
			return result;
		}

		public static string ToUrlParameterString( this DateTime dateTime )
		{
			string strRes = XmlConvert.ToString( dateTime, "yyyy-MM-ddTHH:mm:ss" );
			string result = strRes.Replace( "T", "%20" );
			return result;
		}

		public static string ToSoapParameterString( this DateTime dateTime )
		{
			string strRes = XmlConvert.ToString( dateTime, "yyyy-MM-ddTHH:mm:ss" );
			string result = strRes.Replace( "T", " " );
			return result;
		}

		public static DateTime ToDateTimeOrDefault( this string srcString )
		{
			try
			{
				DateTime dateTime = DateTime.Parse( srcString, CultureInfo.InvariantCulture );
				return dateTime;
			}
			catch
			{
				return default( DateTime );
			}
		}

		public static int ToIntOrDefault( this string srcString )
		{
			try
			{
				int result = int.Parse( srcString, CultureInfo.InvariantCulture );
				return result;
			}
			catch
			{
				return default( int );
			}
		}

		public static decimal ToDecimalOrDefault( this string srcString )
		{
			decimal parsedNumber;

			try
			{
				parsedNumber = decimal.Parse( srcString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture );
			}
			catch
			{
				try
				{
					parsedNumber = decimal.Parse( srcString, new NumberFormatInfo { NumberDecimalSeparator = "," } );
				}
				catch
				{
					parsedNumber = default( decimal );
				}
			}

			return parsedNumber;
		}

		public static T DeepClone< T >( this T obj )
		{
			using( var ms = new MemoryStream() )
			{
				var formstter = new BinaryFormatter();
				formstter.Serialize( ms, obj );
				ms.Position = 0;
				return ( T )formstter.Deserialize( ms );
			}
		}

		public static string BuildUrl( this IEnumerable< string > urlParrts, bool escapeUrl = false )
		{
			string resultUrl = string.Empty;
			try
			{
				resultUrl = urlParrts.Aggregate( ( ac, x ) =>
				{
					string result;

					if( !string.IsNullOrWhiteSpace( ac ) )
						ac = ac.EndsWith( "/" ) ? ac : ac + "/";

					if( !string.IsNullOrWhiteSpace( x ) )
					{
						x = x.EndsWith( "/" ) ? x : x + "/";
						x = x.StartsWith( "/" ) ? x.TrimStart( '/' ) : x;

						if( escapeUrl )
							result = string.IsNullOrWhiteSpace( ac ) ? new Uri( x ).AbsoluteUri : new Uri( new Uri( ac ), x ).AbsoluteUri;
						else
							result = string.IsNullOrWhiteSpace( ac ) ? x : string.Format( "{0}{1}", ac, x );
						// new Uri(new Uri(ac), x).AbsoluteUri;
					}
					else
					{
						if( escapeUrl )
							result = string.IsNullOrWhiteSpace( ac ) ? string.Empty : new Uri( ac ).AbsoluteUri;
						else
							result = string.IsNullOrWhiteSpace( ac ) ? string.Empty : ac;
					}

					return result;
				} );
			}
			catch
			{
			}

			return resultUrl;
		}

		public static List< List< T > > SplitToChunks< T >( this List< T > source, int chunkSize )
		{
			int i = 0;
			var chunks = new List< List< T > >();
			while( i < source.Count() )
			{
				List< T > temp = source.Skip( i ).Take( chunkSize ).ToList();
				chunks.Add( temp );
				i += chunkSize;
			}
			return chunks;
		}

		public static IEnumerable< IEnumerable< T > > Batch< T >(
			this IEnumerable< T > source, int batchSize )
		{
			using( IEnumerator< T > enumerator = source.GetEnumerator() )
			{
				while( enumerator.MoveNext() )
					yield return YieldBatchElements( enumerator, batchSize - 1 );
			}
		}

		private static IEnumerable< T > YieldBatchElements< T >(
			IEnumerator< T > source, int batchSize )
		{
			yield return source.Current;
			for( int i = 0; i < batchSize && source.MoveNext(); i++ )
			{
				yield return source.Current;
			}
		}
		#endregion
	}
}