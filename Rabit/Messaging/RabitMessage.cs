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
    /// Message:
    /// Abstract message type.
    /// </summary>
    /// <remarks>Message contain data and a timestamp.
    /// All messages must support the minimal functionality defined
    /// by this class.
    /// </remarks>
    public abstract class RabitMessage : ICloneable, IDisposable
    {

        protected DateTime timestamp;
        /// <summary>
        /// Gets or sets the timestamp.
        /// The timestamp is an integral part of the publish subscribe 
        /// system.  Messages are only copied out if the timestamp is different.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTime Timestamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }

        #region Message Data Infrastructure

        /// <summary>
        /// Establish a generic process for one thread to wait for the 
        /// global message to change.
        /// Don't Copy this Message object to another message in the
        /// CopyIntoThisMessage() method.
        /// </summary>
        protected EventWaitHandle ewhWaitHandle = null;


        private PublishSubscribeMessage globalPublishSubscribeMessageRef = null;

        /// <summary>
        /// This is a reference to a global SSMU_Message of the
        /// same type as this message.  This allows a local copy of a 
        /// message to maintain a reference to the global message object.
        /// </summary>
        public PublishSubscribeMessage GlobalPublishSubscribeMessageRef
        {
            get { return globalPublishSubscribeMessageRef; }
            set { globalPublishSubscribeMessageRef = value; }
        }
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="RabitMessage"/> class.
        /// </summary>
        /// <param name="mt">The mt.</param>
        public RabitMessage()
		{
		}


        #region  Message Infrastructure
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
                if (ewhWaitHandle != null)
                {
                    ewhWaitHandle.Close();
                    ewhWaitHandle = null;
                }
            }
        }

		/// <summary>
		/// The Message Type Name should be the same as the class
		/// name of the Message.  This parameter is used so that 
		/// message types can easily be identified... often 
		/// the MsgTypeName is used in a case statement to process
		/// messages of different types.
		/// This is on the order of 10 times slower than using an "is" test
		/// do to the overhead of generating the name of the message.
		/// </summary>
		public string GetMessageTypeName()
		{
			string name = this.GetType().ToString ();
			int idx = name.LastIndexOf('.');
			name = name.Substring(idx + 1);
			return name;
		}

        /// <summary>
        /// waitForMessageChangedEH() 
        /// This is the delegate is called by the global message whenever the message is updated.
        /// This method simply wakes up the thread that owns this instance of the message object 
        /// if it is waiting for the global message to change..
        /// </summary>
        /// <param name="source">Source object calling this method.</param>
        /// <param name="e">Event Arguments that may be passed to this method.</param>
        private void WaitForMessageChangedEH(object source, EventArgs e)
        {
            //Release thread to continue process.
            if (ewhWaitHandle != null)
            {
                ewhWaitHandle.Set();
            }
        }

        /// <summary>
        /// Registers the wait event with global message.
        /// Used in conjunction with the method: waitForMessageChanged(). 
        /// This registration process allows a manager or thread to go into a wait state by calling
        /// the method:  waitForMessageChanged.  The manager or thread will then wait at the point 
        /// the msg.waitForMessageChanged() method is called until the message is updated.  
        /// There is a time-out associated with the msg.waitForMessageChanged() method.
        /// </summary>
        public void RegisterWaitEventWithGlobalMessage()
        {
            if (globalPublishSubscribeMessageRef != null)
            {
                if (ewhWaitHandle == null)
                {
                    ewhWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                }
                globalPublishSubscribeMessageRef.RegisterEvent(new PublishSubscribeMessChangedHandler(WaitForMessageChangedEH));
            }
            else
            {
				Console.WriteLine("Attempting to registerWaitEventWithGlobalMessage without a reference to the global message established.");
            }
        }


        /// <summary>
        /// Registers the delegate with global message.
        /// This method allows a manager or other user to register a delegate function
        /// with the message.  When the message is modified/updated, the delegage event
        /// will be called.
        /// </summary>
        /// <param name="delegateFunction">The delegate function.</param>
       public void RegisterEventWithGlobalMessage(PublishSubscribeMessChangedHandler delegateFunction)
        {
            if (globalPublishSubscribeMessageRef != null)
            {
                globalPublishSubscribeMessageRef.RegisterEvent(delegateFunction);
            }
            else
            {
				Console.WriteLine("Attempting to registerEventWithGlobalMessage without a reference to the global message established.");
            }
        }

        /// <summary>
        /// Unregister a delegate event function.
        /// </summary>
        /// <param name="delegateFunction"></param>
        public void UnregisterDelegateWithGlobalMessage(PublishSubscribeMessChangedHandler delegateFunction)
        {
            if (globalPublishSubscribeMessageRef != null)
            {
                globalPublishSubscribeMessageRef.UnregisterEvent(delegateFunction);
            }
            else
            {
				Console.WriteLine("Attempting to unregisterDelegateWithGlobalMessage without a reference to the global message established.");
            }
        }

        /// <summary>
        /// Unregister a Wait Event.
        /// </summary>
        public void UnregisterWaitEventWithGlobalMessage()
        {
            if (globalPublishSubscribeMessageRef != null)
            {
                globalPublishSubscribeMessageRef.UnregisterEvent(new PublishSubscribeMessChangedHandler(WaitForMessageChangedEH));
            }
            else
            {
				Console.WriteLine("Attempting to unregisterWaitEventWithGlobalMessage without a reference to the global message established.");
            }
        }

        /// <summary>
        /// waitForMessageChanged()
        /// This method causes the calling thread to wait for another thread to change/update
        /// the message's global object.  This is typically used by a local copy of a global 
        /// message.
        /// </summary>
        /// <param name="maxWaitTimeMS">The max wait time in milliseconds.</param>
        public virtual void WaitForMessageChanged(int maxWaitTimeMS)
        {
            if (ewhWaitHandle != null)
            {
                ewhWaitHandle.WaitOne(maxWaitTimeMS);
            }
        }

        /// <summary>
        /// Sets the message timestamp to the current time.
        /// </summary>
        public void SetTimeToNow()
        {
            timestamp = DateTime.Now;
        }

        public int CompareTime(RabitMessage msg)
        {
            return this.timestamp.CompareTo(msg.timestamp);
        }

        /// <summary>
        /// Posts this message the global message.
        /// This will make the message's content available to all 
        /// subscribers of the global message.
        /// </summary>
        public void PostMessage()
        {
            if (globalPublishSubscribeMessageRef != null)
            {
                globalPublishSubscribeMessageRef.PostMessage(this);
            }
            else
            {
				Console.WriteLine("Attempting to updateGlobalMessWithThisMessage without a reference to the global message established");
            }
        }

        /// <summary>
        /// Gets a copy of the global message.
        /// This will always get a copy of the Global Message... even if there
        /// has been no change in the Global message... In general is is more
        /// efficient to use:  FetchMessage
        /// </summary>
        /// <returns>True if Global Message was changed based on Timestamp.</returns>
        public bool ForceFetchMessage()
        {
            bool messChanged = false;
            if (globalPublishSubscribeMessageRef != null)
            {
                messChanged = globalPublishSubscribeMessageRef.Timestamp != timestamp;
                globalPublishSubscribeMessageRef.ForceFetchMessage(this);
            }
            else
            {
				Console.WriteLine("Attempting to getCopyOfGlobalMessage without a reference to the global message established");
            }
            return messChanged;
        }

        /// <summary>
        /// Gets a copy of the global message, if the global message is newer (based on timestamp)
        /// than this message..
        /// </summary>
        /// <returns>True if Global Message was changed based on Timestamp.</returns>
        public bool FetchMessage()
        {
            bool messChanged = false;
            if (globalPublishSubscribeMessageRef != null)
            {
                messChanged = globalPublishSubscribeMessageRef.FetchMessage(this);
            }
            else
            {
				Console.WriteLine("Attempting to getCopyOfGlobalMessage without a reference to the global message established");
            }
            return messChanged;
        }
        #endregion

        #region User Methods to Override
        /// <summary>
        /// CopyIntoThisMessag() This method copys all the data from msg to this message. 
        /// The Copy method is an integral part of the Publish Subscribe 
        /// mechanism... so this method must reliably hand the copy process.
        /// If the message contains arrays, lists, dictionaries, or similar
        /// types... these objects must be properly copied... don't just 
        /// copy the pointer or there will be all sorts of crazy errors caused
        /// in the system.
        /// The base CopyIntoThisMessage verifies that the message type
        /// being copied is the same message type as this message.
        /// A System.ArgumentException() is thrown if they are not.
        /// </summary>
        /// <param name="mess">The message with the data to be copied into
        /// this message.</param>
        virtual public void CopyMessage(RabitMessage msg)
        {
            //Verify we are coping the same concrete message type...
            //This is a serious error if we are not.
            if (msg.GetType() != this.GetType())
            {
                throw new System.ArgumentException();
            }
            Timestamp = msg.Timestamp;
        }


        /// <summary>
        /// Clone()  creates a new message object which is a copy of the 
        /// Message object. 
        /// Clone must be overriden if the message contains arrays, lists, dictionaries, 
        /// or similar types.  Clone is an integral part of the Publish Subscribe System.
        /// Note:  the Message Clone intentionally removes the Message ewhWaitHandle
        /// and the globalPublishSubscribeMessageRef.  Normally the new message does
        /// not need these references.  If the new message does need them... then they
        /// must be manually re-establish.
        /// </summary>
        /// <returns>Message object</returns>
        virtual public object Clone()
        {
            RabitMessage msgClone = (RabitMessage)this.MemberwiseClone();
            //Do not carry the Event Wait Handle or Global Publish Subscibe references forward...
            //This could cause strange behaviors... they must be re-established if needed...
            //in general they are not required.
            msgClone.ewhWaitHandle = null;
            msgClone.globalPublishSubscribeMessageRef = null;
            return msgClone;
        }


        /// <summary>
        /// Clears this message.
        /// Only needs to be overriden if there is a reason for 
        /// clearing the message... in general a good idea... but not
        /// required for Rabit's use.
        /// </summary>
        virtual public void Clear()
        {
            SetTimeToNow();
        }


        /// <summary>
        /// Create a string version of the message... typically used for logging
        /// purposes.  Override this method to fillin details of the Message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
			string msgStr = string.Concat("MessageType:", GetMessageTypeName());
            return msgStr;
        }
        #endregion
    }
}
