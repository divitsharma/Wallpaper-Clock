using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Wallpaper_Tests
{
    public class Wallpaper
    {
        public string fileName;
        public BitmapImage imgSource;
        public string time;

        public Wallpaper(string name, /*BitmapImage imgSource,*/ int hours, int mins)
        {
            this.fileName = name;
            //this.imgSource = imgSource;
            this.time = hours.ToString() + " : " + mins.ToString();
        }
    }
}
