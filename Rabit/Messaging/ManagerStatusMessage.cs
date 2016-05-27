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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Rabit
{

    public enum eManagerStatus
    {
        Startup,        //In the startup state
        Running,        //In the run state
        ShuttingDown,   //In the process of shutting down.
        ShutDown,       //the manager is completly shutdown.
        ErrorState      //The manager is in an un-recoverable error state.
    }
    /// <summary>
    /// A message to let other managers know what the status
    /// of the given manager is.
    /// </summary>
    public class ManagerStatusMessage : RabitMessage
    {

        public eManagerStatus ManagerStatus = eManagerStatus.Startup;

        public ManagerStatusMessage()
			:base()
        {
            Clear();
        }

        public override void Clear()
        {
            base.Clear();
            ManagerStatus = eManagerStatus.Startup;
        }

        public override void CopyMessage(RabitMessage msg)
        {
            //Ensure that all base parameters/variables are copied.
            base.CopyMessage(msg);
            //Cast Message into the Concrete Message.
            ManagerStatusMessage cmsg = (ManagerStatusMessage)msg;
            ManagerStatus = cmsg.ManagerStatus;
        }

        public override string ToString()
        {
            string msgStr = "ManagerStatusMessage:";
            msgStr = string.Concat(msgStr, "ManagerStatus", ManagerStatus.ToString());
            return msgStr;
        }
    }
}
