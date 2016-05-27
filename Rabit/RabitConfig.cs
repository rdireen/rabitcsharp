/***************************************************************************
=========================================================================
  								Rabit
  
DireenTech Inc. 
www.DireenTech.com

Develpers:
	Harry Direen PhD

Start Date:  August, 2015
=========================================================================
               Copyright (c) 2015 DireenTech Inc.
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
=========================================================================
****************************************************************************/
using System;


namespace Rabit
{
	/// <summary>
	/// Rabit Globals... 
	/// A Static Class if Global Values.
	/// </summary>
	public class RabitGlobals
	{
		#if LINUX
		public static string DirSeparator = "/";
		#else
		public static string DirSeparator = "\\";
		#endif


	}

	/// <summary>
	/// Rabit Config
	/// Configureation Parameters
	/// </summary>
	public class RabitConfig
	{
		public RabitConfig ()
		{


		}
	}
}

