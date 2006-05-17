/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace smiletray
{
	public class Ex
	{
		public static void DumpException( Exception ex )

		{
			WriteExceptionInfo( ex );                     
			if( null != ex.InnerException )               
			{                                             
				WriteExceptionInfo( ex.InnerException );    
			}
		}
		public static void WriteExceptionInfo( Exception ex )
		{
			MessageBox.Show( "--------- Exception Data ---------\n" 
				+ "Message: " + ex.Message + "\n"              
				+ "Exception Type: " + ex.GetType().FullName + "\n"
				+ "Source: " + ex.Source + "\n"               
				+ "StrackTrace: " + ex.StackTrace + "\n"         
				+ "TargetSite: " + ex.TargetSite + "\n",
				"Exception");
		}
	}
}
