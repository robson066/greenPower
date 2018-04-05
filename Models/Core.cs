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

namespace GreenPower.Models
{

    public class Core
    {
        RECEIVED Received = new RECEIVED();
        TOSEND ToSend = new TOSEND();

        private readonly BackgroundWorker worker = new BackgroundWorker();
        private double[] Yaxis = new double[50];

        #region Deklaracja i inicjalizacja zmiennych
        
        public static bool IsConnected = false;
        public static bool Exit = false;

        public static int AutoRefreshDelay = 100;

        #endregion

        #region Deklaracja i inicjalizacja stałych

        private const byte _flashEraseSector = 3;
        private const byte _flashWriteData = 4;
        private const byte _runProgramFromFlashOn = 5;
        private const byte _runProgramFromFlashOff = 6;
        private const byte _read12Data = 10;
        private const byte _write1Data = 13;
        private const byte _read1Data = 14;
        private const byte _readBlockData = 15;
        private const byte _writeBlockData = 16;

        public const char Header = '@';
        public const uint _timeToWait = 10000;
        public const uint _timeToWaitFast = 200;

        private const int ErrorOk = 0;
        private const int ErrorMissSh363 = 1;
        private const int ErrorMissUsbConnection = 3;

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

        public static int Error;
        public static bool Success = false;

        private static readonly IntPtr _invalidHandleValue = new IntPtr(-1);
        public static bool runBackgruondWorker = true;

        private static uint _quantityOfSent, _quantityOfReceived;

        #endregion


        private static bool ReadData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesRead, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int status = SiUsbXp.SI_SUCCESS;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(dwTimeout, 0); 
            status = SiUsbXp.SI_Read(handle, buffer, dwSize, ref lpdwBytesRead, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return (status == SiUsbXp.SI_SUCCESS);
        }

        private static bool WriteData(IntPtr handle, byte[] buffer, uint dwSize, ref uint lpdwBytesWritten, uint dwTimeout)
        {
            uint tmpReadTO = 0, tmpWriteTO = 0;
            int status = SiUsbXp.SI_SUCCESS;

            SiUsbXp.SI_GetTimeouts(ref tmpReadTO, ref tmpWriteTO); 
            SiUsbXp.SI_SetTimeouts(0, dwTimeout); 
            status = SiUsbXp.SI_Write(handle, buffer, dwSize, ref lpdwBytesWritten, IntPtr.Zero); 
            SiUsbXp.SI_SetTimeouts(tmpReadTO, tmpWriteTO); 
            return (status == SiUsbXp.SI_SUCCESS);
        }

