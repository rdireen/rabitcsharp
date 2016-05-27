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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Rabit
{
    /// <summary>
    /// The message queue class sets up a mechanism for passing messages
    /// between managers.  This is used when the message is not global in
    /// nature.  A message queue ensure each message created, is passed on 
    /// to the corrisponding manager and that no interving messages are lost.
    /// The receving manager can register an event so that it can be woken
    /// up when a new message is added to the queue.
    /// </summary>
    public class RabitMessageQueue
    {
		/// <summary>
		/// This this the Logger that can be used with message queue.
		/// </summary>
	
        private Queue messQueue;
        private Queue syncMessQueue;
        private int _maxNoMessagesAllowedInQueue;
        private string msgQueueName;

        /// <summary>
        /// A name for the queue... primarily used for logging purposes.
        /// </summary>
        public string MessageQueueName
        {
            get { return msgQueueName; }
        }

        /// <summary>
        /// A Message Queue should not be allowed to grow without bounds
        /// which could cause a system failure.  This can happen if the receiving
        /// end has a failure and is not processing messages for some reason.
        /// Set this to a reasonable value for the system's normal operation.
        /// </summary>
        public int MaxNoMessagesAllowedInQueue
        {
            get { return _maxNoMessagesAllowedInQueue; }
            set { _maxNoMessagesAllowedInQueue = value < 1 ? 1 : value > 1000000 ? 1000000 : value; }
        }

        public RabitMessageQueue(int maxNoMessages, string msgQName)
        {
            MaxNoMessagesAllowedInQueue = maxNoMessages;
            msgQueueName = string.IsNullOrEmpty(msgQName) ? "MessageQueue" : msgQName;
            messQueue = new Queue();
            syncMessQueue = Queue.Synchronized(messQueue);
        }

        /// <summary>
        /// Changed:  event called whenever a message is added to the queue..
        /// Add event delagates here to register "wake-up" calls.
        /// </summary>
		private event PublishSubscribeMessChangedHandler Changed;
		public void RegisterEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            Changed += delegateFunction;
        }
		public void UnregisterEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            Changed -= delegateFunction;
        }

        /// <summary>
        /// OnChange() this metod fires the Changed event.
        /// </summary>
        protected void OnChange()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Message Picked Up:  event called whenever a message is removed from the queue..
        /// Add event delagates here to register "wake-up" calls.
        /// </summary>
		private event PublishSubscribeMessChangedHandler MessagePickedUp;
		public void RegisterMsgPickedUpEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            MessagePickedUp += delegateFunction;
        }
		public void UnregisterMsgPickedEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            MessagePickedUp -= delegateFunction;
        }

        /// <summary>
        /// OnChange() this metod fires the Changed event.
        /// </summary>
        protected void OnMsgPickedUp()
        {
            if (MessagePickedUp != null)
            {
                MessagePickedUp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Clears the message queue.
        /// </summary>
        public void ClearMessQueue()
        {
            syncMessQueue.Clear();
        }

        /// <summary>
        /// Gets a count of the number of items in the message queue
        /// </summary>
        /// <returns></returns>
        public int NoMessagesInQueue()
        {
            return syncMessQueue.Count;
        }

        /// <summary>
        /// Adds adds a message to the queue and calls the event handler.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void addMessage(Object msg)
        {
            if (syncMessQueue.Count < MaxNoMessagesAllowedInQueue)
            {
                syncMessQueue.Enqueue(msg);
                OnChange();
            }
            else
            {
				Console.WriteLine(msgQueueName + " is Full.  Count = " + MaxNoMessagesAllowedInQueue.ToString());
            }
        }

        /// <summary>
        /// Adds adds a message to the queue and without calling the event handler.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void addMessageNoEventTrigger(Object msg)
        {
            if (syncMessQueue.Count < MaxNoMessagesAllowedInQueue)
            {
                syncMessQueue.Enqueue(msg);
            }
            else
            {
				Console.WriteLine(msgQueueName + " is Full.  Count = " + MaxNoMessagesAllowedInQueue.ToString());
            }
        }

        /// <summary>
        /// Gets then next message from the queue.
        /// Returns a null if the queue is empty.
        /// </summary>
        /// <returns>A Message or null</returns>
        public Object getMessage()
        {
            Object msg = null; 
            if (syncMessQueue.Count > 0)
            {
                msg = (Object)syncMessQueue.Dequeue();
                if (msg != null) 
                { 
                    OnMsgPickedUp(); 
                }
            }
                        
            return msg;
        }      
    }
}
