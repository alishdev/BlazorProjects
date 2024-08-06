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
                X = 2,
                Y = 2,
                ColSpan = 2,
                RowSpan = 2,
                Color = Color.Aqua,
                BgColor = Color.Black,
                Text = "Burger",
                PopupMessage = false
            },
            new(){ 
                Id = 2,
                X = 5,
                Y = 2,
                ColSpan = 4,
                RowSpan = 4,
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
