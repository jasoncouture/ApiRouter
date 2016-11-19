namespace System.Net
{
    [FriendAccessAllowed]
    internal class RtcState
    {
        // Enum equivalent to CONTROL_CHANNEL_TRIGGER_STATUS enum.
        // Argument structures for SIO_QUERY_TRANSPORT_SETTING and
        // SIO_QUERY_TRANSPORT_SETTING.
        [FriendAccessAllowed]
        internal enum ControlChannelTriggerStatus
        {
            Invalid = 0,
            SoftwareSlotAllocated,
            HardwareSlotAllocated,
            PolicyError,
            SystemError,
            TransportDisconnected,
            ServiceUnavailable
        };

        // Socket configuration data we need to pass to IOControl after creating the socket.
        internal byte[] inputData;
        // IOControl results
        internal byte[] outputData;
        // Set this event after we have succesfully created and configured the socket.
        internal ManualResetEvent connectComplete;
        // Set this event after we have succesfully sent the headers and request body.
        internal ManualResetEvent flushComplete;
        // Any error codes that need to be reported to NotificationChannel
        internal int result;

        private int isAborted;

        internal bool IsAborted
        {
            get { return isAborted != 0; }
        }

        internal RtcState()
        {
            connectComplete = new ManualResetEvent(false);
            flushComplete = new ManualResetEvent(false);
        }

        internal void Abort()
        {
            Interlocked.Exchange(ref isAborted, 1);

            // The request failed, unblock the work item.
            connectComplete.Set();
            flushComplete.Set();
        }

        internal bool IsEnabled()
        {
            Debug.Assert((outputData != null) && (outputData.Length == sizeof(ControlChannelTriggerStatus)),
                "outputData not initialized.");

            ControlChannelTriggerStatus status = (ControlChannelTriggerStatus)BitConverter.ToInt32(outputData, 0);

            // We consider RTC setup successful if all of the following are true:
            // - IOCTL calls succeeded
            // - IOCTL(SIO_QUERY_TRANSPORT_SETTING) call returned either
            //   CONTROL_CHANNEL_TRIGGER_STATUS_SOFTWARE_SLOT_ALLOCATED or
            //   CONTROL_CHANNEL_TRIGGER_STATUS_HARDWARE_SLOT_ALLOCATED
            return (result == 0) &&
                   ((status == ControlChannelTriggerStatus.SoftwareSlotAllocated) ||
                    (status == ControlChannelTriggerStatus.HardwareSlotAllocated));
        }
    }
}