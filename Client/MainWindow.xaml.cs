using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Linq.Expressions;
using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.Remoting.Messaging;

using API_Classes;
using Newtonsoft.Json;
using RestSharp;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RestClient restClient; // REST connection to Business Web API

        public delegate bool SearchDelegate(    // Delegate used to run Business Tier surname search asynchronously
            string lastName,
            out uint acctNo,
            out uint pin,
            out int bal,
            out string fName,
            out string lName,
            out byte[] profilePicture);

        public MainWindow()
        {
            InitializeComponent();

            restClient = new RestClient("http://localhost:5005");   // Connect to Business Web API

            try
            {
                RestRequest request = new RestRequest("api/values");

                RestResponse response = restClient.ExecuteGet(request);

                TotalNum.Text = response.Content;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to connect to the Business Web API.\n\n" +
                    ex.Message,
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                GoButton.IsEnabled = false;
                SearchButton.IsEnabled = false;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)  
        {
            int index = 0;

            if (!Int32.TryParse(IndexNum.Text, out index)) 
            {
                MessageBox.Show("Please enter a valid numeric index."); 
                return;
            }

            if (index < 0 || index >= int.Parse(TotalNum.Text))
            {
                MessageBox.Show("Please enter an index between 0 and " + (int.Parse(TotalNum.Text) - 1) + ".");
                return;
            }

            try 
            {
                RestRequest request = new RestRequest("api/getall/" + index.ToString());

                RestResponse response = restClient.ExecuteGet(request);

                DataIntermed data = JsonConvert.DeserializeObject<DataIntermed>(response.Content);

                FNameBox.Text = data.fname;
                LNameBox.Text = data.lname;
                BalanceBox.Text = data.bal.ToString("C");
                AcctNoBox.Text = data.acct.ToString();
                PinBox.Text = data.pin.ToString("D4");

                if (data.profilePicture != null && data.profilePicture.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(data.profilePicture))
                    {
                        BitmapImage bitmap = new BitmapImage();

                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();

                        ProfileImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to retrieve the account.\n\n" + ex.Message,
                    "Request Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Searches database by last name using Business Tier
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string lastName = SearchLastNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Please enter a last name.", "Search Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool validLastName = lastName.All(
                c => char.IsLetter(c) ||
                    c == ' ' ||
                    c == '-' ||
                    c == '\'');

            if (!validLastName)
            {
                MessageBox.Show(
                    "Last name can only contain letters, spaces, hyphens and apostrophes.", 
                    "Search Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            SearchLastNameBox.IsReadOnly = true;
            IndexNum.IsReadOnly = true;

            FNameBox.IsReadOnly = true;
            LNameBox.IsReadOnly = true;
            AcctNoBox.IsReadOnly = true;
            PinBox.IsReadOnly = true;
            BalanceBox.IsReadOnly = true;

            SearchButton.IsEnabled = false;
            GoButton.IsEnabled = false; 

            SearchProgressBar.IsIndeterminate = true;

            // Start asynchronous search.
            SearchDelegate searchDel = SearchBusinessTier;   
            AsyncCallback callbackDel = OnSearchCompletion;     

            uint acctNo;
            uint pin;
            int bal;
            string fName;
            string lName;
            byte[] profilePicture;

            searchDel.BeginInvoke(
                lastName,
                out acctNo,
                out pin,
                out bal,
                out fName,
                out lName,
                out profilePicture,
                callbackDel,
                null);
        }

        private bool SearchBusinessTier(
            string lastName,
            out uint acctNo,
            out uint pin,
            out int bal,
            out string fName,
            out string lName,
            out byte[] profilePicture)
        {
            acctNo = 0;
            pin = 0;
            bal = 0;
            fName = "";
            lName = "";
            profilePicture = null;

            SearchData searchData = new SearchData();
            searchData.searchStr = lastName;

            RestRequest request = new RestRequest("api/search", Method.Post);

            string json = JsonConvert.SerializeObject(searchData);

            request.AddStringBody(json, ContentType.Json);

            RestResponse response = restClient.Execute(request);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                return false;
            }

            DataIntermed data = JsonConvert.DeserializeObject<DataIntermed>(response.Content);

            if (data == null)
            {
                return false;
            }

            acctNo = data.acct;
            pin = data.pin;
            bal = data.bal;
            fName = data.fname;
            lName = data.lname;
            profilePicture = data.profilePicture;

            return true;
        }

        // Runs on worker thread after asynchronous surname search finishes
        private void OnSearchCompletion(IAsyncResult asyncResult)
        {
            AsyncResult asyncObj = (AsyncResult)asyncResult;  // Get info about asynchronous delegate call

            SearchDelegate searchDel = (SearchDelegate)asyncObj.AsyncDelegate;  // Get delegate that originally started the search

            try
            {
                if (asyncObj.EndInvokeCalled == false)  // EndInvoke must only be called once
                {
                    bool found = searchDel.EndInvoke(   // Retrieve completed search result and out parameters
                        out uint acctNo,
                        out uint pin,
                        out int bal,
                        out string fName,
                        out string lName,
                        out byte[] profilePicture,
                        asyncResult);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (found)
                        {
                            // Display matching record
                            FNameBox.Text = fName;
                            LNameBox.Text = lName;
                            AcctNoBox.Text = acctNo.ToString();
                            PinBox.Text = pin.ToString();
                            BalanceBox.Text = bal.ToString("C");

                            // Display returned profile picture
                            if (profilePicture != null && profilePicture.Length > 0)
                            {
                                using (MemoryStream stream = new MemoryStream(profilePicture))
                                {
                                    BitmapImage bitmap = new BitmapImage();

                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.StreamSource = stream;
                                    bitmap.EndInit();

                                    ProfileImage.Source = bitmap;
                                }
                            }
                        }
                        else
                        {
                            // Search has finished = stop progress bar animation before display no-match message
                            SearchProgressBar.IsIndeterminate = false;

                            MessageBox.Show("No matching last name was found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show(
                        "An error occurred while searching.\n\n" + ex.Message,
                        "Search Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }));
            }
            finally
            {
                Dispatcher.Invoke(new Action(() =>  // ALways restore GUI even if search fails
                {
                    SearchLastNameBox.IsReadOnly = false;
                    IndexNum.IsReadOnly = false;

                    FNameBox.IsReadOnly = false;
                    LNameBox.IsReadOnly = false;
                    AcctNoBox.IsReadOnly = false;
                    PinBox.IsReadOnly = false;
                    BalanceBox.IsReadOnly = false;

                    SearchButton.IsEnabled = true;
                    GoButton.IsEnabled = true;

                    // Stop progress bar
                    SearchProgressBar.IsIndeterminate = false;
                }));

                asyncObj.AsyncWaitHandle.Close();   // Clean up asynchronous operation
            }
        }
    }
}
