/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005-2006 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using Kudlacz.XML;
using System.Windows.Forms;
using smiletray;

namespace Kudlacz.Web
{
	public enum Error
	{
		NotFoundInTable = -4,
		StringMissMatch = -3,
		ParseError = -2,
		ServerError = -1,
		Fail = 0,
		OK = 1,
		NoNewVersion = 2,
		NewVersion = 3,

	}
	public class VersionCheck
	{
		public class VersionData
		{
			public String name;
			public String description;
			public String version;
			public String intversion;
			public String date;
			public String time;
			public String downloadurl;
			public String fileurl;
			public String comments;
			public String hash;
		}

		public string URL;
		private string Class;
		private string data;
		public VersionData vd;
		public int timeout; 

		public VersionCheck(string URL, string Class)
		{
			this.URL = URL;
			this.Class  = Class;
			timeout = 30000;
			vd = new VersionData();
		}

		public Error GetData()
		{
			string file;


			HttpWebResponse response = null;
			StreamReader streamreader = null;
			try
			{
				// Load Data from URL
				HttpWebRequest Client = (HttpWebRequest)WebRequest.Create(URL);
				Client.Timeout = 30000;
				response = (HttpWebResponse)Client.GetResponse();
				streamreader = new StreamReader(response.GetResponseStream());
				file = streamreader.ReadToEnd();

				// If the data is the same as stored data then return what we already have
				if(data != null && data.GetHashCode() == file.GetHashCode())
					return Error.OK;

				data = file;
			}
			catch
			{
				return Error.ServerError;
			}
			finally
			{
				if(streamreader != null) streamreader.Close();
				if(response != null) response.Close();
			}


			StringReader strreader = null;
			XmlTextReader xmlreader = null;
			Element e;
			try
			{
				strreader = new StringReader(data);
				xmlreader = new XmlTextReader(strreader);
				
				// Parse XML Data string
				strreader = new StringReader(data);
				xmlreader = new XmlTextReader(strreader);
				DOMParser dp = new DOMParser();
				e = dp.parse(xmlreader);
			}
			catch
			{
				return Error.ParseError;
			}
			finally
			{
				if(xmlreader != null) xmlreader.Close();
				if(strreader != null) strreader.Close();
			}

			try
			{

				// Find Class object we want
				Element c = null;
				foreach(Element ce in e.ChildElements)
				{
					if(String.Compare(ce.TagName, Class) == 0)
					{
						c = ce;
						break;
					}
				}
				if(c == null)
					return Error.NotFoundInTable;

				foreach(Element ce in c.ChildElements)
				{
					switch(ce.TagName)
					{
						case "name":
							vd.name = ce.Text;
							break;
						case "description":
							vd.description = ce.Text;
							break;
						case "intversion":
							vd.intversion = ce.Text;
							break;
						case "version":
							vd.version = ce.Text;
							break;
						case "date":
							vd.date = ce.Text;
							break;
						case "time":
							vd.time = ce.Text;
							break;
						case "downloadurl":
							vd.downloadurl = ce.Text;
							break;
						case "fileurl":
							vd.fileurl = ce.Text;
							break;
						case "hash":
							vd.hash = ce.Text;
							break;
						case "comments":
							vd.comments = ce.Text;
							break;
					}
				}
				return Error.OK;
			}
			catch
			{
				return Error.Fail;
			}
		}

		public Error Check(string version)
		{
			Error err;
			if((int)(err = GetData()) <= 0)
				return err;

			String [] split_ver = version.Split('.'); 
			String [] split_sver = vd.intversion.Split('.');
			
			if(split_ver.Length != split_sver.Length)
				return Error.StringMissMatch;

			for(int i = 0; i < split_ver.Length; i++)
			{
				try
				{
                    if (int.Parse(split_ver[i]) < int.Parse(split_sver[i]))
                        return Error.NewVersion;
                    else if (int.Parse(split_ver[i]) > int.Parse(split_sver[i]))
                        break;
                    else
                        continue;
				}
				catch
				{
						return Error.Fail;
				}
			}

			return Error.NoNewVersion;
		}
	}
}
