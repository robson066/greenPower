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
using GreenPower.Models;
using greenPower.Models;
using static GreenPower.Models.UnsafeStruct;

namespace greenPower.Models
{

    public partial class Core
    {
        RECEIVED received = new RECEIVED();
        TOSEND toSend = new TOSEND();
        Data data = new Data();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private double[] Yaxis = new double[50];

        #region Deklaracja i inicjalizacja zmiennych
        
        public static bool IsConnected = false;
        public static bool Exit = false;

        public static int AutoRefreshDelay = 100;

        #endregion



        #region Deklaracja i inicjalizacja zmiennych na potrzeby metod

        private StringBuilder _devStr = new StringBuilder(SiUsbXp.SI_MAX_DEVICE_STRLEN);  
        private StringBuilder _serialStr = new StringBuilder();
        private StringBuilder _descriptionStr = new StringBuilder();
        private StringBuilder _vid = new StringBuilder();
        private StringBuilder _pid = new StringBuilder();

        private int[] _serialNumbers = new int[9];

        public static IntPtr hMasterCOM = IntPtr.Zero;

        private uint _lpdwNumDevices; 
        private int _status; 
        private int _serial;

        public int i;
        private byte _checkSum, _quantity;

        public byte[] DataOut = new byte[512];
        public byte[] DataIn = new byte[512];


        public static bool runBackgruondWorker = true;


        #endregion


