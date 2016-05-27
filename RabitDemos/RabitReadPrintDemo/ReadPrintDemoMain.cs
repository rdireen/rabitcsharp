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
    class ReadPrintDemoMain
    {
        static void Main(string[] args)
        {

            ReadCmdLineManager ReadCmdLineMgr = new ReadCmdLineManager();
            PrintManager PrintMgr = new PrintManager();

            RabitReactor reactor = new RabitReactor();

            reactor.AddManager(ReadCmdLineMgr);
            reactor.AddManager(PrintMgr);

            reactor.Run();

        }
    }
}
