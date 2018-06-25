using System;
using GreenPower.UsbHandlers;

namespace GreenPower.UsbHandlers
{

    public partial class Core
    {
        public int GetBoardInfo(uint deviceNumber)
        {
            int errorHandler;

            errorHandler = SiUsbXp.SI_GetProductString(deviceNumber, _devStr, SiUsbXp.SI_RETURN_LINK_NAME);
            errorHandler = SiUsbXp.SI_GetProductString(deviceNumber, _serialStr, SiUsbXp.SI_RETURN_SERIAL_NUMBER);
            errorHandler = SiUsbXp.SI_GetProductString(deviceNumber, _vid, SiUsbXp.SI_RETURN_VID);
            errorHandler = SiUsbXp.SI_GetProductString(deviceNumber, _pid, SiUsbXp.SI_RETURN_PID);
            errorHandler = SiUsbXp.SI_GetProductString(deviceNumber, _descriptionStr, SiUsbXp.SI_RETURN_DESCRIPTION);

            return errorHandler;
        }

        private void PrepareHeader(ref byte[] data, byte command, byte checkSum)
        {
            data[0] = Convert.ToByte(_HEADER);
            data[1] = command;
            data[2] = checkSum;
            data[3] = 0;
        }

        private void PrepareByteData(ref byte[] data, float dataSource, int startIndex)
        {
            byte[] byteData = BitConverter.GetBytes(dataSource);

            data[startIndex] = byteData[0];
            data[startIndex + 1] = byteData[1];
            data[startIndex + 2] = byteData[2];
            data[startIndex + 3] = byteData[3];
        }

        private byte PrepareCheckSum(byte[] data, byte amountOfData)
        {
            byte _checkSum = 0;

            for (int i = 4; i < amountOfData; i++) _checkSum += data[i];
            for (int i = 0; i < 3; i++) _checkSum += data[i];

            return _checkSum;
        }
    }
}