/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005-2006 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Text;

namespace Kudlacz.XML
{
	public class Elements : CollectionBase 
	{
		public Elements() 
		{

		}
		public void Add(Element se) 
		{
			this.List.Add(se);
		}
		public Element Item(int index) 
		{
			return (Element) this.List[index];
		}
		public Object[] ToArray() 
		{
			Object[] ar = new Object[this.List.Count];
			for (int i=0; i < this.List.Count; i++) 
			{
				ar[i] = this.List[i];
			}
			return ar;
		}
	}


	public class Element 
	{
		private String tagName;
		private String text;
		private StringDictionary attributes;
		private Elements childElements;


		public Element(String tagName) 
		{
			this.tagName = tagName;
			attributes = new StringDictionary();
			childElements = new Elements();
			this.text="";
		}

		public String TagName 
		{
			get 
			{
				return tagName;
			}
			set 
			{
				this.tagName = value;
			}
		}
		public string Text 
		{
			get 
			{
				return text;
			}
			set 
			{
				this.text = value;
			}
		}

		public Elements ChildElements 
		{
			get 
			{
				return this.childElements;
			}
		}
		public StringDictionary Attributes 
		{
			get 
			{
				return this.attributes;
			}
		}
		public String Attribute(String name) 
		{
			return (String) attributes[name];
		}

		public void setAttribute(String name, String value) 
		{
			attributes.Add(name, value);
		}
	}


	public class DOMParser
	{
		private XmlTextReader Reader;
		private Stack elements;
		private Element currentElement;
		private Element rootElement;
		public DOMParser() 
		{
			elements = new Stack();
			currentElement = null;
		}

		public Element parse(XmlTextReader reader) 
		{
			Element se = null;
			this.Reader = reader;
			while (!Reader.EOF)
			{
				Reader.Read();            
				switch (Reader.NodeType)
				{
					case XmlNodeType.Element :
						// create a new SimpleElement
						se = new Element(Reader.LocalName);
						currentElement = se;                  
						if (elements.Count == 0) 
						{
							rootElement = se;
							elements.Push(se);
						}
						else 
						{                  
							Element parent = (Element) elements.Peek();
							parent.ChildElements.Add(se);

							// don['t push empty elements onto the stack
							if (Reader.IsEmptyElement) // ends with "/>"
							{
								break;
							}
							else 
							{
								elements.Push(se);
							}
						}
						if (Reader.HasAttributes) 
						{
							while(Reader.MoveToNextAttribute()) 
							{
								currentElement.setAttribute(Reader.Name,Reader.Value);
							}
						}
						break;
					case XmlNodeType.Attribute :
						se.setAttribute(Reader.Name,Reader.Value);
						break;
					case XmlNodeType.EndElement :
						//pop the top element 
						elements.Pop();
						break;
					case XmlNodeType.Text :
						currentElement.Text=Reader.Value;
						break;
					case XmlNodeType.CDATA :
						currentElement.Text=Reader.Value;
						break;
					default :
						// ignore
						break;
				}
			}
			return rootElement;
		}
	}

}
