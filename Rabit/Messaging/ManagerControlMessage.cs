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

    /// <summary>
    /// A Message to provide Syncronized Control between all managers.
    /// In general there will be one manager that has the top-level control 
    /// of all the managers.  That manager will be responsible for monitoring
    /// the other managers and setting the Run State and Shutdown.
    /// All Managers are setup to trigger whenever this message is 
    /// published.
    /// </summary>
    public class ManagerControlMessage : RabitMessage
    {
        /// <summary>
        /// The Main Top-Level Manager (or any other manager) will set this 
        /// flag and post the message to cause all manager to shutdown.
        /// </summary>
        public bool ShutDownAllManagers = false;

        /// <summary>
        /// The top level manager should set this flag after all managers have
        /// completed their Start-up process.  
        /// This is optional... but can be used inform all managers that the 
        /// system is up and fully operational.
        /// </summary>
        public bool RunState = false;

        public ManagerControlMessage()
			:base()
        {
            Clear();
        }

        public override void Clear()
        {
            base.Clear();
            ShutDownAllManagers = false;
            RunState = false;
        }

        public override void CopyMessage(RabitMessage msg)
        {
            //Ensure that all base parameters/variables are copied.
            base.CopyMessage(msg);
            //Cast Message into the Concrete Message.
            ManagerControlMessage cmsg = (ManagerControlMessage)msg;
            ShutDownAllManagers = cmsg.ShutDownAllManagers;
            RunState = cmsg.RunState;
        }

        public override string ToString()
        {
            string msgStr = "ManagerControlMessage:";
            msgStr = string.Concat(msgStr, " RunState:", RunState.ToString());
            return msgStr;
        }
    }
}

