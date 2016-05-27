/***************************************************************************
=========================================================================
  								Rabit Hello World Demo
  
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
using Rabit;

namespace RabitHelloWorld
{
	public class PrintManager : RabitManager
	{
		public PrintManager ()
			: base()
		{
            this.WakeUpTimeDelayMilliseconds = 2500;
		}


		public override void ExecuteUnitOfWork ()
		{
			Console.WriteLine ("Hello World");
		}
	}


}

