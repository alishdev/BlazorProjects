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
                Top = "0px",
                Right = "0px",
                Width = "200px",
                Height = "200px",
                Color = Color.Aqua,
                BgColor = Color.Black,
                Text = "0-0",
                PopupMessage = false
            },
            new(){ 
                Id = 2,
                Top = "220px",
                Right = "0px",
                Width = "200px",
                Height = "200px",
                BgColor = Color.DarkRed,
                Color = Color.White,
                Text = "220-0",
                PopupMessage = true
            },
           new(){
                Id = 3,
                Top = "400px",
                Right = "0px",
                Width = "200px",
                Height = "200px",
                Color = Color.Yellow,
                BgColor = Color.Black,
                Text = "300-0",
                PopupMessage = false
            },
            new(){
                Id = 4,
                Top = "400px",
                Right = "220px",
                Width = "200px",
                Height = "200px",
                BgColor = Color.SpringGreen,
                Color = Color.White,
                Text = "300-220",
                PopupMessage = true
            }
        };
        public List<ButtonModel> GetTopPage()
        {
            return _topPage;
        }
    }
}