        public static bool WriteReadUSB(IntPtr hMasterCOM, uint quantityToSend, uint quantityToReceive, ref byte[] dataToSend, ref byte[] dataReceived, uint timeoutForReceive)
        {

            byte[] BufferOutput = new byte[512];
            byte[] BufferInput = new byte[512];

            for (int i = 0; i < 512; i++) BufferOutput[i] = 0;
            for (int i = 0; i < 512; i++) BufferInput[i] = 0;

            Success = false;

            if ((hMasterCOM == null) || (hMasterCOM == _invalidHandleValue))
            {
                Error = ErrorMissSh363;
                return Success;
            }
            else
            {
                if ((quantityToSend < 0) || (quantityToSend > 512)) return Success;
                else
                {
                    for (int i = 0; i < 512; i++) BufferOutput[i] = 0;
                    for (int i = 0; i < 512; i++) BufferInput[i] = 0;

                    if ((dataToSend == null) || (dataReceived == null)) return Success;
                    else
                    {
                        Array.Copy(dataToSend, BufferOutput, quantityToSend);

                        if (!WriteData(hMasterCOM, BufferOutput, quantityToSend, ref _quantityOfSent, timeoutForReceive)) return Success;
                        else
                        {
                            if (quantityToSend != _quantityOfSent) return Success;
                            else
                            {
                                if ((quantityToReceive < 0) || (quantityToReceive > 512)) return Success;
                                else
                                {
                                    if (ReadData(hMasterCOM, BufferInput, quantityToReceive, ref _quantityOfReceived, timeoutForReceive) != true)
                                    {
                                        Error = ErrorMissUsbConnection;
                                        return Success;
                                    }
                                    else
                                    {
                                        if (quantityToReceive != _quantityOfReceived)
                                        {
                                            Error = ErrorMissUsbConnection;
                                            return Success;
                                        }
                                        else
                                        {
                                            Error = ErrorOk;
                                            Array.Copy(BufferInput, dataReceived, quantityToReceive);
                                            Success = true;
                                            return Success;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        public void Connect(short serial_number)
        {
            _status = SiUsbXp.SI_GetNumDevices(ref _lpdwNumDevices);

            if (_status == SiUsbXp.SI_SUCCESS)
            {

                for (uint d = 0; d < _lpdwNumDevices; d++)
                {
                    if (d > 9) break;

                    Error = ErrorOk;
                    if (SiUsbXp.SI_GetProductString(d, _devStr, SiUsbXp.SI_RETURN_LINK_NAME) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _serialStr, SiUsbXp.SI_RETURN_SERIAL_NUMBER) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _vid, SiUsbXp.SI_RETURN_VID) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _pid, SiUsbXp.SI_RETURN_PID) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;
                    if (SiUsbXp.SI_GetProductString(d, _descriptionStr, SiUsbXp.SI_RETURN_DESCRIPTION) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;

                    if (Error == ErrorOk)
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
                Error = ErrorMissSh363;
                //MainWindowViewModel.MessageQueue.Enqueue("Błąd! Nie znaleziono sterownika SH363");
            }

        }

        public unsafe void Read12Variables(uint[] address, ref float[] receivedData, int[] type)
        {
            _quantity = 52;

            DataOut[0] = Convert.ToByte(Header);
            DataOut[1] = _read12Data;
            DataOut[2] = _quantity;
            DataOut[3] = 0;

            int j = 0;
            for (i = 4; i < _quantity; i = i + 4)
            {
                if (i == 4) j = 0;
                ToSend._uint = address[j];
                byte[] tempAddress = new byte[4];
                tempAddress = BitConverter.GetBytes(address[j]);
                j++;
                DataOut[i] = tempAddress[0];
                DataOut[i + 1] = tempAddress[1];
                DataOut[i + 2] = tempAddress[2];
                DataOut[i + 3] = tempAddress[3];
            }

            for (i = 0; i < _quantity + 1; i++) DataIn[i] = 0;

            _checkSum = 0;

            for (i = 4; i < _quantity; i++) _checkSum += DataOut[i];
            for (i = 0; i < 3; i++) _checkSum += DataOut[i];
            DataOut[3] = _checkSum;

            if (SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;

            WriteReadUSB(hMasterCOM, _quantity, _quantity, ref DataOut, ref DataIn, _timeToWait);

            if (Error == ErrorOk)
            {
                if (DataIn[0] != Convert.ToByte(Header) ||
                    DataIn[1] != _read12Data ||
                    DataIn[2] != 52)
                    Error = ErrorMissUsbConnection;
                else
                {

                    for (i = 4; i < _quantity; i = i + 4)
                    {
                        if (i == 4) j = 0;
                        fixed (RECEIVED* p = &Received)
                        {

                            p->_byte[0] = DataIn[i];
                            p->_byte[1] = DataIn[i + 1];
                            p->_byte[2] = DataIn[i + 2];
                            p->_byte[3] = DataIn[i + 3];
                        }

                        receivedData[j] = Received._float;
                        Received._float = 0;
                        j++;
                        if (j > 11) break;
                    }

                }
            }

        }

        public unsafe void Write1Variable(uint address, ref float data, int typ)
        {
            _quantity = 12;

            fixed (TOSEND* d = &ToSend)
            {
                d->_uint = address;
            }

            Received._float = data;

            DataOut[0] = Convert.ToByte(Header);
            DataOut[1] = _write1Data;
            DataOut[2] = _quantity;
            DataOut[3] = 0;

            fixed (RECEIVED* z = &Received)
            {
                DataOut[4] = z->_byte[0];
                DataOut[5] = z->_byte[1];
                DataOut[6] = z->_byte[2];
                DataOut[7] = z->_byte[3];
            }

            fixed (TOSEND* d = &ToSend)
            {
                DataOut[8] = d->_byte[0];
                DataOut[9] = d->_byte[1];
                DataOut[10] = d->_byte[2];
                DataOut[11] = d->_byte[3];
            }

            _checkSum = 0;
            for (i = 4; i < _quantity; i++) _checkSum += DataOut[i];
            for (i = 0; i < 3; i++) _checkSum += DataOut[i];
            DataOut[3] = _checkSum;

            if (SiUsbXp.SI_FlushBuffers(hMasterCOM, 1, 1) != SiUsbXp.SI_SUCCESS) Error = ErrorMissSh363;

            WriteReadUSB(hMasterCOM, _quantity, 4, ref DataOut, ref DataIn, _timeToWait);

            if (Error == ErrorOk)
            {
                if (DataIn[0] != Convert.ToByte(Header) ||
                    DataIn[1] != _write1Data ||
                    DataIn[2] != 4 ||
                    DataIn[3] != 81)
                    Error = ErrorMissUsbConnection;
            }
        }

        public void Initialize()
        {
            #region INICJALIZACJA PARAMETRÓW I ADRESÓW
            Data.VariableName[0] = "iwiatrAF"; Data.VariableAddress[0] = 0xb830b;
            Data.VariableName[1] = "uwiatrVF"; Data.VariableAddress[1] = 0xb830c;
            Data.VariableName[2] = "iwiatrA_buck"; Data.VariableAddress[2] = 0xb82ff;
            Data.VariableName[3] = "uwiatrV_buck"; Data.VariableAddress[3] = 0xb82fe; 

            Data.VariableName[4] = "ipanelAF"; Data.VariableAddress[4] = 0xb8307;
            Data.VariableName[5] = "upanelVF"; Data.VariableAddress[5] = 0xb8308;
            Data.VariableName[6] = "ipanelA_buck"; Data.VariableAddress[6] = 0xb8305;
            Data.VariableName[7] = "upanelV_buck"; Data.VariableAddress[7] = 0xb8304;

            Data.VariableName[8] = "iakuAF"; Data.VariableAddress[8] = 0xb8312;
            Data.VariableName[9] = "uakuVF"; Data.VariableAddress[9] = 0xb8311;
            Data.VariableName[10] = "izasobnikAF"; Data.VariableAddress[10] = 0xb82ea;
            Data.VariableName[11] = "uzasobnikVF"; Data.VariableAddress[11] = 0xb82bb;
            #endregion

            foreach (int i in Data.VariableType) Data.VariableType[i] = 0;
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

                Read12Variables(Data.VariableAddress, ref Data.VariableValue, Data.VariableType);

                Thread.Sleep(Core.AutoRefreshDelay);

            }
        }

        public void UpdateData()
        {
            worker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            worker.RunWorkerAsync();
        }


    }
}
