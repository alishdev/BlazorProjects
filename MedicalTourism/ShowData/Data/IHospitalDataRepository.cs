using ShowData.Models;

namespace ShowData.Data
{
    public interface IHospitalDataRepository
    {
        Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions);
    }
}
