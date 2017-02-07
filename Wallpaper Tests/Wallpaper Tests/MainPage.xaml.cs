using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.UserProfile;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Background;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Wallpaper_Tests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<Tuple<int, int>, string> walls = new Dictionary<Tuple<int, int>, string>();
        private string selectedImgName;

        public MainPage()
        {
            InitWallpapers();

            this.InitializeComponent();
            greetingOutput.Text = BackgroundWorkCost.CurrentBackgroundWorkCost.ToString();
        }

        public ObservableCollection<Wallpaper> wallpapers = new ObservableCollection<Wallpaper>();
        public List<int> numList = new List<int>();

        private async void InitWallpapers()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //await (await storageFolder.TryGetItemAsync("wallsFile.txt")).DeleteAsync();

            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("wallsFile.txt") != null)
            {
                // initialize wallpapers list
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("wallsFile.txt");
                string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    string[] pieces = lines[i].Split(':'); // time/name.png
                    string[] nums = pieces[0].Split(' '); // hour/min

                    wallpapers.Add(new Wallpaper(pieces[1],
                            //new BitmapImage(new Uri("ms-appx:///Assets/" + pieces[1])),
                            int.Parse(nums[0]), int.Parse(nums[1])));
                    walls.Add(new Tuple<int, int>(int.Parse(nums[0]), int.Parse(nums[1])), pieces[1]);
                    numList.Add(i);
                }
            }            
        }

        // Choose file and store in walls
        private async void ChooseFile(object sender, RoutedEventArgs e)
        {
            // Set up picker
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Get the assets folder
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            // Pick a file from anywhere
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // If file is not in assets folder
                if (await assetsFolder.TryGetItemAsync(file.Name) == null) //CHANGE TO ==
                {
                    selectedImgName = file.Name;
                    // Copy to assets
                    await file.CopyAsync(assetsFolder, selectedImgName, NameCollisionOption.ReplaceExisting);

                    // SET SOURCE TO REMOVE BITMAP URI WHEN ADD BUTTON IS CLICKED
        //            selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + selectedImgName));

                }
                else if (walls.ContainsValue(file.Name))
                {
                    greetingOutput.Text = "File already added";
                    selectedImgName = "";
                }
                // if in assets folder but not in walls
                else
                {
                    selectedImgName = file.Name;
    //                selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/imgIconBackground.png"));

                    var toDelete = await assetsFolder.GetFileAsync(selectedImgName);
                    await toDelete.DeleteAsync();

                    await file.CopyAsync(assetsFolder, selectedImgName, NameCollisionOption.ReplaceExisting);
     //               selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + selectedImgName));
                }
            }
        }


        // CREATE AND WRITE WALLS INTO THE FILE
        private async void startTaskButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            if (await storageFolder.TryGetItemAsync("wallsFile.txt") != null)
                await (await storageFolder.TryGetItemAsync("wallsFile.txt")).DeleteAsync();

            StorageFile wallsFile = await
                storageFolder.CreateFileAsync("wallsFile.txt", CreationCollisionOption.ReplaceExisting);

            List<string> wallsList = new List<string>();

            // walls -> wallslist
            foreach (var item in walls)
            {
                // leaves with item1 (space) item2
                string[] hour_min = item.Key.ToString().TrimEnd(')').TrimStart('(').Split(',');
                wallsList.Add(hour_min[0] + hour_min[1] + ":" + item.Value);
            }

            await FileIO.WriteLinesAsync(wallsFile, wallsList); // MUST BE IN ORDER


            RequestBackgroundAccess();
        }


        // CREATING TASK --------------------------------------------------
        private async void RequestBackgroundAccess()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();
            greetingOutput.Text = result.ToString();
            if (result != BackgroundAccessStatus.DeniedByUser) RegisterBackgroundTask();
        }

        // REGISTER TASK
        private void RegisterBackgroundTask()
        {
            // Unregister old task
            foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
                if (bgTask.Value.Name == "BackgroundTrigger")
                    bgTask.Value.Unregister(true);

            // Register new task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = "BackgroundTrigger";
            builder.TaskEntryPoint = "BackgroundTasks.WallpaperBGTask";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected));

            BackgroundTaskRegistration task = builder.Register();
        }

        // DELETE LAST IMAGE
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            var name = wallpapers[wallpapers.Count - 1].fileName;

            //wallpapers[wallpapers.Count - 1].imgSource.UriSource = null;
            //((Wallpaper)imagesPanel.Items[wallpapers.Count - 1]).imgSource.UriSource = null;

            wallpapers[wallpapers.Count - 1] = null;
            selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/imgIconBackground.png"));
            wallpapers.RemoveAt(wallpapers.Count - 1);

            // remove from walls
            walls.Remove(walls.Keys.Last());
            numList.Remove(numList.Count - 1);

            if (name != null && await assetsFolder.TryGetItemAsync(name) != null)
            {
                StorageFile toRemove = await assetsFolder.GetFileAsync(name);
                await toRemove.DeleteAsync();
            }
        }

        private void Button_Add(object sender, RoutedEventArgs e)
        {
            BitmapImage sImgSource = (BitmapImage)selectImg.Source;

    //        if (sImgSource.UriSource.LocalPath != "/Assets/imgIconBackground.png")
            {
                // Add to Dictionary
                if (!walls.ContainsKey(new Tuple<int, int>(selectTime.Time.Hours, selectTime.Time.Minutes)))
                {
                    walls.Add(new Tuple<int, int>(selectTime.Time.Hours, selectTime.Time.Minutes), selectedImgName);
                    // SET SOURCE TO REMOVE BITMAP URI
                    selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/imgIconBackground.png"));

                    // add to observed collection
                    wallpapers.Add(new Wallpaper(selectedImgName,
                        //new BitmapImage(new Uri("ms-appx:///Assets/" + selectedImgName)), no image to wallpaper
                        selectTime.Time.Hours, selectTime.Time.Minutes));

                    numList.Add(numList.Count);
                }
                else
                    greetingOutput.Text = "Already added";              
            }
        }

        private void StopTask_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
                if (bgTask.Value.Name == "BackgroundTrigger")
                    bgTask.Value.Unregister(true);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Identify first wallpaper to be changed
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["wallIndex"] = ((ComboBox)sender).SelectedValue;
        }

        private async void TestWeather_Click(object sender, RoutedEventArgs e)
        {

            // weather testing - TEMP IN KELVIN
            RootObject myWeather = await WeatherInfoProxy.GetWeather(1, 2);

            greetingOutput.Text = myWeather.name + " - " + myWeather.weather[0].icon + " = " + myWeather.weather[0].description;
        }

        private async void inputRainButton_Click(object sender, RoutedEventArgs e)
        {
            // Set up picker
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Get the assets folder
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            // Pick a file from anywhere
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                rainPicName.Text = file.Name;

                // Copy to assets
                await file.CopyAsync(assetsFolder, "rainPic.png", NameCollisionOption.ReplaceExisting);
            }
        }

        private async void inputSnowButton_Click(object sender, RoutedEventArgs e)
        {
            // Set up picker
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Get the assets folder
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            // Pick a file from anywhere
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                snowPicName.Text = file.Name;

                // Copy to assets
                await file.CopyAsync(assetsFolder, "snowPic.png", NameCollisionOption.ReplaceExisting);
            }
        }

        private async void deleteRainBtn_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            if (await assetsFolder.TryGetItemAsync("rainPic.png") != null)
            {
                StorageFile toRemove = await assetsFolder.GetFileAsync("rainPic.png");
                await toRemove.DeleteAsync();
            }
        }

        private async void deleteSnowBtn_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");

            if (await assetsFolder.TryGetItemAsync("snowPic.png") != null)
            {
                StorageFile toRemove = await assetsFolder.GetFileAsync("snowPic.png");
                await toRemove.DeleteAsync();
            }
        }

    }
}
