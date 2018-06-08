namespace greenPower.Models
{
    public class Data
    {

        private static Data _instance;
        private static object syncLock=new object();

        private int INT_TYPE = 0;

        private static string[] name = new string[12];
        private static uint[] address = new uint[12];
        private static float[] values = new float[12];
        private static int[] type = new int[12];

        public string[] Name { get => name; set => name = value; }
        public uint[] Address { get => address; set => address = value; }
        public float[] Value { get => values; set => values = value; }
        public int[] Type { get => type; set => type = value; }

        protected Data()
        {
            #region INICJALIZACJA PARAMETRÓW I ADRESÓW

            Name[0] = "iwiatrAF"; Address[0] = 0xb830b;
            Name[1] = "uwiatrVF"; Address[1] = 0xb830c;
            Name[2] = "iwiatrA_buck"; Address[2] = 0xb82ff;
            Name[3] = "uwiatrV_buck"; Address[3] = 0xb82fe; 

            Name[4] = "ipanelAF"; Address[4] = 0xb8307;
            Name[5] = "upanelVF"; Address[5] = 0xb8308;
            Name[6] = "ipanelA_buck"; Address[6] = 0xb8305;
            Name[7] = "upanelV_buck"; Address[7] = 0xb8304;

            Name[8] = "iakuAF"; Address[8] = 0xb8312;
            Name[9] = "uakuVF"; Address[9] = 0xb8311;
            Name[10] = "izasobnikAF"; Address[10] = 0xb82ea;
            Name[11] = "uzasobnikVF"; Address[11] = 0xb82bb;

            foreach (int i in Type) Type[i] = INT_TYPE;

            #endregion
            
        }

        public static Data GetData()
        {
            if(_instance == null)
            {
                lock (syncLock)
                {
                    if(_instance==null)
                    {
                        _instance = new Data();
                    }
                }
            }
            return _instance;
        }
    }
}