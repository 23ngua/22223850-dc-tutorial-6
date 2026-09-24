using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.IO;
using System.Windows.Media.Imaging;

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

        public MainWindow()
        {
            InitializeComponent();

            restClient = new RestClient("http://localhost:5005");   // Connect to Business Web API

            GoButton.IsEnabled = false;
            SearchButton.IsEnabled = false;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RestRequest request = new RestRequest("api/values");

                RestResponse response = await restClient.ExecuteGetAsync(request);

                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    throw new Exception("The Business Web API returned an empty response.");
                }

                if (!response.IsSuccessful)
                {
                    ErrorData error = JsonConvert.DeserializeObject<ErrorData>(response.Content);

                    if (error != null)
                    {
                        throw new Exception(error.exceptionType + ": " + error.message);
                    }

                    throw new Exception("The Business Web API returned an error response.");
                }

                TotalNum.Text = response.Content;

                GoButton.IsEnabled = true;
                SearchButton.IsEnabled = true;
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

        private async void GoButton_Click(object sender, RoutedEventArgs e)  
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
            
            GoButton.IsEnabled = false;
            SearchButton.IsEnabled = false;

            try 
            {
                RestRequest request = new RestRequest("api/getall/" + index.ToString());

                RestResponse response = await restClient.ExecuteGetAsync(request);

                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    throw new Exception("The Business Web API returned an empty response.");
                }

                if (!response.IsSuccessful)
                {
                    ErrorData error = JsonConvert.DeserializeObject<ErrorData>(response.Content);

                    if (error != null)
                    {
                        throw new Exception(error.exceptionType + ": " + error.message);
                    }

                    throw new Exception("The Business Web API returned an error response.");
                }

                DataIntermed data = JsonConvert.DeserializeObject<DataIntermed>(response.Content);

                if (data == null)
                {
                    throw new Exception("The account response could not be deserialized.");
                }

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
            finally
            {
                GoButton.IsEnabled = true;
                SearchButton.IsEnabled = true;
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
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

            try
            {
                DataIntermed data = await SearchBusinessTierAsync(lastName);

                if (data != null)
                {
                    FNameBox.Text = data.fname;
                    LNameBox.Text = data.lname;
                    AcctNoBox.Text = data.acct.ToString();
                    PinBox.Text = data.pin.ToString("D4");
                    BalanceBox.Text = data.bal.ToString("C");

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
                else
                {
                    MessageBox.Show(
                        "No matching last name was found.",
                        "Search Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while searching.\n\n" +
                    ex.Message,
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
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

                SearchProgressBar.IsIndeterminate = false;
            }
        }

        private async Task<DataIntermed> SearchBusinessTierAsync(string lastName)
        {
            SearchData searchData = new SearchData();
            searchData.searchStr = lastName;

            RestRequest request = new RestRequest("api/search", Method.Post);

            string json = JsonConvert.SerializeObject(searchData);

            request.AddStringBody(json, ContentType.Json);

            RestResponse response = await restClient.ExecutePostAsync(request);

            // No matching surname - return 204 No Content
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                throw new Exception("The Business Web API returned an empty response.");
            }

            // Error response - deserialize ErrorData
            if (!response.IsSuccessful)
            {
                ErrorData error = JsonConvert.DeserializeObject<ErrorData>(response.Content);

                if (error != null)
                {
                    throw new Exception(error.exceptionType + ": " + error.message);
                }

                throw new Exception("The Business Web API returned an error response.");
            }

            // Successful response - deserialize DataIntermed
            DataIntermed data = JsonConvert.DeserializeObject<DataIntermed>(response.Content);

            if (data == null)
            {
                throw new Exception("The search response could not be deserialized.");
            }

            return data;
        }
    }
}
