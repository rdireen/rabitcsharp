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
using System.Diagnostics;

namespace RabitReadPrintDemo
{
    class PrintManager : RabitManager
    {
        private DataMessage dataMsg;

        public PrintManager()
            : base()
        {
            dataMsg = new DataMessage();
            AddPublishSubscribeMessage("DataToPrint", dataMsg);
            //dataMsg.RegisterEventWithGlobalMessage(wakeUpManagerEH);
            WakeUpTimeDelaySeconds = 10.0;
		}

        public override void ExecuteUnitOfWork()
        {
            bool msgPrint = false;
            if (dataMsg.FetchMessage() && dataMsg.DataValue != null )
            {
                Console.WriteLine("Publish Subsribe Message = {0}\n", dataMsg.DataValue);
                msgPrint = true;
            }
            while (MgrMessageQueue.NoMessagesInQueue() > 0)
            {
                object msg = MgrMessageQueue.getMessage();
                if (msg is DataMessage)
                {
                    DataMessage dmsg = (DataMessage)msg;
                    Console.WriteLine("Queue Message = {0}\n", dmsg.DataValue);
                    msgPrint = true;
                }
            }
            if (!msgPrint)
            {
                Console.WriteLine("PrintManager is waiting for a Message");
            }
        }

    }
}
