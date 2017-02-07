using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.System.UserProfile;
using Windows.Storage;

using Windows.ApplicationModel.Background;

namespace Wallpaper_Tests
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // frame = (Frame)Window.Current.Content;
            //page = (MainPage)frame.Content;
        }

        /*protected async override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("wallsFile.txt");
            string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');

            Tuple<int, int> hourMin = new Tuple<int, int>(DateTime.Now.Hour, DateTime.Now.Minute);

            // GO THROUGH EACH LINE OF FILE
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] pieces = lines[i].Split(':'); // time/name.png
                string[] nums = pieces[0].Split(' '); // hour/min

                //System.Diagnostics.Debug.WriteLine(nums[0] + " " + nums[1]);
                //System.Diagnostics.Debug.WriteLine(hourMin.Item1 + " " + hourMin.Item2);

                // if current time is time in the current wallpaper
                if (hourMin.Item1 == int.Parse(nums[0]) && hourMin.Item2 == int.Parse(nums[1]))
                {
                    // change wallpaper
                    await SetWallpaperAsync(pieces[1]);

                    // Find time until next change
                    if (i + 1 == lines.Length - 1) i = -1; // since last in lines is ""
                    string[] nextPieces = lines[i + 1].Split(':');
                    string[] nextNums = nextPieces[0].Split(' ');

                    int minTillNext =
                        (int.Parse(nextNums[0]) * 60 + int.Parse(nextNums[1])) - (hourMin.Item1 * 60 + hourMin.Item2);
                    if (minTillNext <= 0) minTillNext += 24 * 60;

                    //System.Diagnostics.Debug.WriteLine(minTillNext);

                    RegisterBGTask(minTillNext);
                    break;
                }
            }
        }*/


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
