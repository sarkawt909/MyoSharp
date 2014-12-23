﻿using System;

using MyoSharp.Internal;
using MyoSharp.Device;

namespace MyoSharp.Communication
{
    /// <summary>
    /// A class that implements the low level functionality of a channel.
    /// </summary>
    public sealed class ChannelDriver : IChannelDriver
    {
        #region Constants
        private static readonly DateTime TIMESTAMP_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        #endregion

        #region Fields
        private readonly IChannelBridge _channelBridge;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDriver"/> class.
        /// </summary>
        /// <param name="channelBridge">The channel bridge. Cannot be <c>null</c>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The exception that is thrown when <paramref name="channelBridge"/> is null.
        /// </exception>
        private ChannelDriver(IChannelBridge channelBridge)
        {
            if (channelBridge == null)
            {
                throw new ArgumentNullException("channelBridge", "The channel bridge cannot be null.");
            }

            _channelBridge = channelBridge;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new <see cref="IChannelDriver"/> instance.
        /// </summary>
        /// <param name="channelBridge">The channel bridge. Cannot be <c>null</c>.</param>
        /// <returns>Returns a new <see cref="IChannelDriver"/> instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The exception that is thrown when <paramref name="channelBridge"/> is null.
        /// </exception>
        public static IChannelDriver Create(IChannelBridge channelBridge)
        {
            if (channelBridge == null)
            {
                throw new ArgumentNullException("channelBridge", "The channel bridge cannot be null.");
            }

            return new ChannelDriver(channelBridge);
        }

        /// <inheritdoc />
        public void ShutdownMyoHub(IntPtr hubPointer)
        {
            if (hubPointer == IntPtr.Zero)
            {
                return;
            }

            if (PlatformInvocation.Running32Bit)
            {
                _channelBridge.ShutdownHub32(hubPointer, IntPtr.Zero);
            }
            else
            {
                _channelBridge.ShutdownHub64(hubPointer, IntPtr.Zero);
            }
        }

        /// <inheritdoc />
        public IntPtr InitializeMyoHub(string applicationIdentifier)
        {
            var errorHandle = IntPtr.Zero;
            try
            {
                IntPtr hubPointer;
                var result = PlatformInvocation.Running32Bit
                    ? _channelBridge.InitHub32(out hubPointer, applicationIdentifier, out errorHandle)
                    : _channelBridge.InitHub64(out hubPointer, applicationIdentifier, out errorHandle);

                if (result == MyoResult.Success)
                {
                    return hubPointer;
                }

                var errorMessage = GetErrorString(errorHandle);
                if (result == MyoResult.ErrorInvalidArgument)
                {
                    throw new ArgumentException(errorMessage);
                }

                throw new Exception(errorMessage);
            }
            finally
            {
                FreeMyoError(errorHandle);
            }
        }

        /// <inheritdoc />
        public DateTime GetEventTimestamp(IntPtr evt)
        {
            var timestampSeconds = PlatformInvocation.Running32Bit
                ? _channelBridge.EventGetTimestamp32(evt)
                : _channelBridge.EventGetTimestamp64(evt);
            return TIMESTAMP_EPOCH.AddMilliseconds(timestampSeconds / 1000d);
        }

        /// <inheritdoc />
        public void Run(IntPtr hubHandle, MyoRunHandler handler, IntPtr userData)
        {
            if (PlatformInvocation.Running32Bit)
            {
                _channelBridge.Run32(
                    hubHandle,
                    1000,
                    handler,
                    userData,
                    IntPtr.Zero);
            }
            else
            {
                _channelBridge.Run64(
                     hubHandle,
                     1000,
                     handler,
                     userData,
                     IntPtr.Zero);
            }
        }

        /// <inheritdoc />
        public MyoEventType GetEventType(IntPtr evt)
        {
            return PlatformInvocation.Running32Bit
                ? _channelBridge.EventGetType32(evt)
                : _channelBridge.EventGetType64(evt);
        }

        /// <inheritdoc />
        public IntPtr GetMyoForEvent(IntPtr evt)
        {
            return PlatformInvocation.Running32Bit
                ? _channelBridge.EventGetMyo32(evt)
                : _channelBridge.EventGetMyo64(evt);
        }

        /// <inheritdoc />
        public string GetErrorString(IntPtr errorHandle)
        {
            return PlatformInvocation.Running32Bit
                ? _channelBridge.LibmyoErrorCstring32(errorHandle)
                : _channelBridge.LibmyoErrorCstring64(errorHandle);
        }

        /// <inheritdoc />
        public void FreeMyoError(IntPtr errorHandle)
        {
            if (PlatformInvocation.Running32Bit)
            {
                _channelBridge.LibmyoFreeErrorDetails32(errorHandle);
            }
            else
            {
                _channelBridge.LibmyoFreeErrorDetails64(errorHandle);
            }
        }
        #endregion
    }
}