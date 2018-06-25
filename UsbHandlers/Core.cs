using System;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using GreenPower.UsbHandlers;

namespace GreenPower.UsbHandlers
{
    public partial class Core
    {
        public void Connect(short serialNum)
        {
            uint lpdwNumDevices = 0;

            ErrorHandler = SiUsbXp.SI_GetNumDevices(ref lpdwNumDevices);

            for (uint boardNumber = 0; boardNumber < lpdwNumDevices; boardNumber++)
            {
                if (boardNumber > 9) break;

                ErrorHandler = GetBoardInfo(boardNumber);

                if (ErrorHandler == SiUsbXp.SI_SUCCESS)
                {
                    int serial = Int16.Parse(_serialStr.ToString());

                    if (serial == serialNum)
                    {
                        ErrorHandler = SiUsbXp.SI_Open(boardNumber, ref hMasterCOM);

                        if (serial > 15) 
                        {
                            ErrorHandler = SiUsbXp.SI_SetBaudRate(hMasterCOM, 921600); 
                        }

                        if (serial < 16) 
                        { 
                            ErrorHandler = SiUsbXp.SI_SetBaudRate(hMasterCOM, 1000000); 
                        }

                        ErrorHandler = SiUsbXp.SI_SetLineControl(hMasterCOM, (8 << 8) + (0 << 4) + (0 << 0));
                        ErrorHandler = SiUsbXp.SI_SetTimeouts(1000, 1000);

                        connectionStatus = true;
                    }
                }
            }

        }

        public void Read12Data(uint[] address)
        {

            byte amount = 52; // 4 + 4*12          

            PrepareHeader(ref dataOut, _READ12DATA, amount);

            for(int i = 1; i < amount/4; i++)
            {
                PrepareByteData(ref dataOut, address[i], i*4);                
            }

            Array.Clear(dataIn, 0, dataIn.Length);

            byte _checkSum = PrepareCheckSum(dataIn, amount);
            dataOut[3] = _checkSum;

            ErrorHandler = SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1); 
            ErrorHandler = UsbHandler.WriteReadUSB(hMasterCOM, amount, amount, ref dataOut, ref dataIn, _timeToWait);

            // TODO: check error

            if (CheckReturnedHeader(dataIn, _READ12DATA, amount))
                ErrorHandler = ERROR_MISS_USB;
            else
            {
                for (int i = 1; i < amount/4; i++)
                {
                    data.DataValue[i*4] = BitConverter.ToInt32(dataIn, i);
                    if (i > 11) break;
                }
            }
        }

        private bool CheckReturnedHeader(byte[] data, byte command, byte amount)
        {
            bool status = true;

            status = dataIn[0] == Convert.ToByte(_HEADER);
            status = dataIn[1] == _READ12DATA;
            status = dataIn[2] == amount;

            return status;
        }

        public void Write1Data(uint address, ref float dataToWrite, int typ)
        {
            byte amount = 12;
            
            PrepareHeader(ref dataOut, _WRITE1DATA, amount);

            byte _checkSum = PrepareCheckSum(dataOut, amount);
            dataOut[3] = _checkSum;

            PrepareByteData(ref dataOut, dataToWrite, 4);
            PrepareByteData(ref dataOut, address, 8);
            
            ErrorHandler = SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1); 
            if(ErrorHandler != SiUsbXp.SI_SUCCESS) return;

            ErrorHandler = UsbHandler.WriteReadUSB(hMasterCOM, amount, 4, ref dataOut, ref dataIn, _timeToWait);
            if(ErrorHandler != SiUsbXp.SI_SUCCESS) return;

            if (CheckReturnedHeader(dataIn, _WRITE1DATA, amount) ||
                dataIn[3] == 81) // _checkSum ?
                ErrorHandler = SiUsbXp.SI_SUCCESS;
            else
            {
                ErrorHandler = SiUsbXp.SI_WRITE_ERROR;
            }
            
        }

        private void WorkerHandler(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                if (!Core.runBackgruondWorker) break;

                Read12Data(data.Address);
                Thread.Sleep(refreshDelay);
            }
        }

        public void UpdateData()
        {
            worker.DoWork += new DoWorkEventHandler(WorkerHandler);
            worker.RunWorkerAsync();
        }     

        private SiUsbHandler UsbHandler { get; }
        
        public int ErrorHandler { get => errorHandler; set 
        {
            errorHandler = value; 
            if(errorHandler != SiUsbXp.SI_SUCCESS) UpdateData(); // TODO: change to error handler
        }
        }

        private int errorHandler;
        private Data data = Data.GetData();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        
        public bool connectionStatus = false;

        public const int refreshDelay = 100;

        public static IntPtr hMasterCOM = IntPtr.Zero;
        private byte[] dataOut = new byte[512];
        private byte[] dataIn = new byte[512];

        public static bool runBackgruondWorker = true;
        
        private StringBuilder _devStr = new StringBuilder(SiUsbXp.SI_MAX_DEVICE_STRLEN);  
        private StringBuilder _serialStr = new StringBuilder();
        private StringBuilder _descriptionStr = new StringBuilder();
        private StringBuilder _vid = new StringBuilder();
        private StringBuilder _pid = new StringBuilder();
    
    }

}
