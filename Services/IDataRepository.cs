using greenPower.Entities;

namespace greenPower.Services
{
    public interface IDataRepository
    {
        bool DataExists(uint address);
        Data[] GetAllData();
        Data GetOneData(uint address);
        void WriteOneData(uint address);

    }
}