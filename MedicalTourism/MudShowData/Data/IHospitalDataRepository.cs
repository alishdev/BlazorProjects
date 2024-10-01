using MudShowData.Models;

namespace MudShowData.Data
{
    public interface IHospitalDataRepository
    {
        Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions);
        Task<List<AnthemCompanyModel>> GetAnthemCompanies();
        Task<IEnumerable<ProcedureDataModel>> GetAnthemPlanDetails(string plan);
    }
}
