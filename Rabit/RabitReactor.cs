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
using System.Text;
using System.Threading.Tasks;

namespace Rabit
{
	public class RabitReactor
	{
        /// <summary>
        /// List of Managers that make up the system.
        /// </summary>
        private List<RabitManager> listOfManagers;

        /// <summary>
        /// The Rabit Reactor.
        /// The user may optionally add their own Rabit Workspace if they have
        /// extended the workspace to add other items/features.
        /// The user workspace must be instantiated before instantiating the
        /// Rabit reactor.
        /// </summary>
        /// <param name="userGlobalWorkspace"></param>
		public RabitReactor()
		{
			//Force the rabit workspace to initialize.
			RabitWorkspace.GetWorkspace();
            listOfManagers = new List<RabitManager>();
		}

        /// <summary>
        /// Add a Rabit Manager to the system.
        /// </summary>
        /// <param name="manager"></param>
        public void AddManager(RabitManager manager)
        {
            if (manager != null)
            {
                listOfManagers.Add(manager);
            }
        }

        /// <summary>
        /// After all managers have been added... call run.
        /// Run is a blocking call and does not return until the Rabit
        /// Manager system completely shuts down.
        /// </summary>
        public void Run()
        {
            //Now start each manager.
            foreach (RabitManager manager in listOfManagers)
            {
                manager.Run();
            }

            //Wait for all managers to shutdown
            foreach (RabitManager manager in listOfManagers)
            {
                manager.ManagerThread.Join();
            }

            //The Rabit Manager System is shutdown... 
        }
	}
}

