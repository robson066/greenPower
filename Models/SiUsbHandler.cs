using System;
using GreenPower.Models;

namespace greenPower.Models
{
    public class SiUsbHandler
    {
        private readonly IntPtr _invalidHandleValue = new IntPtr(-1);
        private uint _quantityOfSent;
        private uint _quantityOfReceived;

        private bool ReadData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesRead, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int status = SiUsbXp.SI_SUCCESS;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(dwTimeout, 0); 
            status = SiUsbXp.SI_Read(handle, buffer, dwSize, ref lpdwBytesRead, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return (status == SiUsbXp.SI_SUCCESS);
        }

        private bool WriteData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesWritten, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int status = SiUsbXp.SI_SUCCESS;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(0, dwTimeout); 
            status = SiUsbXp.SI_Write(handle, buffer, dwSize, ref lpdwBytesWritten, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return (status == SiUsbXp.SI_SUCCESS);
        }

        public bool WriteReadUSB(IntPtr hMasterCOM, uint quantityToSend, uint quantityToReceive, ref byte[] dataToSend, ref byte[] dataReceived, uint timeoutForReceive)
        {

            byte[] bufferOutput = new byte[512];
            byte[] bufferInput = new byte[512];
            bool status = true;

            Array.Clear(bufferOutput, 0, bufferOutput.Length);
            Array.Clear(bufferInput, 0, bufferInput.Length);

            status = CheckQuantity(quantityToSend);
            status = CheckQuantity(quantityToReceive);
            status = (hMasterCOM == null) ? false : true;
            status = (hMasterCOM == _invalidHandleValue) ? false : true;
            status = (dataToSend == null) ? false : true;
            status = (dataReceived == null) ? false : true;

            if(!status) return false;

            Array.Clear(bufferOutput, 0, bufferOutput.Length);
            Array.Clear(bufferInput, 0, bufferInput.Length);
            Array.Copy(dataToSend, bufferOutput, quantityToSend);

            if (!WriteData(hMasterCOM, bufferOutput, quantityToSend, ref _quantityOfSent, timeoutForReceive)) return false;
            if (quantityToSend != _quantityOfSent) return false;
            if (ReadData(hMasterCOM, bufferInput, quantityToReceive, ref _quantityOfReceived, timeoutForReceive) != true) return false;
            if (quantityToReceive != _quantityOfReceived) return false;
            Array.Copy(bufferInput, dataReceived, quantityToReceive);

            return true;
        }

        private bool CheckQuantity(uint quantity)
        {
            return (quantity > 0 || quantity < 512) ? true : false;
        }
    }
}