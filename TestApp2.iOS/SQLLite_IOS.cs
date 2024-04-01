using System;

using Xamarin.Forms;

namespace TestApp2.iOS
{
    public class SQLLite_IOS : ContentPage
    {
        public SQLLite_IOS()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

