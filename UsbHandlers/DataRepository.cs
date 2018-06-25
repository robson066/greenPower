namespace GreenPower.UsbHandlers
{
    public class DataRepository
    {
        private static DataRepository _instance;
        private static object syncLock = new object();

        private static string[] name = new string[12];
        private static uint[] address = new uint[12];
        private static float[] dataValue = new float[12];

        public string[] Name { get => name; }
        public uint[] Address { get => address; }
        public float[] DataValue { get => dataValue; set => dataValue = value; }

        protected DataRepository()
        {
            name[0] = "iwiatrAF"; address[0] = 0xb830b;
            name[1] = "uwiatrVF"; address[1] = 0xb830c;
            name[2] = "iwiatrA_buck"; address[2] = 0xb82ff;
            name[3] = "uwiatrV_buck"; address[3] = 0xb82fe; 

            name[4] = "ipanelAF"; address[4] = 0xb8307;
            name[5] = "upanelVF"; address[5] = 0xb8308;
            name[6] = "ipanelA_buck"; address[6] = 0xb8305;
            name[7] = "upanelV_buck"; address[7] = 0xb8304;

            name[8] = "iakuAF"; address[8] = 0xb8312;
            name[9] = "uakuVF"; address[9] = 0xb8311;
            name[10] = "izasobnikAF"; address[10] = 0xb82ea;
            name[11] = "uzasobnikVF"; address[11] = 0xb82bb;
        }

        public static DataRepository GetData()
        {
            if(_instance == null)
            {
                lock (syncLock)
                {
                    if(_instance==null)
                    {
                        _instance = new DataRepository();
                    }
                }
            }

            return _instance;
        }
    }
}