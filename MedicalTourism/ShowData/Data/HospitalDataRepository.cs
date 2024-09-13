using ShowData.Models;

namespace ShowData.Data
{
    public class HospitalDataRepository : IHospitalDataRepository
    {
        public Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions)
        {
            throw new NotImplementedException();
        }
    }
}
