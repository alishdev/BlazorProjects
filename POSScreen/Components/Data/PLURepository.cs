using POSScreen.Components.Models;
using System.Drawing;

namespace POSScreen.Components.Data
{
    public class PLURepository : IPLURepository
    {
        public List<ButtonModel> GetNextPage(int pageId)
        {
            throw new NotImplementedException();
        }

        List<ButtonModel> _topPage = new List<ButtonModel>()
        {
            new(){
                Id = 1,
                Cols = "col-1",
                Rows = "row-1",
                Color = Color.Aqua,
                BgColor = Color.Black,
                Text = "Burger",
                PopupMessage = false
            },
            new(){ 
                Id = 2,
                Cols = "col-3",
                Rows = "row-cols-5",
                BgColor = Color.DarkRed,
                Color = Color.White,
                Text = "Coke",
                PopupMessage = true
            }
        };
        public List<ButtonModel> GetTopPage()
        {
            return _topPage;
        }
    }
}
