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
    class DataMessage : RabitMessage
    {
        public string DataValue = null;

        public DataMessage()
			: base()
        {
        }

        public override void CopyMessage(RabitMessage msg)
        {
            base.CopyMessage(msg);
            DataMessage dmsg = (DataMessage)msg;
            DataValue = dmsg.DataValue;
        }
    }
}
