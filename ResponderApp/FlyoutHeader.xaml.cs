using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Xml;

namespace ResponderApp
{
    public partial class FlyoutHeader : ContentView
    {
        public FlyoutHeader()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            string email = Preferences.Get("savedUsername", "guest@example.com");
            string name = email.Contains("@") ? email.Split('@')[0] : "Guest";

            NameLabel.Text = name;
            EmailLabel.Text = email;
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}
