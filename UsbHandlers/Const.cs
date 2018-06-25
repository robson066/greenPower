namespace GreenPower.UsbHandlers
{
    public partial class Core
    {
        private const byte _FLASHERASESECTOR = 3;
        private const byte _FLASHWRITEDATA = 4;
        private const byte _RUNPROGRAMFROMFLASHON = 5;
        private const byte _RUNPROGRAMFROMFLASHOFF = 6;
        private const byte _READ12DATA = 10;
        private const byte _WRITE1DATA = 13;
        private const byte _READ1DATA = 14;
        private const byte _READBLOCKDATA = 15;
        private const byte _WRITEBLOCKDATA = 16;

        #region Deklaracja i inicjalizacja stałych

        public const char _HEADER = '@';
        public const uint _timeToWait = 10000;
        public const uint _timeToWaitFast = 200;

        private const int ERROR_OK = 0;
        private const int ERROR_MISS_SH363 = 1;
        private const int ERROR_MISS_USB = 3;

        #endregion

    }
}