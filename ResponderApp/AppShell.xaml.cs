using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace ResponderApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var fullName = Preferences.Get("ResponderName", "ResponderName");
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.FindByName<Label>("UserNameLabel") is Label userNameLabel)
                {
                    userNameLabel.Text = fullName;
                }
            });

            Routing.RegisterRoute("home", typeof(HomePage));
            Routing.RegisterRoute("report", typeof(ReportPage));
            Routing.RegisterRoute("assessment", typeof(AssessmentPage));
        }

        // This will handle the profile icon click event
        private async void OnProfileIconClicked(object sender, EventArgs e)
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "Cancel");
            if (confirm)
            {
                await App.LogoutAndRestartAppAsync();
            }
        }

        // Logout MenuItem Clicked Event
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "Cancel");
            if (confirm)
            {
                await App.LogoutAndRestartAppAsync();
            }
        }
    }
}
