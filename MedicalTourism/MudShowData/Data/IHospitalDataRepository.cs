using MudShowData.Models;

namespace MudShowData.Data
{
    public interface IHospitalDataRepository
    {
        Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions);
    }
}
