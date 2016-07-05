using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LinqToTwitter;
using WpfApplication2;

namespace TwitterChitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SingleUserAuthorizer authorizer = new SingleUserAuthorizer
        {
            CredentialStore =
                new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey =
                        "emkQidoEKMzuSzXn8S4pzkH9t",
                    ConsumerSecret =
                        "Wa3fgdDvX5Bhtg81XKPLRHrtca4mijzR9M4nInoxEHs5EHT7bF",
                    AccessToken =
                        "2337178069-ZCghMUUfyVYuOfOYz3i9uTYFriBwxeavRoe9yxd",
                    AccessTokenSecret =
                        "pFrqARei0EzKn1MFgXY0X9uE78dRN0wpbZwa6kSVPwzGc"
                }
        };

        private List<Status> currentTweets;

        private void Get200StatusRecent()
        {
            var twitterContext = new TwitterContext(authorizer);

            var tweets = from tweet in twitterContext.Status
                where tweet.Type == StatusType.Home &&
                      tweet.Count == 200
                select tweet;

            currentTweets = tweets.ToList();
            Console.WriteLine("Amount of tweets returned: " + currentTweets.Count);
            amount_returned.Text = currentTweets.Count.ToString();

        }

        private async void GetFollowers()
        {

            var twitterContext = new TwitterContext(authorizer);

            var relationships =
                await
                    (from look in twitterContext.Friendship
                        where look.Type == FriendshipType.Lookup &&
                              look.UserID == "15411837,16761255,14761251,19761290,18761255"
                        select look.Relationships)
                        .SingleOrDefaultAsync();

            if (relationships != null)
                relationships.ForEach(rel => Console.WriteLine(
                    "Relationship to " + rel.ScreenName +
                    ", is Following: " + rel.Following +
                    ", Followed By: " + rel.FollowedBy));
            if (relationships != null)
            {
                relationships.ForEach(rel => listFollowNames.Items.Add(rel.ScreenName));
            }

        }

        public MainWindow()
        {
            InitializeComponent();

            //right list
            Get200StatusRecent();

            foreach (var twit in currentTweets)
            {
                listTweetList.Items.Add(twit.Text);
            }

            //left list

            /*GetSideBarList(GetFollowers()).ForEach(name =>
                listFollowNames.Items.Add(name));
             */
            GetFollowers();
        }

        private async void get_tweets(string selectedItem)
        {
            listTweetList.Items.Clear();

            var twitterContext = new TwitterContext(authorizer);

            string search_q = selectedItem; //this is required you cant put searchBox.Text inside the search rsponse

            Console.WriteLine("CONSOLE PRINT " + search_q);

            var searchResponse =
                await
                    (from search in twitterContext.Search
                        where search.Type == SearchType.Search &&
                              search.Query == search_q
                        select search)
                        .SingleOrDefaultAsync();

            if (searchResponse != null && searchResponse.Statuses != null) //only prints to the console
                searchResponse.Statuses.ForEach(tweet =>
                    Console.WriteLine(
                        "User: {0}, Tweet: {1}",
                        tweet.User.ScreenNameResponse,
                        tweet.Text));

            if (searchResponse != null && searchResponse.Statuses != null) //populate right table
                searchResponse.Statuses.ForEach(tweet =>
                    listTweetList.Items.Add(tweet.Text));
        }

        private async void followers_details(string selectedItem)
        {

            listTweetList.Items.Clear();
            detailsView.Items.Clear();

            var twitterContext = new TwitterContext(authorizer);

            string search_q = selectedItem; //this is required you cant put searchBox.Text inside the search rsponse
            
            if (search_q != String.Empty)
            {
            
            var friendship =
                await
                    (from friend in twitterContext.Friendship
                        where friend.Type == FriendshipType.FollowersList &&
                              friend.ScreenName == search_q
                        select friend)
                        .SingleOrDefaultAsync();

            if (friendship != null && friendship.Users != null)
                friendship.Users.ForEach(friend =>
                    detailsView.Items.Add(friend.ScreenNameResponse));
            //friend.UserIDResponse may be needed as well
        }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            listTweetList.Items.Clear();
            listFollowNames.Items.Clear();

            var twitterContext = new TwitterContext(authorizer);

            var search_q = searchBox.Text; //this is required you cant put searchBox.Text inside the search rsponse

            var searchResponse =
                await
                    (from search in twitterContext.Search
                        where search.Type == SearchType.Search &&
                              search.Query == search_q
                        select search)
                        .SingleOrDefaultAsync();

            if (searchResponse != null && searchResponse.Statuses != null) //only prints to the console
                searchResponse.Statuses.ForEach(tweet =>
                    Console.WriteLine(
                        "User: {0}, Tweet: {1}",
                        tweet.User.ScreenNameResponse,
                        tweet.Text));

            if (searchResponse != null && searchResponse.Statuses != null) //populate right table
                searchResponse.Statuses.ForEach(tweet =>
                    listTweetList.Items.Add(tweet.Text));

            if (searchResponse != null && searchResponse.Statuses != null) //populate left table
                searchResponse.Statuses.ForEach(tweet =>
                    listFollowNames.Items.Add(tweet.User.ScreenNameResponse));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine(listTweetList.Items.GetItemAt(3));
          //  detailsView.Items.Add(listTweetList.SelectedItem);

        }
        

        private void lstFollowNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listFollowNames.SelectedItem as string != null)
            {
                string selected_name = listFollowNames.SelectedItem as string;
                Console.WriteLine("name selected " + selected_name);

                get_tweets(selected_name);
                followers_details(selected_name);
            }
        }

        private void detailsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            DetailsWindow detailswin= new DetailsWindow();
            detailswin.Show();
            //this.Close(); Closes the main window
        }
    }
}
