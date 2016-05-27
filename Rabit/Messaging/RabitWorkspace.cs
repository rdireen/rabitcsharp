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
    /// The Rabit Workspace contains thread safe message containers and 
    /// access to message queues.
    /// The Rabit workspace is a singleton class as there can be only one
    /// Rabit Workspace in the system.
    /// </summary>
    public class RabitWorkspace : IDisposable
    {
        //The unique workspace.
        private static RabitWorkspace rabitWorkspace = null;

        
        #region PublishSubscribeMessageRegion
		//***********  This section contains all the Publish Subscribe Messages *************
        public struct PSMsgContainer
        {
            public RabitMessage MgrMsgRef;
            public PublishSubscribeMessage PSMsg;
        }
        
        /// <summary>
        /// A Dictionary for holding all the Publish Subscribe Messages
        /// </summary>
        private Dictionary<string, PSMsgContainer> publishSubcribeMsgDict;

        /// <summary>
        /// The direct access to the PublishSubcribeMsgDictionary is primarily 
        /// for the Rabit Reactor's use and should not be used by a Rabit Manager.
        /// </summary>
        public Dictionary<string, PSMsgContainer> PublishSubcribeMsgDictionary
        {
            get { return publishSubcribeMsgDict; }
        }

		//***********  End of the PublishSubscribeMessages *************       
        #endregion


        #region MessageQueueRegion
        //***********  This section contains all the Message Queues *************
        //Message Queues are used to get messages from one Manager to another 
        //manager.  The name of the message queue indicates the receiving 
        //manager.  Any manager may add messages to the queue, the receiving
        //manager will get and process the messages.

        /// <summary>
        /// A Dictionary for holding all the Message Queues
        /// </summary>
        private Dictionary<string, RabitMessageQueue> messageQueueDict;

        //private ReaderWriterLockSlim rwlMessageQueueDict;

        /// <summary>
        /// The direct access to the MessageQueueDictionary is primarily 
        /// for the Rabit Reactor's use and should not be used by a Rabit Manager.
        /// </summary>
        public Dictionary<string, RabitMessageQueue> MessageQueueDictionary
        {
            get { return messageQueueDict; }
        }


        //***********  End of Message Queues *************       
        #endregion


        protected RabitWorkspace()
        {
            rabitWorkspace = this;  //This works if the Workspace was derived from RabitWorkspace
            publishSubcribeMsgDict = new Dictionary<string, PSMsgContainer>();
            //rwlPublishSubscribeDict = new ReaderWriterLockSlim();

            messageQueueDict = new Dictionary<string, RabitMessageQueue>();
            //rwlMessageQueueDict = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Get a reference to the Rabit Workspace
        /// </summary>
        /// <returns></returns>
        public static RabitWorkspace GetWorkspace()
        {
            if (rabitWorkspace == null)
            {
                rabitWorkspace = new RabitWorkspace();
            }
            return rabitWorkspace;
        }


        // Summary:
        //     Performs application-defined tasks associated with freeing, releasing, or
        //     resetting unmanaged resources.
        public void Dispose()
        {
            //Release the Event Handle
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (PSMsgContainer psMsgC in publishSubcribeMsgDict.Values)
                {
                    psMsgC.PSMsg.Dispose();
                }
            }
        }

        /// <summary>
        /// Add a new message as a Publish Subscribe Message to the List
        /// of Publish Subscibe messages.  
        /// A reference to the Global Publish Subscribe Message will be added
        /// to the message passed in. 
        /// Each Manager should add all messages it needs to work with in the 
        /// Global Message Container.
        /// It is assumed that messages with the same name are the same Message
        /// type and are being used for the same purpose across all Managers.
        /// The first Manager to all the publish subscribe message will establish
        /// the message in the Global WorkSpace.  All other managers will simply
        /// have their message updated with a reference to the Global Message.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="msg"></param>
        /// /// <returns>true if Error, false if ok.</returns>
        public bool AddPublishSubscribeMessage(string messageName, RabitMessage msg)
        {
            bool error = false;
            //rwlPublishSubscribeDict.EnterWriteLock();
            if (!publishSubcribeMsgDict.ContainsKey(messageName))
            {
                msg.SetTimeToNow();     //Ensure the Current Time is set
                PSMsgContainer psMsgContainer;
                psMsgContainer.MgrMsgRef = msg;     //Reference to the orginal message
                psMsgContainer.PSMsg = new PublishSubscribeMessage((RabitMessage)msg.Clone());
                msg.GlobalPublishSubscribeMessageRef = psMsgContainer.PSMsg;
                publishSubcribeMsgDict.Add(messageName, psMsgContainer);
            }
            else
            {
                if (publishSubcribeMsgDict[messageName].PSMsg.MsgType == msg.GetType())
                {
                    //Update the incoming message with the Publish Subscribe 
                    //Message Reference.
                    msg.GlobalPublishSubscribeMessageRef = publishSubcribeMsgDict[messageName].PSMsg;
                }
                else
                {
                    //Error!
					Console.WriteLine("AddPublishSubscribeMessage: A new message is being added that conflicts with a current message.  MessageName:{0}, CurrentMsgType:{1}, NewMsgType:{2}",
                        messageName, publishSubcribeMsgDict[messageName].PSMsg.MsgType.ToString(), msg.GetType().ToString());
                    error = true;
                }

            }
            //rwlPublishSubscribeDict.ExitWriteLock();
            return error;
        }

        /// <summary>
        /// Get a copy of a Publish Subscribe message for the given message name.
        /// Returns a copy of the message or null if no message by that name.
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
        public RabitMessage GetMessage(string messageName)
        {
            RabitMessage msg = null;
            if(publishSubcribeMsgDict.ContainsKey(messageName))
            {
                msg = publishSubcribeMsgDict[messageName].PSMsg.GetCopyOfMessage();
            }
            return msg;
        }

        /// <summary>
        /// Post a message to the Publish Subscribe message.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="msg"></param>
        /// <returns>true if error... no message of the given name or the message types do not match, 
        /// false otherwise.</returns>
        public bool PostMessage(string messageName, RabitMessage msg)
        {
            bool error = true;
            if (publishSubcribeMsgDict.ContainsKey(messageName))
            {
                if (publishSubcribeMsgDict[messageName].PSMsg.MsgType == msg.GetType())
                {
                    publishSubcribeMsgDict[messageName].PSMsg.PostMessage(msg);
                }
            }

            return error;
        }


        /// <summary>
        /// Get a reference to the Global Publish Subscribe Message... this is 
        /// normally used to get the reference used by the Message.
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
        //public PublishSubscribeMessage GetPublishSubscribeMessageRef(string messageName)
        //{
        //    PublishSubscribeMessage psMsgRef = null;
        //    //rwlPublishSubscribeDict.EnterReadLock();
        //    if( publishSubcribeMsgDict.ContainsKey(messageName ) )
        //    {
        //        psMsgRef = publishSubcribeMsgDict[messageName].PSMsg;
        //    }
        //    //rwlPublishSubscribeDict.ExitReadLock();
        //    return psMsgRef;
        //}

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
            //rwlMessageQueueDict.EnterWriteLock();
            messageQueueDict[queueName] = msgQueue;
            //rwlMessageQueueDict.ExitWriteLock();
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
            bool error = true;
            RabitMessageQueue msgQueue = null;
            if (messageQueueDict.TryGetValue(queueName, out msgQueue))
            {
                msgQueue.RegisterMsgPickedUpEvent(delegateFunction);
                error = false;
            }
            return error;
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
            bool error = true;
            //As long as the messageQueuesDict is not being changed
            //after the Manager's first initialize... it is safe to 
            //obtain the message queue without getting a lock.
            RabitMessageQueue msgQueue = null;
            if( messageQueueDict.TryGetValue(queueName, out msgQueue) )
            {
                msgQueue.addMessage(msg);
                error = false;
            }
            return error;
        }
    }
}
