﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Jet.Misc;
using JetAccess.Misc;

namespace JetAccess.Services
{
	internal class WebRequestServices: IWebRequestServices, ICreateCallInfo
	{
		#region BaseRequests
		public async Task< WebRequest > CreatePutRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return await CreateCustomRequestAsync( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Put ).ConfigureAwait( false );
		}

		public WebRequest CreatePutRequest( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return CreateCustomRequest( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Put );
		}

		public async Task< WebRequest > CreatePatchRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return await this.CreateCustomRequestAsync( serviceUrl, body, rawHeaders, "PATCH" ).ConfigureAwait( false );
		}

		public WebRequest CreatePatchRequest( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return this.CreateCustomRequest( serviceUrl, body, rawHeaders, "PATCH" );
		}

		public async Task< WebRequest > CreateGetRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return await CreateCustomRequestAsync( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Get ).ConfigureAwait( false );
		}

		public WebRequest CreateGetRequest( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return CreateCustomRequest( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Get );
		}

		public async Task< WebRequest > CreatePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return await CreateCustomRequestAsync( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Post ).ConfigureAwait( false );
		}

		public WebRequest CreatePostRequest( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
		{
			return CreateCustomRequest( serviceUrl, body, rawHeaders, WebRequestMethods.Http.Post );
		}

		protected async Task< WebRequest > CreateCustomRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders, string method = WebRequestMethods.Http.Get )
		{
			try
			{
				if( rawHeaders == null )
					rawHeaders = new Dictionary< string, string >();

				if( body == null )
					body = string.Empty;

				var encoding = new UTF8Encoding();
				var encodedBody = encoding.GetBytes( body );

				var serviceRequest = ( HttpWebRequest )WebRequest.Create( serviceUrl );
				serviceRequest.Method = method;
				serviceRequest.ContentType = "application/json";
				serviceRequest.ContentLength = encodedBody.Length;
				serviceRequest.KeepAlive = true;

				foreach( var rawHeadersKey in rawHeaders.Keys )
				{
					serviceRequest.Headers.Add( rawHeadersKey, rawHeaders[ rawHeadersKey ] );
				}

				if( encodedBody.Length > 0 )
				{
					using( var newStream = await serviceRequest.GetRequestStreamAsync().ConfigureAwait( false ) )
						newStream.Write( encodedBody, 0, encodedBody.Length );
				}

				return serviceRequest;
			}
			catch( Exception exc )
			{
				var methodParameters = string.Format( "{{Url:\'{0}\', Body:\'{1}\', Headers:{2}}}", serviceUrl, body, rawHeaders.ToJson() );
				throw new Exception( string.Format( "Exception occured. {0}", this.CreateMethodCallInfo( methodParameters ) ), exc );
			}
		}

		protected WebRequest CreateCustomRequest( string serviceUrl, string body, Dictionary< string, string > rawHeaders, string method = WebRequestMethods.Http.Get )
		{
			try
			{
				if( rawHeaders == null )
					rawHeaders = new Dictionary< string, string >();

				if( body == null )
					body = string.Empty;

				var encoding = new UTF8Encoding();
				var encodedBody = encoding.GetBytes( body );

				var serviceRequest = ( HttpWebRequest )WebRequest.Create( serviceUrl );
				serviceRequest.Method = method;
				serviceRequest.ContentType = "application/json";
				serviceRequest.ContentLength = encodedBody.Length;
				serviceRequest.KeepAlive = true;

				foreach( var rawHeadersKey in rawHeaders.Keys )
				{
					serviceRequest.Headers.Add( rawHeadersKey, rawHeaders[ rawHeadersKey ] );
				}

				if( encodedBody.Length > 0 )
				{
					using( var newStream = serviceRequest.GetRequestStream() )
						newStream.Write( encodedBody, 0, encodedBody.Length );
				}

				return serviceRequest;
			}
			catch( Exception exc )
			{
				var methodParameters = string.Format( "{{Url:\'{0}\', Body:\'{1}\', Headers:{2}}}", serviceUrl, body, rawHeaders.ToJson() );
				throw new Exception( string.Format( "Exception occured. {0}", this.CreateMethodCallInfo( methodParameters ) ), exc );
			}
		}

		public void PopulateRequestByBody( string body, HttpWebRequest webRequest )
		{
			try
			{
				if( !string.IsNullOrWhiteSpace( body ) )
				{
					var encodedBody = new UTF8Encoding().GetBytes( body );

					webRequest.ContentLength = encodedBody.Length;
					webRequest.ContentType = "text/xml";
					var getRequestStremTask = webRequest.GetRequestStreamAsync();
					getRequestStremTask.Wait();
					using( var newStream = getRequestStremTask.Result )
						newStream.Write( encodedBody, 0, encodedBody.Length );
				}
			}
			catch( Exception exc )
			{
				var webrequestUrl = "null";

				if( webRequest != null )
				{
					if( webRequest.RequestUri != null )
					{
						if( webRequest.RequestUri.AbsoluteUri != null )
							webrequestUrl = webRequest.RequestUri.AbsoluteUri;
					}
				}

				throw new Exception( string.Format( "Exception occured on PopulateRequestByBody(body:{0}, webRequest:{1})", body ?? "null", webrequestUrl ), exc );
			}
		}
		#endregion

		#region ResponseHanding
		public Stream GetResponseStream( WebRequest webRequest )
		{
			try
			{
				using( var response = ( HttpWebResponse )webRequest.GetResponse() )
				using( var dataStream = response.GetResponseStream() )
				{
					var memoryStream = new MemoryStream();
					if( dataStream != null )
						dataStream.CopyTo( memoryStream, 0x100 );
					memoryStream.Position = 0;
					return memoryStream;
				}
			}
			catch( Exception ex )
			{
				var webrequestUrl = "null";

				if( webRequest != null )
				{
					if( webRequest.RequestUri != null )
					{
						if( webRequest.RequestUri.AbsoluteUri != null )
							webrequestUrl = webRequest.RequestUri.AbsoluteUri;
					}
				}

				throw new Exception( string.Format( "Exception occured on GetResponseStream( webRequest:{0})", webrequestUrl ), ex );
			}
		}

		public async Task< Stream > GetResponseStreamAsync( WebRequest webRequest )
		{
			try
			{
				using( var response = ( HttpWebResponse )await webRequest.GetResponseAsync().ConfigureAwait( false ) )
				using( var dataStream = await new TaskFactory< Stream >().StartNew( () => response != null ? response.GetResponseStream() : null ).ConfigureAwait( false ) )
				{
					var memoryStream = new MemoryStream();
					await dataStream.CopyToAsync( memoryStream, 0x100 ).ConfigureAwait( false );
					memoryStream.Position = 0;
					return memoryStream;
				}
			}
			catch( Exception ex )
			{
				var webrequestUrl = PredefinedValues.NotAvailable;

				if( webRequest != null )
				{
					if( webRequest.RequestUri != null )
					{
						if( webRequest.RequestUri.AbsoluteUri != null )
							webrequestUrl = webRequest.RequestUri.AbsoluteUri;
					}
				}

				throw new Exception( string.Format( "Exception occured on GetResponseStreamAsync( webRequest:{0})", webrequestUrl ), ex );
			}
		}
		#endregion
	}
}