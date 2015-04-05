using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using RestSharp;

namespace netaccess
{
    class LibNetAccess
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private bool IsAuthValid { get; set; }

        public void AddCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public bool Authenticate()
        {
            try
            {
                IsAuthValid = false;
                IList<RestResponseCookie> cookies = RetrieveCookie();
                AuthSession(Username, Password, cookies);
                if (!IsAuthValid)
                    return IsAuthValid;
                AuthUsage(cookies, false);
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return false;
            }
        }

        private IList<RestResponseCookie> RetrieveCookie()
        {
            //Gets current cookie from netaccess server
            RestClient authClient = new RestClient("https://netaccess.iitm.ac.in/account/login");
            RestRequest cookieget = new RestRequest(Method.GET);
            RestResponse cookieResponse = (RestResponse)authClient.Execute(cookieget);
            return cookieResponse.Cookies;
        }

        private void AuthSession(string username, string password, IList<RestResponseCookie> cookie)
        {
            //Authorisation module
            RestClient authClient = new RestClient("https://netaccess.iitm.ac.in/account/login");
            RestRequest credentials = new RestRequest(Method.POST);
            credentials.AddCookie(cookie[0].Name, cookie[0].Value);
            credentials.AddParameter("userLogin", username, ParameterType.GetOrPost);
            credentials.AddParameter("userPassword", password, ParameterType.GetOrPost);
            credentials.AddParameter("submit", "", ParameterType.GetOrPost);
            IRestResponse credentialResponse = authClient.Execute(credentials);
            if (credentialResponse.ResponseUri.AbsoluteUri == @"https://netaccess.iitm.ac.in/account/index")
                IsAuthValid = true;
            if (credentialResponse.ResponseUri.AbsoluteUri == @"https://netaccess.iitm.ac.in/account/login")
            {
                IsAuthValid = false;
                MessageBox.Show(@"Wrong username-password combination. Try again!");
            }
        }

        private async void AuthUsage(IList<RestResponseCookie> cookie, bool isOneHour)
        {
            var timeToken = isOneHour ? "1" : "2";
            RestClient authClient = new RestClient("https://netaccess.iitm.ac.in/account/approve");
            RestRequest credentials = new RestRequest(Method.POST);
            credentials.AddCookie(cookie[0].Name, cookie[0].Value);
            credentials.AddParameter("duration", timeToken, ParameterType.GetOrPost);
            credentials.AddParameter("approveBtn", "", ParameterType.GetOrPost);
            IRestResponse credentialResponse = await authClient.ExecuteAwait(credentials);
            if (credentialResponse.StatusCode == HttpStatusCode.OK)
                IsAuthValid = true;
        }
    }
}
