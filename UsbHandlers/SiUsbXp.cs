using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GreenPower.UsbHandlers
{
    public class SiUsbXp
    {
        #region Return codes
        public const byte SI_SUCCESS = 0x00;
        public const byte SI_DEVICE_NOT_FOUND = 0xFF;
        public const byte SI_INVALID_HANDLE = 0x01;
        public const byte SI_READ_ERROR = 0x02;
        public const byte SI_RX_QUEUE_NOT_READY = 0x03;
        public const byte SI_WRITE_ERROR = 0x04;
        public const byte SI_RESET_ERROR = 0x05;
        public const byte SI_INVALID_PARAMETER = 0x06;
        public const byte SI_INVALID_REQUEST_LENGTH = 0x07;
        public const byte SI_DEVICE_IO_FAILED = 0x08;
        public const byte SI_INVALID_BAUDRATE = 0x09;
        public const byte SI_FUNCTION_NOT_SUPPORTED = 0x0a;
        public const byte SI_GLOBAL_DATA_ERROR = 0x0b;
        public const byte SI_SYSTEM_ERROR_CODE = 0x0c;
        public const byte SI_READ_TIMED_OUT = 0x0d;
        public const byte SI_WRITE_TIMED_OUT = 0x0e;
        public const byte SI_IO_PENDING = 0x0f;
        #endregion
        #region GetProductString() function flags
        public const byte SI_RETURN_SERIAL_NUMBER = 0x00;
        public const byte SI_RETURN_DESCRIPTION = 0x01;
        public const byte SI_RETURN_LINK_NAME = 0x02;
        public const byte SI_RETURN_VID = 0x03;
        public const byte SI_RETURN_PID = 0x04;
        #endregion
        #region RX Queue status flags
        public const byte SI_RX_NO_OVERRUN = 0x00;
        public const byte SI_RX_EMPTY = 0x00;
        public const byte SI_RX_OVERRUN = 0x01;
        public const byte SI_RX_READY = 0x02;
        #endregion
        #region Buffer size limits
        public const int SI_MAX_DEVICE_STRLEN = 256;
        public const int SI_MAX_READ_SIZE = 4096 * 16;
        public const int SI_MAX_WRITE_SIZE = 4096;
        #endregion
        #region Input and Output pin Characteristics
        public const byte SI_HELD_INACTIVE = 0x00;
        public const byte SI_HELD_ACTIVE = 0x01;
        public const byte SI_FIRMWARE_CONTROLLED = 0x02;
        public const byte SI_RECEIVE_FLOW_CONTROL = 0x02;
        public const byte SI_TRANSMIT_ACTIVE_SIGNAL = 0x03;
        public const byte SI_STATUS_INPUT = 0x00;
        public const byte SI_HANDSHAKE_LINE = 0x01;
        #endregion
        #region Mask and Latch value bit definitions
        public const byte SI_GPIO_0 = 0x01;
        public const byte SI_GPIO_1 = 0x02;
        public const byte SI_GPIO_2 = 0x04;
        public const byte SI_GPIO_3 = 0x08;
        #endregion
        #region GetDeviceVersion() return codes
        public const byte SI_CP2101_VERSION = 0x01;
        public const byte SI_CP2102_VERSION = 0x02;
        public const byte SI_CP2103_VERSION = 0x03;

        #endregion

        #region Command

        /// <summary>
        /// This function returns the number of devices connected to the host.
        /// </summary>
        /// <param name="lpdwNumDevices"></param>Address of a DWORD variable that will contain the number of devices connected on return
        /// <returns>SI_SUCCESS or SI_DEVICE_NOT_FOUND or SI_INVALID_PARAMETER</returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetNumDevices(
        ref uint lpdwNumDevices
        );

        /// <summary>
        /// This function returns a null terminated serial number (S/N) string or product description string for
        /// the device specified by an index passed in DeviceNum. The index for the first device is 0 and the
        /// last device is the value returned by SI_GetNumDevices – 1.
        /// </summary>
        /// <param name="dwDeviceNum">DeviceNum—Index of the device for which the product description string or serial number
        /// string is desired.</param>
        /// <param name="lpvDeviceString">DeviceString—Variable of type SI_DEVICE_STRING which will contain a NULL terminated
        /// device descriptor or serial number string on return.</param>
        /// <param name="dwFlags">Options—DWORD containing flags to determine if DeviceString contains a serial number,
        /// product description, Vendor ID, or Product ID string. See "Appendix D—Definitions from C++
        /// header file SiUSBXp.h” for flags</param>
        /// <returns>SI_SUCCESS or SI_DEVICE_NOT_FOUND or SI_INVALID_PARAMETER</returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetProductString(
        uint dwDeviceNum,
        StringBuilder lpvDeviceString,
        uint dwFlags
        );

        /// <summary>
        /// Opens a device (using device number as returned by SI_GetNumDevices) and returns a handle
        /// which will be used for subsequent accesses
        /// </summary>
        /// <param name="dwDevice">DeviceNum—Device index. 0 for first device, 1 for 2nd, etc.</param>
        /// <param name="cyHandle">Handle—Pointer to a variable where the handle to the device will be stored. This handle will be
        /// used by all subsequent accesses to the device.</param>
        /// <returns>SI_SUCCESS or SI_DEVICE_NOT_FOUND or SI_INVALID_PARAMETER or SI_GLOBAL_DATA_ERROR</returns>    
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_Open(
        uint dwDevice,
        ref IntPtr cyHandle
        );

        /// <summary>
        /// Closes an open device using the handle provided by SI_Open and sets the handle to
        /// INVALID_HANDLE_VALUE.
        /// </summary>
        /// <param name="cyHandle">Handle—Handle to the device to close as returned by SI_Open</param>
        /// <returns>SI_SUCCESS or SI_INVALID_HANDLE or SI_SYSTEM_ERROR_CODE or SI_GLOBAL_DATA_ERROR </returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_Close(
        IntPtr cyHandle
        );

        /// <summary>
        /// Reads the available number of bytes into the supplied buffer and retrieves the number of bytes
        /// that were read (this can be less than the number of bytes requested). This function returns synchronously
        /// if the overlapped object is set to NULL (this happens by default) but will not block system
        /// execution.
        /// </summary>
        /// <param name="cyHandle">Handle—Handle to the device to read as returned by SI_Open.</param>
        /// <param name="lpBuffer">Buffer—Address of a character buffer to be filled with read data</param>
        /// <param name="dwBytesToRead">NumBytesToRead—Number of bytes to read from the device into the buffer (0–64 kB).</param>
        /// <param name="lpdwBytesReturned">NumBytesReturned—Address of a DWORD which will contain the number of bytes actually
        /// read into the buffer on return.</param>
        /// <param name="o">(Optional)—Address of an initialized OVERLAPPED object that can be used for asynchronous
        /// reads.</param>
        /// <returns>SI_SUCCESS or SI_READ_ERROR or SI_INVALID_PARAMETER or SI_INVALID_HANDLE or SI_SI_READ_TIMED_OUT or SI_IO_PENDING or SI_SYSTEM_ERROR_CODE or SI_INVALID_REQUEST_LENGTH or SI_DEVICE_IO_FAILED</returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_Read(
        IntPtr cyHandle,
        byte[] lpBuffer,
        uint dwBytesToRead,
        ref uint lpdwBytesReturned,
        IntPtr o
        );

        /// <summary>
        /// Writes the specified number of bytes from the supplied buffer to the device. This function returns
        /// synchronously if the overlapped object is set to NULL (this happens by default) but will not block
        /// system execution.
        /// </summary>
        /// <param name="cyHandle">Handle—Handle to the device to write as returned by SI_Open.</param>
        /// <param name="lpBuffer">Buffer—Address of a character buffer of data to be sent to the device.</param>
        /// <param name="dwBytesToWrite">NumBytesToWrite—Number of bytes to write to the device (0–4096 bytes).</param>
        /// <param name="lpdwBytesWritten">NumBytesWritten—Address of a DWORD which will contain the number of bytes actually written to the device.</param>
        /// <param name="o">. (Optional)—Address of an initialized OVERLAPPED object that can be used for asynchronous writes.</param>
        /// <returns>SI_SUCCESS or SI_WRITE_ERROR or SI_INVALID_REQUEST_LENGTH or SI_INVALID_PARAMETER or SI_INVALID_HANDLE or SI_WRITE_TIMED_OUT or SI_IO_PENDING or SI_SYSTEM_ERROR_CODE or SI_DEVICE_IO_FAILED</returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_Write(
        IntPtr cyHandle,
        byte[] lpBuffer,
        uint dwBytesToWrite,
        ref uint lpdwBytesWritten,
        IntPtr o
        );

        /// <summary>
        /// Interface for any miscellaneous device control functions. A separate call to SI_DeviceIOControl
        /// is required for each input or output operation. A single call cannot be used to perform both an
        /// input and output operation simultaneously. Refer to DeviceIOControl function definition on
        /// MSDN Help for more details
        /// </summary>
        /// <param name="cyHandle">Handle—Handle to the device as returned by SI_Open.</param>
        /// <param name="dwIoControlCode">IOControlCode—Code to select control function.</param>
        /// <param name="lpInBuffer">InBuffer—Pointer to input data buffer.</param>
        /// <param name="dwBytesToRead">BytesToRead—Number of bytes to be read into InBuffer.</param>
        /// <param name="lpOutBuffer">OutBuffer—Pointer to output data buffer.</param>
        /// <param name="dwBytesToWrite">BytesToWrite—Number of bytes to write from OutBuffer.</param>
        /// <param name="lpdwBytesSucceeded">BytesSucceeded—Address of a DWORD variable that will contain the number of bytes read
        /// by a input operation or the number of bytes written by a output operation on return.</param>
        /// <returns>SI_SUCCESS or SI_DEVICE_IO_FAILED or SI_INVALID_HANDLE</returns>
        [DllImport("SiUSBXp.dll")]
        public static extern int SI_DeviceIOControl(
        IntPtr cyHandle,
        uint dwIoControlCode,
        byte[] lpInBuffer,
        uint dwBytesToRead,
        byte[] lpOutBuffer,
        uint dwBytesToWrite,
        ref uint lpdwBytesSucceeded
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_FlushBuffers(
        IntPtr cyHandle,
        byte FlushTransmit,
        byte FlushReceive
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetTimeouts(
        uint dwReadTimeout,
        uint dwWriteTimeout
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetTimeouts(
        ref uint lpdwReadTimeout,
        ref uint lpdwWriteTimeout
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_CheckRXQueue(
        IntPtr cyHandle,
        ref uint lpdwNumBytesInQueue,
        ref uint lpdwQueueStatus
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetBaudRate(
        IntPtr cyHandle,
        uint dwBaudRate
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetBaudDivisor(
        IntPtr cyHandle,
        ushort wBaudDivisor
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetLineControl(
        IntPtr cyHandle,
        ushort wLineControl
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetFlowControl(
        IntPtr cyHandle,
        byte bCTS_MaskCode,
        byte bRTS_MaskCode,
        byte bDTR_MaskCode,
        byte bDSR_MaskCode,
        byte bDCD_MaskCode,
        bool bFlowXonXoff
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetModemStatus(
        IntPtr cyHandle,
        ref byte ModemStatus
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_SetBreak(
        IntPtr cyHandle,
        ushort wBreakState
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_ReadLatch(
        IntPtr cyHandle,
        ref byte lpbLatch
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_WriteLatch(
        IntPtr cyHandle,
        byte bMask,
        byte bLatch
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetPartNumber(
        IntPtr cyHandle,
        ref byte lpbPartNum
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetDLLVersion(
        ref uint HighVersion,
        ref uint LowVersion
        );

        [DllImport("SiUSBXp.dll")]
        public static extern int SI_GetDriverVersion(
        ref uint HighVersion,
        ref uint LowVersion
        );
        #endregion
    }
}