        public void Connect(short serial_number)
        {
            _status = SiUsbXp.SI_GetNumDevices(ref _lpdwNumDevices);

            if (_status == SiUsbXp.SI_SUCCESS)
            {

                for (uint d = 0; d < _lpdwNumDevices; d++)
                {
                    if (d > 9) break;

                    ErrorHandler = ERROR_OK;
                    if (SiUsbXp.SI_GetProductString(d, _devStr, SiUsbXp.SI_RETURN_LINK_NAME) != SiUsbXp.SI_SUCCESS) ErrorHandler = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _serialStr, SiUsbXp.SI_RETURN_SERIAL_NUMBER) != SiUsbXp.SI_SUCCESS) ErrorHandler = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _vid, SiUsbXp.SI_RETURN_VID) != SiUsbXp.SI_SUCCESS) ErrorHandler = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _pid, SiUsbXp.SI_RETURN_PID) != SiUsbXp.SI_SUCCESS) ErrorHandler = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _descriptionStr, SiUsbXp.SI_RETURN_DESCRIPTION) != SiUsbXp.SI_SUCCESS) ErrorHandler = ErrorMissSh363;

                    if (ErrorHandler == ERROR_OK)
                    {
                        _serial = Int16.Parse(_serialStr.ToString());

                        if (_serial == serial_number)
                        {
                            if (SiUsbXp.SI_Open(d, ref hMasterCOM) == SiUsbXp.SI_SUCCESS)
                            {
                                //MainWindowViewModel.MessageQueue.Enqueue("Rozpoczęto połączenie");
                                if (_serial > 15) { _status = SiUsbXp.SI_SetBaudRate(hMasterCOM, 921600); 
                                //MainWindowViewModel.MessageQueue.Enqueue("Prędkość połączenia: 921600 Bodów"); 
                                }
                                if (_serial < 16) { _status = SiUsbXp.SI_SetBaudRate(hMasterCOM, 1000000); 
                                //MainWindowViewModel.MessageQueue.Enqueue("Prędkość połączenia: 1000000 Bodów"); 
                                }
                                _status = SiUsbXp.SI_SetLineControl(hMasterCOM, (8 << 8) + (0 << 4) + (0 << 0));
                                _status = SiUsbXp.SI_SetTimeouts(1000, 1000);

                                IsConnected = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                ErrorHandler = ErrorMissSh363;
                //MainWindowViewModel.MessageQueue.Enqueue("Błąd! Nie znaleziono sterownika SH363");
            }

        }


        private void PrepareHeader(ref byte[] data, byte command, byte checkSum)
        {

            data[0] = Convert.ToByte(_HEADER);
            data[1] = command;
            data[2] = checkSum;
            data[3] = 0;

        }
        public void Read12Variables(uint[] address, ref float[] receivedData, int[] type)
        {

            
            byte _quantity = 52; // 4 + 4*12 
            byte _checkSum = 0;            

            PrepareHeader(ref DataOut, _READ12DATA, _quantity);

            for (int i = 4; i < _quantity; i = i + 4)
            {
                int j = i/4;
                byte[] tempAddress = BitConverter.GetBytes(address[j]);

                DataOut[i] = tempAddress[0];
                DataOut[i + 1] = tempAddress[1];
                DataOut[i + 2] = tempAddress[2];
                DataOut[i + 3] = tempAddress[3];
            }

            Array.Clear(DataIn, 0, DataIn.Length);

            DataOut[3] = PrepareCheckSum(DataIn, _quantity);

            ErrorHandler = SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1); if(ErrorHandler != SiUsbXp.SI_SUCCESS) return;

            UsbHandler.WriteReadUSB(hMasterCOM, _quantity, _quantity, ref DataOut, ref DataIn, _timeToWait);

            if (ErrorHandler == ERROR_OK)
            {
                if (DataIn[0] != Convert.ToByte(_HEADER) ||
                    DataIn[1] != _READ12DATA ||
                    DataIn[2] != 52)
                    ErrorHandler = ERROR_MISS_USB;
                else
                {
                    for (i = 4; i < _quantity; i = i + 4)
                    {
                        int j = i/4;
                        float receivedDataTemp = BitConverter.ToInt32(DataIn, i);
                        receivedData[j] = receivedDataTemp;
                        receivedDataTemp = 0;
                        
                        if (j > 11) break;
                    }

                }
            }

        }

        public void Write1Variable(uint address, ref float data, int typ)
        {
            byte _quantity = 12;
            byte[] tempData = BitConverter.GetBytes(data);
            byte[] tempAddress = BitConverter.GetBytes(address);
            
            PrepareHeader(ref DataOut, _WRITE1DATA, _quantity);

            DataOut[3] = PrepareCheckSum(DataOut, _quantity);

            DataOut[4] = tempData[0];
            DataOut[5] = tempData[1];
            DataOut[6] = tempData[2];
            DataOut[7] = tempData[3];

            DataOut[8] = tempAddress[0];
            DataOut[9] = tempAddress[1];
            DataOut[10] = tempAddress[2];
            DataOut[11] = tempAddress[3];
            
            ErrorHandler = SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1); if(ErrorHandler != SiUsbXp.SI_SUCCESS) return;

            UsbHandler.WriteReadUSB(hMasterCOM, _quantity, 4, ref DataOut, ref DataIn, _timeToWait);

            if (ErrorHandler == SiUsbXp.SI_SUCCESS)
            {
                if (DataIn[0] != Convert.ToByte(_HEADER) ||
                    DataIn[1] != _WRITE1DATA ||
                    DataIn[2] != 4 ||
                    DataIn[3] != 81)
                    ErrorHandler = ERROR_MISS_USB;
            }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                if (!Core.runBackgruondWorker) break;

                Read12Variables(data.Address, ref data.Value, data.Type);

                Thread.Sleep(Core.AutoRefreshDelay);

            }
        }

        private byte PrepareCheckSum(byte[] data, byte amountOfData)
        {
            byte _checkSum = 0;

            for (i = 4; i < amountOfData; i++) _checkSum += data[i];
            for (i = 0; i < 3; i++) _checkSum += data[i];

            return _checkSum;
        }

        public void UpdateData()
        {
            worker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            worker.RunWorkerAsync();
        }

        static float[] ConvertByteArrayToFloat(byte[] bytes)
        {
            if(bytes.Length % 4 != 0) throw new ArgumentException();

            float[] floats = new float[bytes.Length/4];
            for(int i = 0; i < floats.Length; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i*4);
            }

            return floats;
        }  

         static float ConvertByteArrayToOneFloat(byte[] bytes)
        {
            if(bytes.Length % 4 != 0) throw new ArgumentException();

            return BitConverter.ToSingle(bytes, 4);;
        }      

        private SiUsbHandler UsbHandler { get; }
        
        public int ErrorHandler { get => errorHandler; set 
        {
            if(value != SiUsbXp.SI_SUCCESS) UpdateData(); // zmienić na metodę do obsługi błedu
            else errorHandler = value; // coś tam
        }
        }

        private int errorHandler;
    
    }

}
