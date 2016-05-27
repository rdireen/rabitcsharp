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
    /// Desc: Publish Subscribe Message Array
    /// This class is similar to the PublishSubscribeMessage, the primary difference
    /// is that this class contains an array of the same type of message.
    /// </summary>
    public class PublishSubscibeMsgArray : IDisposable
    {
        private readonly int MaxNoMessages;

        private RabitMessage[] messageArray;

        protected ReaderWriterLockSlim rwl;     //Only established if master copy.

        public int ArraySize
        {
            get { return messageArray.Length; }
        }

        /// <summary>
        /// PublishSubscibeMsgArray with an array size which is the largest
        /// number of messages to store in the array.
        /// </summary>
        /// <param name="arraySize"></param>
        /// <param name="defaultMessage">If defaultMessage not null the MessageArray will be
        /// filled with copies of the default message, otherwise the message array
        /// will be filled with nulls.</param>
        public PublishSubscibeMsgArray(int arraySize, RabitMessage defaultMessage)
        {
            MaxNoMessages = arraySize < 5 ? 5 : arraySize;
            messageArray = new RabitMessage[MaxNoMessages];

            if (defaultMessage != null)
            {
                for (int i = 0; i < MaxNoMessages; i++)
                    messageArray[i] = (RabitMessage)defaultMessage.Clone();
            }

            rwl = new ReaderWriterLockSlim();
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

        /// <summary>
        /// Gets a copy of the message at index msgIdx.
        /// </summary>
        /// <param name="msgIdx"></param>
        /// <returns>a copy of the message if it exits, null otherwise</returns>
        public RabitMessage GetCopyOfMessage(int msgIdx)
        {
            RabitMessage obj = null;
            if (msgIdx >= 0 && msgIdx < MaxNoMessages)
            {
                rwl.EnterReadLock();
                try
                {
                    if (messageArray[msgIdx] != null)
                        obj = (RabitMessage)messageArray[msgIdx].Clone();
                }
                finally
                {
                    rwl.ExitReadLock();
                }
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
        /// <param name="msgIdx">index to message</param>
        /// <param name="mess">A reference to a Message object which 
        /// will recieve the Global message's information</param>
        /// <returns>true if a copy was made, false if the message array is empty at that point</returns>
        public bool ForceFetchMessage(int msgIdx, RabitMessage msg)
        {
            bool copyMade = false;
            if (msgIdx >= 0 && msgIdx < MaxNoMessages)
            {
                rwl.EnterReadLock();
                try
                {
                    if (messageArray[msgIdx] != null)
                    {
                        msg.CopyMessage(messageArray[msgIdx]);
                        copyMade = true;
                    }
                }
                finally
                {
                    rwl.ExitReadLock();
                }
            }
            return copyMade;
        }


        /// <summary>
        /// FetchMessage()  copies the message information into the message object
        /// passed in, if the message is newer in timestamep than the message passed in.  
        /// </summary>
        /// <param name="msgIdx">index to message</param>
        /// <param name="mess">A reference to a Message object which 
        /// will recieve the Global message's information</param>
        /// <returns>true if a copy was made, false if a copy was not made</returns>
        public bool FetchMessage(int msgIdx, RabitMessage msg)
        {
            bool copyMade = false;
            if (msgIdx >= 0 && msgIdx < MaxNoMessages
                && messageArray[msgIdx] != null 
                && (messageArray[msgIdx].Timestamp != msg.Timestamp))
            {
                rwl.EnterReadLock();
                try
                {
                    msg.CopyMessage(messageArray[msgIdx]);
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
        /// Post() This method updates the master copy of the message information.
        /// A write lock is obtained before updating the master object's information.
        /// This ensures another manager thread is not trying to copy the information out
        /// at the same time.
        /// After updating the message information, the <c>OnChange</c> method is called
        /// to inform other managers that the information has been changed.
        /// </summary>
        /// <param name="msgIdx"></param>
        /// <param name="mess">a SSMU_StatusMessage object containing the new
        /// message information.</param>
        public void PostMessage(int msgIdx, RabitMessage msg)
        {
            if (msgIdx >= 0 && msgIdx < MaxNoMessages)
            {
                rwl.EnterWriteLock();
                try
                {
                    if (messageArray[msgIdx] != null)
                        messageArray[msgIdx].CopyMessage(msg);
                    else
                        messageArray[msgIdx] = (RabitMessage)msg.Clone();
                }
                finally
                {
                    rwl.ExitWriteLock();
                }
                OnChange();
            }
        }

    }
}
