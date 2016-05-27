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
    /// PublishSubscribeMessChangedHandler
    /// Each message creates a Change Event whenever the message is updated.
    /// This is the delegate formate for the Change Event.
    /// </summary>
    /// <param name="sender">Source Object</param>
    /// <param name="e">EventArgs</param>
    public delegate void PublishSubscribeMessChangedHandler(object sender, EventArgs e);

    /// <summary>
    /// PublishSubscribeMessage:  
    /// Publish Subscribe Messasges provide a mechanism for sharing information between
    /// managers.  One (and only one) manager should be responsible for suppling the information
    /// for a given message (updating the message content) and all other managers (as needed)
    /// can subscribe to the message.  The subscribers can pull copies of the message as they
    /// need it, or they can register events and be automatically notified when the message 
    /// changes.  This is a very powerful, thread-safe, mechanism for communicating between
    /// managers.
    /// </summary>
    public class PublishSubscribeMessage : IDisposable
    {
        private readonly RabitMessage statusMessage;
        protected ReaderWriterLockSlim rwl;     //Only established if master copy.

        /// <summary>
        /// Get a reference to the Message Object.
        /// Be very careful with this... the reason for the Status Message Wrapper
        /// is to provide thread safe access to the message.  Getting a reference to 
        /// the message give a non-thread safe reference to the message.
        /// This is typically only used by the Messaging system during system setup
        /// and not during normal operation.
        /// </summary>
        public RabitMessage StatusMessage
        {
            get { return statusMessage; }
        }


        /// <summary>
        /// Get the Message Type  contained in the PublishSubscribeMessage
        /// object.
        /// </summary>
        public Type MsgType
        {
            get { return statusMessage.GetType(); }
        }


        public PublishSubscribeMessage(RabitMessage stMessage)
        {
            statusMessage = stMessage;
            rwl = new ReaderWriterLockSlim();
        }


        /// <summary>
        /// Get the Message's time stamp... it is assumed that the time stamp
        /// can safely be retieved without a thread lock... Do not rely on this 
        /// value for anything other than checking to see if the time changed...
        /// Get a copy of the whole message for a reliable time stamp.
        /// </summary>
        public DateTime Timestamp
        {
            get { return statusMessage.Timestamp; }
        }

        /// <summary>
        /// Changed:  event called whenever the message is updated.
        /// </summary>
        private event PublishSubscribeMessChangedHandler Changed;

        public void RegisterEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            rwl.EnterWriteLock();
            Changed += delegateFunction;
            rwl.ExitWriteLock();
        }

        public void UnregisterEvent(PublishSubscribeMessChangedHandler delegateFunction)
        {
            rwl.EnterWriteLock();
            Changed -= delegateFunction;
            rwl.ExitWriteLock();
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
                // free managed resources
                if (rwl != null)
                {
                    rwl.Dispose();
                    rwl = null;
                }
            }
        }

		/// <summary>
		/// Get the Message Type Name contained in the PublishSubscribeMessage
		/// object.
		/// </summary>
		public string GetMessageTypeName()
		{
			return statusMessage.GetMessageTypeName();
		}


        /// <summary>
        /// GetCopyOfMessage()  creates a new message which is a clone of the 
        /// <paramref name="statusMessage"/>.  A read lock must is used 
        /// to ensure an <c>Update</c> is not occurring when the copy is made.
        /// </summary>
        /// <returns>Message object</returns>
        public RabitMessage GetCopyOfMessage()
        {
            RabitMessage obj;
            rwl.EnterReadLock();
            try
            {
                obj = (RabitMessage)statusMessage.Clone();
            }
            finally
            {
                rwl.ExitReadLock();
            }
            return obj;
        }

        /// <summary>
        /// ForceFetchMessage()  copies the message information into the message object
        /// passed in. ForceFetchMessage() is used so that new objects are not created 
        /// only to be distroyed later, which can be time consumming.
        /// A read lock is used to ensure an <c>Update</c> 
        /// is not occurring when the copy is made.
        /// </summary>
        /// <param name="mess">A reference to a Message object which 
        /// will recieve the Global message's information</param>
        public void ForceFetchMessage(RabitMessage msg)
        {
            rwl.EnterReadLock();
            try
            {
                msg.CopyMessage(statusMessage);
            }
            finally
            {
                rwl.ExitReadLock();
            }
        }

        /// <summary>
        /// getCopyOfMessageIfNewer()  copies the message information into the message object
        /// passed in, if the message is newer in timestamep than the message passed in.  
        /// </summary>
        /// <param name="mess">A reference to a Message object which 
        /// will recieve the Global message's information</param>
        /// <returns>true if a copy was made, false if a copy was not made</returns>
        public bool FetchMessage(RabitMessage mess)
        {
            bool copyMade = false;
            if (statusMessage.Timestamp != mess.Timestamp)
            {
                rwl.EnterReadLock();
                try
                {
                    mess.CopyMessage(statusMessage);
                    copyMade = true;
                }
                finally
                {
                    rwl.ExitReadLock();
                }
            }
            return copyMade;
        }


        /// <summary>
        /// Update() This method updates the master copy of the message information.
        /// A write lock is obtained before updating the master object's information.
        /// This ensures another manager thread is not trying to copy the information out
        /// at the same time.
        /// After updating the message information, the <c>OnChange</c> method is called
        /// to inform other managers that the information has been changed.
        /// </summary>
        /// <param name="mess">a PublishSubscribeMessage object containing the new
        /// message information.</param>
        public void PostMessage(RabitMessage msg)
        {
            rwl.EnterWriteLock();
            try
            {
                //Set the time the message was posted.
                msg.SetTimeToNow();
                statusMessage.CopyMessage(msg);
            }
            finally
            {
                rwl.ExitWriteLock();
            }
            OnChange();
        }

    }

}
