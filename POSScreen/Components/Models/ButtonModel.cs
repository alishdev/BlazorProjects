using System.Drawing;

namespace POSScreen.Components.Models
{
    public class ButtonModel
    {
        public int Id { get; set; }
        public string Top { get; set; }
        public string Width { get; set; }
        public string Right { get; set; }
        public string Height { get; set; }
        public Color BgColor { get; set; }
        public Color Color { get; set; }
        public string Text { get; set; }
        public bool PopupMessage { get; set; }  // if true, pop up message, if false open another page
    }
}
