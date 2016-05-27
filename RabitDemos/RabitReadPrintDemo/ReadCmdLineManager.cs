/***************************************************************************
=========================================================================
  								Rabit Read Print Demo
  
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

namespace RabitReadPrintDemo
{
    class ReadCmdLineManager : RabitManager
    {
        private DataMessage dataMessage;

        public ReadCmdLineManager()
            : base()
        {
            dataMessage = new DataMessage();
            AddPublishSubscribeMessage("DataToPrint", dataMessage);
            WakeUpTimeDelaySeconds = 0;
        }

        public override void ExecuteUnitOfWork()
        {
            Console.Write("Enter Command: ");
            string cmdLine = Console.ReadLine();
			if (cmdLine != null)
			{
				Console.WriteLine ("\n");

				dataMessage.DataValue = cmdLine;

				string cmdLineLC = cmdLine.Trim ().ToLower ();
				if (cmdLineLC.StartsWith ("quit") || cmdLineLC.StartsWith ("exit"))
				{
					this.MgrControl.ShutDownAllManagers = true;
					MgrControl.PostMessage ();
				} else if (cmdLineLC.Contains ("queue"))
				{
					AddMessageToQueue ("PrintManager", dataMessage.Clone ());
				} else
				{
					dataMessage.PostMessage ();
				}
			}
        }

    }
}
