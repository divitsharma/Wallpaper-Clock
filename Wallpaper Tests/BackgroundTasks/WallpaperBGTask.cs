using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;

namespace BackgroundTasks
{
    public sealed class WallpaperBGTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        List<string> rainIcons = new List<string>() { "09d", "10d", "11d", "09n", "10n", "11n" };
        List<string> snowIcons = new List<string>() { "13d", "13n"};


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            RootObject myWeather = await WeatherInfoBGProxy.GetWeather(20, 20);
            if (rainIcons.Contains(myWeather.weather[0].icon))
            {
                if (true)
                {
                    await SetWallpaperAsync("rainPic.png");
                }
            }
            else if (snowIcons.Contains(myWeather.weather[0].icon))
            {
                if (true)
                {
                    await SetWallpaperAsync("snowPic.png");
                }
            }
            else
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("wallsFile.txt");
                string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');

                int i = (int)ApplicationData.Current.LocalSettings.Values["wallIndex"];

                string[] pieces = lines[i].Split(':'); // time/name.png
                string[] nums = pieces[0].Split(' '); // hour/min

                //System.Diagnostics.Debug.WriteLine(nums[0] + " " + nums[1]);
                //System.Diagnostics.Debug.WriteLine(hourMin.Item1 + " " + hourMin.Item2);

                // if current time is time in the current wallpaper
                if (DateTime.Now.Hour >= int.Parse(nums[0]) && DateTime.Now.Minute >= int.Parse(nums[1]))
                {
                    // change wallpaper
                    await SetWallpaperAsync(pieces[1]);
                    if (i == lines.Length - 2) i = -1;
                    i++;
                    ApplicationData.Current.LocalSettings.Values["wallIndex"] = i;
                }


                _deferral.Complete();
            }
        }
        

        // Change wallpaper
        // Pass in a relative path to a file inside the assets folder
        async Task<bool> SetWallpaperAsync(string assetsFileName)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var uri = new Uri("ms-appx:///Assets/" + assetsFileName);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                return await profileSettings.TrySetWallpaperImageAsync(file);
            }
            return false;
        }

    }
}
