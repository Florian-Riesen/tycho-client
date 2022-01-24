using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class ReadCardViewModel : BaseViewModel
    {
        private string _someData = "";


        public string SomeData
        {
            get => _someData;
            set => SetProperty(ref _someData, value + Environment.NewLine);
        }

        public ICommand ReadTagCommand { get; }

        public ICommand WriteToTagCommand { get; }

        public ReadCardViewModel()
        {
            Title = "Read Card";
            

            ReadTagCommand = new Command(ReadAndWriteTag);
        }
        

        protected override void OnFreeloaderCardScanned(NfcEventArgs e)
        {
            base.OnFreeloaderCardScanned(e);

            SomeData += e.Data?.ToJson() ?? "No freeloader data found!";
        }


        private void ReadAndWriteTag()
        {
            //Nfc.StartListening(); // makes android use this App preferably the next time a tag is presented
            //Nfc.StartPublishing(); // prepares for writing
            //SomeData += "Waiting for tag...";
        }

        
        
    }
}