using MudShowData.Models;

namespace MudShowData.Data
{
    public class HospitalDataRepository : IHospitalDataRepository
    {
        public Task<List<AnthemCompanyModel>> GetAnthemCompanies()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProcedureDataModel>> GetAnthemPlanDetails(string plan)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions)
        {
            throw new NotImplementedException();
        }
    }
}
