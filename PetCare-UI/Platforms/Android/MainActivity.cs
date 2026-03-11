using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace PetCare.UI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Hide the status bar
        Window?.AddFlags(WindowManagerFlags.Fullscreen);
        Window?.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
    }
}
