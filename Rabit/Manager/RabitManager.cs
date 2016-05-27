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
    /// RabitManager is the abstract class that establishes the basic 
    /// functions of each Manager.  Each Manager will run as an independent
    /// thread under Rabi.     
    /// </summary>
    abstract public class RabitManager
    {

        private int _wakeupTimeDelayMSec = 1000;

        /// <summary>
        /// After each ExecuteUnitOfWork(), the Manager Loop will 
        /// go to sleep until woken up by an event or a maximum wake-up
        /// time established by this parameter.
        /// The minimum time is 10 milliseconds.  
        /// </summary>
        public int WakeUpTimeDelayMilliseconds
        {
            get { return _wakeupTimeDelayMSec; }
            set { _wakeupTimeDelayMSec = value < 10 ? 10 : value; }
        }

        /// <summary>
        /// After each ExecuteUnitOfWork(), the Manager Loop will 
        /// go to sleep until woken up by an event or a maximum wake-up
        /// time established by this parameter.
        /// The minimum time is 10 milliseconds.  
        /// </summary>
        public double WakeUpTimeDelaySeconds
        {
            get { return 0.001 * (double)_wakeupTimeDelayMSec; }
            set { WakeUpTimeDelayMilliseconds = (int)(1000.0 * value); }
        }


        private Thread managerThread;
        private readonly string managerName;     //Used to name thread for debugging
        private bool shutdownManager = false;       //Set to true to shutdown the manager.

        /// <summary>
        /// ManagerThread Accessor
        /// </summary>
        public Thread ManagerThread
        {
            get { return managerThread; }
        }
        /// <summary>
        /// ManagerName Accessor
        /// </summary>
        public string ManagerName
        {
            get { return managerName; }
        }

        /// <summary>
        /// A Publish Subscribe Message use by this Manager to Post 
        /// this manager's status to the rest of the system.
        /// </summary>
        public ManagerStatusMessage MgrStatus;

        /// <summary>
        /// A Message to provide Syncronized Control between all managers.
        /// In general there will be one manager that has the top-level control 
        /// of all the managers.  That manager will be responsible for monitoring
        /// the other managers and setting the Run State and Shutdown.
        /// </summary>
        public ManagerControlMessage MgrControl;

        /// <summary>
        /// A A Message Queue so that Other Managers can send messages to this manager.
        /// The global Workspace name for this queue will be:  ManagerNameMessageQueue
        /// </summary>
        public RabitMessageQueue MgrMessageQueue;


        //A wait handle to allow other threads and messages to wake up 
        //the Manager when something has changed. 
        private EventWaitHandle ewhWaitHandle;
        private event PublishSubscribeMessChangedHandler wakeupManagerEvent;

        /// <summary>
        /// RabitManager constructor.  Creates the manager's thread.
        /// If the user has derived their own Workspace from the RabitWorkspace,
        /// it must be provided when the manager is constructed... otherwise
        /// the standard RabitWorkspace will be used.
        /// </summary>
        public RabitManager()
        {
			//Generate the Manager Name for use in tracking error messages.
			//and setting up queue names.
			managerName = this.GetType().ToString ();
			int idx = managerName.LastIndexOf('.');
			managerName = managerName.Substring(idx + 1);

            managerThread = new Thread(new ThreadStart(managerMain));
            managerThread.Name = managerName;
            shutdownManager = false;

            MgrStatus = new ManagerStatusMessage();
            //Add all publish subscribe messages this manager relies on to the 
            //global workspace here
            string mgrStatusMsgName = string.Concat(ManagerName, "Status");
            AddPublishSubscribeMessage(mgrStatusMsgName, MgrStatus);

            MgrControl = new ManagerControlMessage();
            AddPublishSubscribeMessage("ManagerControl", MgrControl);

            //string mgrMsgQueueName = string.Concat(ManagerName, "MessageQueue");
            MgrMessageQueue = new RabitMessageQueue(100, ManagerName);
            //Add the Message Queue to the Global Workspace
            AddManagerMessageQueue(ManagerName, MgrMessageQueue);

            //Setup Event Handling
            ewhWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            wakeupManagerEvent = new PublishSubscribeMessChangedHandler(wakeUpManagerEH);
        }

        /// <summary>
        /// General Delegate used to wake up the Manager.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void wakeUpManagerEH(object source, EventArgs e)
        {
            //Release thread to continue process.
            ewhWaitHandle.Set();
        }

        /// <summary>
        /// Add a new message as a Publish Subscribe Message to the List
        /// of Publish Subscibe messages.  
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="msg"></param>
        /// /// <returns>true if Error, false if ok.</returns>
        public bool AddPublishSubscribeMessage(string messageName, RabitMessage msg)
        {
            return RabitWorkspace.GetWorkspace().AddPublishSubscribeMessage(messageName, msg);
        }

        /// <summary>
        /// Add a thread-safe message queue to the Global WorkSpace.
        /// Managers that are on the recieving end of the Queue should add the 
        /// message queue to the global workspace.  Other managers may obtain
        /// references to the message queue or add messages to the queue at run
        /// time.  
        /// </summary>
        /// <param name="msgQueue"></param>
        /// <returns></returns>
        public void AddManagerMessageQueue(string queueName, RabitMessageQueue msgQueue)
        {
            RabitWorkspace.GetWorkspace().AddManagerMessageQueue(queueName, msgQueue);
        }


        /// <summary>
        /// A manager can register a message queue pick up event if the manager
        /// needs to know when a message has been picked up by the receiving queue.
        /// Registering events should be done in the Manager's startup code.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="delegateFunction"></param>
        /// <returns></returns>
        public bool RegisterMsgPickedUpEvent(string queueName, PublishSubscribeMessChangedHandler delegateFunction)
        {
            return RabitWorkspace.GetWorkspace().RegisterMsgPickedUpEvent(queueName, delegateFunction);
        }

        /// <summary>
        /// Add a message to a Manager's Message Queue. 
        /// It is assumed the manager added the Message Queue when it
        /// was initialized.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="msg"></param>
        /// <returns>true if error... not message queue, false otherwise</returns>
        public bool AddMessageToQueue(string queueName, Object msg)
        {
            return RabitWorkspace.GetWorkspace().AddMessageToQueue(queueName, msg);
        }


        /// <summary>
        /// run() starts the Manager's main control loop.
        /// </summary>
        public void Run()
        {
            managerThread.Start(); 
        }


        /// <summary>
        /// This method may be called to shut the manager down.
        /// </summary>
        public void ShutDownManager()
        {
            shutdownManager = true;
        }

        /// <summary>
        /// This Method is called only if the the manager thread is locked
        /// up and the normal shutdown process will not work.  
        /// </summary>
        public void KillManager()
        {
            shutdownManager = true;
            managerThread.Abort();
        }


        /// <summary>
        /// Startup:  This method is run when the manager is first started.
        /// All Startup and Initialization code for the manager must be placed in 
        /// this method.
        /// All Messages that need to register an Event must do so in the 
        /// Start-up method (See example below).
        /// When overriding this method there is no reason to call the base 
        /// Startup Method.
        /// </summary>
        public virtual void Startup()
        {
            //Register a wake-up for messages posted to the Messages Queue like this:
            MgrMessageQueue.RegisterEvent(wakeUpManagerEH);

            //At the end of the Startup Process publish the Manager status  as
            //Run State.
            MgrStatus.ManagerStatus = eManagerStatus.Running;
            MgrStatus.PostMessage();
        }

        /// <summary>
        /// Shutdown:  This method is run when the manager is shutdown.
        /// All shutdown and finalization code for the manager must be placed in 
        /// this method.
        /// When overriding this method there is no reason to call the base 
        /// Shutdown Method.
        /// </summary>
        public virtual void Shutdown()
        {
            //Let the rest of the system know we are going into shutdown.
            MgrStatus.ManagerStatus = eManagerStatus.ShuttingDown;
            MgrStatus.PostMessage();

            //User's Method:  Do the Shutdown work.... 

            //Let the rest of the system know we are going into shutdown.
            MgrStatus.ManagerStatus = eManagerStatus.ShutDown;
            MgrStatus.PostMessage();
        }

        /// <summary>
        /// The Manager Main runs as in infinite loop.  Each time through
        /// the loop the the ExecuteUnitOfWork method is called.  This is where
        /// all the Manager's work is accomplished.  The manager's work should
        /// be accomplished in "Units of Work".  After the Unit of work is accomplished
        /// the manager's main loop will sleep until woken up by a timeout or 
        /// an event.
        /// When overriding this method there is no reason to call the base 
        /// Shutdown Method.
        /// </summary>
        public abstract void ExecuteUnitOfWork();


        /// <summary>
        /// managerMain()  The Manager's main control loop resides here.
        /// </summary>
        private void managerMain()
        {
            //Register the MgrControl Event
            MgrControl.RegisterEventWithGlobalMessage(wakeUpManagerEH);
            MgrControl.ForceFetchMessage();   //Ensure we have the latest copy.

            //Manager Startup Process.
            Startup();

            while (!shutdownManager && !MgrControl.ShutDownAllManagers)
            {
                try
                {
                    ExecuteUnitOfWork();
					if( _wakeupTimeDelayMSec > 0 )
					{
                    	ewhWaitHandle.WaitOne(_wakeupTimeDelayMSec);
					}
                    MgrControl.FetchMessage();
                }
                catch (Exception ex)
                {
					Console.WriteLine("Manager:{0}\nThrew execption: {1}\nStackTrace: {2}", managerName, ex.Message, ex.StackTrace);
                }
            }

            //Manager Shutdown Process.
            Shutdown();
        }

    }
}
