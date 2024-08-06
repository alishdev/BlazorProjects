using POSScreen.Components.Models;

namespace POSScreen.Components.Data
{
    public interface IPLURepository
    {
        List<ButtonModel> GetTopPage();
        List<ButtonModel> GetNextPage(int pageId);

    }
}
