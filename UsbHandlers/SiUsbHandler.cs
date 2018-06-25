using System;
using GreenPower.UsbHandlers;

namespace GreenPower.UsbHandlers
{
    public class SiUsbHandler
    {
        private int ReadData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesRead, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int transmissionStatus;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(dwTimeout, 0); 
            transmissionStatus = SiUsbXp.SI_Read(handle, buffer, dwSize, ref lpdwBytesRead, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return transmissionStatus;
        }

        private int WriteData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesWritten, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int transmissionStatus;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(0, dwTimeout); 
            transmissionStatus = SiUsbXp.SI_Write(handle, buffer, dwSize, ref lpdwBytesWritten, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return transmissionStatus;
        }

        public int WriteReadUSB(IntPtr hMasterCOM, uint amountToSend, uint amountToReceive, ref byte[] dataToSend, ref byte[] dataReceived, uint timeoutForReceive)
        {
            IntPtr _invalidHandleValue = new IntPtr(-1);

            byte[] bufferOutput = new byte[512];
            byte[] bufferInput = new byte[512];
            bool status = true;
            int transmissionStatus;
            uint _quantityOfSent = 0;
            uint _quantityOfReceived = 0;

            Array.Clear(bufferOutput, 0, bufferOutput.Length);
            Array.Clear(bufferInput, 0, bufferInput.Length);

            status = CheckQuantity(amountToSend);
            status = CheckQuantity(amountToReceive);
            status = (hMasterCOM == null) ? false : true;
            status = (hMasterCOM == _invalidHandleValue) ? false : true;
            status = (dataToSend == null) ? false : true;
            status = (dataReceived == null) ? false : true;

            if(!status) return SiUsbXp.SI_INVALID_PARAMETER;

            Array.Clear(bufferOutput, 0, bufferOutput.Length);
            Array.Clear(bufferInput, 0, bufferInput.Length);
            Array.Copy(dataToSend, bufferOutput, amountToSend);

            transmissionStatus = WriteData(hMasterCOM, bufferOutput, amountToSend, ref _quantityOfSent, timeoutForReceive);
            if (amountToSend != _quantityOfSent) return SiUsbXp.SI_INVALID_PARAMETER;

            transmissionStatus = ReadData(hMasterCOM, bufferInput, amountToReceive, ref _quantityOfReceived, timeoutForReceive);
            if (amountToReceive != _quantityOfReceived) return SiUsbXp.SI_INVALID_PARAMETER;

            Array.Copy(bufferInput, dataReceived, amountToReceive);

            return transmissionStatus;
        }

        private bool CheckQuantity(uint quantity)
        {
            return (quantity > 0 || quantity < 512) ? true : false;
        }
    }
}