using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Collections.ObjectModel;
using TestApp2.ViewModels;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Notes : ContentPage
    {
        private ObservableCollection<NotesViewModel> myList;
        public ObservableCollection<NotesViewModel> MyList
        {
            get { return myList; }
            set { myList = value; }
        }   
       
        private string listdata { get; set; }
        private string subnotes { get; set; }       
        public Notes()
        {
            InitializeComponent();
            this.BindingContext = this;           
            MyList = new ObservableCollection<NotesViewModel>();
            ATTLIST.ItemsSource = MyList;
        }
        private async void AddNoteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Note());            
        }
        protected override void OnAppearing()
        {
            this.BindingContext = this;
            MessagingCenter.Subscribe<Note, string>(this, "ListData", async (sender, arg) =>
            {
                listdata = arg;
                
            });
            MessagingCenter.Subscribe<Note, string>(this, "Subnotes", async (sender, arg) =>
            {
                subnotes = arg;

            });
            if (listdata != null)
            {
                MyList.Add(new NotesViewModel() { Listdata = listdata, Subnotes = subnotes });
            }
        }                
    }
}