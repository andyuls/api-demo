﻿using System;
using System.Web;
using System.Web.UI;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Script.Serialization ; 


	// class for the holding oAuth Data 
	
	public class OAuthToken
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public int expires_in { get; set; }

		//for Errors
		public Boolean errorflag { get; set;}
		public OAuthError Error { get; set; }
	}

	/// <summary>
	/// O auth error.
	/// </summary>
	public class OAuthError
	{
		public string error { get; set; } // Json key - returned by PayTrace API for error
		public string error_description { get; set; } // Json key - returned by PayTrace API for error
		public string token_error_http{ get; set;} // http error 
	}
		
	public class OAuthTokenGenerator
	{
		//Keep all your settings here

		public OAuthToken GetToken()
		{
			// Those URL are available at Authentication header page.
			String Baseurl = "https://api.paytrace.com";
			String AuthURL = "/oauth/token";

			// variables for request stream and Respone reader 
			Stream dataStream = null;
			StreamReader reader = null;
			WebResponse response = null;

			//object 
			OAuthToken result =new OAuthToken();

			try
			{
				// Create a request using a URL that can receive a post. 
				WebRequest request = WebRequest.Create(Baseurl + AuthURL );

				// Set the Method property of the request to POST.
				request.Method = "POST";

				//to set HTTP version of the current request, use the Version10 and Version11 fields of the HttpVersion class.
				((HttpWebRequest)request).ProtocolVersion = HttpVersion.Version11;

				// optional - to set the Accept property, cast the WebRequest object into HttpWebRequest class
				((HttpWebRequest)request).Accept = "*/*";

				//Set the ContentType property of the WebRequest.
				request.ContentType = "application/x-www-form-urlencoded";

				// Create Request data and convert it to a byte array.
				//String requestData = "grant_type=password&username=demo123&password=demo123";
				String requestData = "grant_type=password&username=RupaApi&password=Rup@D3m0";
				byte[] byteArray = Encoding.UTF8.GetBytes (requestData);

				// Set the ContentLength property of the WebRequest.
				request.ContentLength = byteArray.Length ;

				// Get the request stream.
				dataStream = request.GetRequestStream ();

				// Write the data to the request stream.
				dataStream.Write(byteArray, 0, byteArray.Length);
				// Close the Stream object.
				dataStream.Close ();

				// To Get the response.
				response = request.GetResponse ();

				// Assuming Respose status is OK otherwise catch{} will be excuted 
				// Get the stream containing content returned by the server.
				dataStream = response.GetResponseStream ();

				// Open the stream using a StreamReader for easy access.
				reader = new StreamReader (dataStream);

				// Read the content.
				String responseFromServer = reader.ReadToEnd ();

				// Display the Response content
				result = AuthTokenData(responseFromServer);

			}
			catch(WebException e) 
			{

				// This exception will be raised if the server didn't return 200 - OK within response.
				// Retrieve more information about the error 

				result.errorflag = true;

				if (e.Response != null)
				{
					using (var responseStream = e.Response.GetResponseStream())
					{
						if (responseStream != null)
						{
							JavaScriptSerializer js = new JavaScriptSerializer ();
							String temp = (new StreamReader(responseStream)).ReadToEnd();
							//result.Error = JsonConvert.DeserializeObject<OAuthError> (temp); 
							result.Error = js.Deserialize<OAuthError>(temp);
						}
					}

					//Retrive http Error 
					HttpWebResponse err = (HttpWebResponse)e.Response;
					result.Error.token_error_http = ((int)err.StatusCode) + " " + err.StatusDescription;
				}	
				//Do your own error logging in this case
			}
			finally 
			{
				// Clean up the streams.
				if (null != reader) 
					reader.Close ();

				if (null != dataStream) 
					dataStream.Close ();

				if (null != response) 
					response.Close ();
			}

			//Do your code here
			return result;
		}

		protected OAuthToken AuthTokenData(String ResponseData)
		{
			// Create an object to parse JSON data
			OAuthToken ObjOauthToken = null;
			JavaScriptSerializer js = new JavaScriptSerializer ();
			
			if (null != ResponseData) 
			{
				// parase JSON data into C# obj
				//ObjOauthToken = JsonConvert.DeserializeObject<OAuthToken> (ResponseData);
				ObjOauthToken = js.Deserialize<OAuthToken> (ResponseData);
				ObjOauthToken.errorflag = false; //optional as by default it will be false 		
			} 
			return ObjOauthToken;
		}
	}

