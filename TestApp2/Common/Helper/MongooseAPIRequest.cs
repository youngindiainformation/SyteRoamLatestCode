using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TestApp2.Common.Helper
{
    public class MongooseAPIRequest
    {
        /* Start-------- Client API informations which we can get it from SyteLine system ------*/
        //string baseURL = "https://mingle-sso.inforcloudsuite.com:443/MCCLINTONENERGY_TRN/as/token.oauth2";
        //string clientId = "MCCLINTONENERGY_TRN~MWdUf2M9KZPS3_jxNeyq_ilQCNuVX0SWf9L7MaJnM7c";
        //string clientSecret = "Z2UOuPA9rWoRQJenkX2KfS1RcPmPYsX4Os3osIVttn8NgGZ81tUkELvgSOQQN_Scl83PgZpYHKOrv_btI2EHlA";
        //string userName = "MCCLINTONENERGY_TRN#jlNKUaCWGLSX86B2U0Ni8MElgv7ub0nT3cgMnFy06sBGhSQE1bZ-TLeS7-bfnKIVlZoRK-l70Fs-WF3oU4PfRg";
        //string password = "N6bERIi-LCvG45mhHGvf6N9JvXHUBfmwBpvGgEn3F_oIBXOHksTmCXgvPeUrghaJA8Wr3TKk0w6BcxQ1qpefxA";
        //string clientCode = "LKFvCSiAQhRsmbrCjmQ1TEpz2aT2oMYdKGSCmAJLH_iwao_-WkJ8Lcs-j8UUTB7hJ2GPkucInCycrImVmBB1nA";

        string baseURL = "";
        string clientId = "";
        string clientSecret = "";
        string userName = "";
        string password = "";
        string clientCode = "";
        /*------------------------------End----------------------------------------------------- */
        public string GenerateClientAccessToken()
        {
            string clinetToken = null;
            try
            {
                HttpClient client = HeadersForAccessTokenGenerate();
                string body = "grant_type=password&password=" + password + "&username=" + userName;
                client.BaseAddress = new Uri(baseURL);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Content = new StringContent(body,
                                                    Encoding.UTF8,
                                                    "application/x-www-form-urlencoded");//CONTENT-TYPE header  

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "password"));
                postData.Add(new KeyValuePair<string, string>("username", userName));
                postData.Add(new KeyValuePair<string, string>("password", password));
                request.Content = new FormUrlEncodedContent(postData);
                HttpResponseMessage tokenResponse = client.PostAsync(baseURL, new FormUrlEncodedContent(postData)).Result;
                var apiResult = tokenResponse.Content.ReadAsStringAsync().Result;
                var apiResultJson = JObject.Parse(apiResult);
                clinetToken = apiResultJson["access_token"].ToString();
                // clinetToken = "eyJraWQiOiJrZzplMGRlNmFiZC1jY2NlLTQyYzEtYjFlNS05ZDkwNjdhMmRkMGMiLCJhbGciOiJSUzI1NiJ9.eyJJZGVudGl0eTIiOiIzYmI3ZjhhOC1jNzZmLTQyNDgtYmMzOS1kZjlkNjI2M2U2N2QiLCJFbmZvcmNlU2NvcGVzRm9yQ2xpZW50IjoiMCIsImlzcyI6Imh0dHBzOi8vbWluZ2xlLXNzby5pbmZvcmNsb3Vkc3VpdGUuY29tL2luZm9yc3RzIiwiSW5mb3JTVFNJc3N1ZWRUeXBlIjoiQVMiLCJjbGllbnRfaWQiOiJpbmZvcn5hT193UHNITUd3QUs3N0JJcGRqcmNGZS03dXdURHNaR3ZBWDNhcGR5NHdrX09JREMiLCJhdWQiOiJodHRwczovL21pbmdsZS1pb25hcGkuaW5mb3JjbG91ZHN1aXRlLmNvbSIsIlRlbmFudCI6Ik1DQ0xJTlRPTkVORVJHWV9UUk4iLCJuYmYiOjE2NjU5OTcwNzAsIlNTT1Nlc3Npb25JRCI6Ik1DQ0xJTlRPTkVORVJHWV9UUk5-YmIxODk5YjQtNzBlNC00YWIxLTlmMWUtYWEzYjlhZmY4YmQ2Iiwic2NvcGUiOiJvcGVuaWQiLCJJRlNBdXRoZW50aWNhdGlvbk1vZGUiOiJDTE9VRF9JREVOVElUSUVTIiwiZ3JhbnRfaWQiOiIwZTQ0YWYzYy1kZDFiLTRiNjktOWU2Zi00ZTVmZjkzMjEwMTMiLCJleHAiOjE2NjYwMDQyNzAsImlhdCI6MTY2NTk5NzA3MCwianRpIjoiYTZmZjE5YzgtYmVkMi00YmFhLTljYTYtMTJjNDEwM2NkZTdkIn0.WyEKds9weH2uFX_ZeCWh8K8SBY84cK3-gfOJO-wvx0nzLnsq2WQuftSSmEnc-S1kN-BtcYgD7ry9Y5DCqKI72mIBuhIcF-V7Dmty4CGU1fFSCuEKccmzWtuMJANwWuN3dvV9X8_U1eiNcSet4phsTzDlj1z7_BMUR6F8idfiadlVa8yMZk-WKDcLBKZPFWqOoDcXdVCmFmiELf9v3WUrzFDflIFt6iskcRQEaLSvVJ0kJC8ZuvoSkw0XpA7EejPPgJKC_hvsdsC3AQhe0IJZXfPjQ-iaiN5euUVgv0RroZSZnSmPkQboUXT4yqtyLO9rxh60XISZH9DXqwZQMQPz2A";

            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
            return clinetToken;

        }
        private HttpClient HeadersForAccessTokenGenerate()
        {
            var apiKeysReq = GetAPIKeysProcessRequest("http://20.127.31.191:8082/api/APIKeys/" + RestApiConstants.CongfigurationGroup.Trim(), "GET");
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
            HttpClient client = new HttpClient(handler);
            try
            {
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                //  client.DefaultRequestHeaders.Add("apikey", apikey);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(
                         System.Text.ASCIIEncoding.ASCII.GetBytes(
                            $"{clientId}:{clientSecret}")));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return client;
        }
        public async void TokenGenerator()
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            String id = "CSI10DEV1_TST~NH25bhsdmW5V8A8iUqnA_iUWUPhfom5bxATxyXjEVHo";
            String secret = "LKFvCSiAQhRsmbrCjmQ1TEpz2aT2oMYdKGSCmAJLH_iwao_-WkJ8Lcs-j8UUTB7hJ2GPkucInCycrImVmBB1nA";

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://mingle-cqa-sso.cqa.inforcloudsuite.com:443/CSI10DEV1_TST/as/token.oauth2");

            var request = new HttpRequestMessage(HttpMethod.Post, "/https://mingle-cqa-sso.cqa.inforcloudsuite.com:443/CSI10DEV1_TST/as/token.oauth2");

            var byteArray = new UTF8Encoding().GetBytes("CSI10DEV1_TST~NH25bhsdmW5V8A8iUqnA_iUWUPhfom5bxATxyXjEVHo:LKFvCSiAQhRsmbrCjmQ1TEpz2aT2oMYdKGSCmAJLH_iwao_-WkJ8Lcs-j8UUTB7hJ2GPkucInCycrImVmBB1nA");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var formData = new List<KeyValuePair<string, string>>();
            formData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            formData.Add(new KeyValuePair<string, string>("username", "CSI10DEV1_TST#0ulj3p-MwM8GHc37hoo1y7-jR-bhsFpxmgmt8seyKLNi2ekzAOToCTVFNnjjk_dmOPA65eYfgWUw2H1gjmE32g"));
            formData.Add(new KeyValuePair<string, string>("password", "158qTaqCnASRtQnml8emotILfdyAmLRJpeqWVYoshHekORdHt8QOoONI7aKyanGfefSq-WSxO2fa77xJfzwXpA"));
            formData.Add(new KeyValuePair<string, string>("client_id", "CSI10DEV1_TST~NH25bhsdmW5V8A8iUqnA_iUWUPhfom5bxATxyXjEVHo"));
            formData.Add(new KeyValuePair<string, string>("client_secret", "LKFvCSiAQhRsmbrCjmQ1TEpz2aT2oMYdKGSCmAJLH_iwao_-WkJ8Lcs-j8UUTB7hJ2GPkucInCycrImVmBB1nA"));
            request.Content = new FormUrlEncodedContent(formData);
            var response = await client.SendAsync(request);
            //   var handler = new HttpClientHandler { Credentials = ... }
            //using (var client = new HttpClient(handler))
            //{
            //    var result = await client.GetAsync(...);
            //}
            //var client = new RestClient("https://mingle-cqa-sso.cqa.inforcloudsuite.com:443/CSI10DEV1_TST/as/token.oauth2");

            //var request = new RestRequest(Method.POST);
            //request.Credentials = new NetworkCredential("CSI10DEV1_TST#0ulj3p-MwM8GHc37hoo1y7-jR-bhsFpxmgmt8seyKLNi2ekzAOToCTVFNnjjk_dmOPA65eYfgWUw2H1gjmE32g", "158qTaqCnASRtQnml8emotILfdyAmLRJpeqWVYoshHekORdHt8QOoONI7aKyanGfefSq-WSxO2fa77xJfzwXpA");
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("application/x-www-form-urlencoded", "grant_type=password&password=158qTaqCnASRtQnml8emotILfdyAmLRJpeqWVYoshHekORdHt8QOoONI7aKyanGfefSq-WSxO2fa77xJfzwXpA&username=CSI10DEV1_TST#0ulj3p-MwM8GHc37hoo1y7-jR-bhsFpxmgmt8seyKLNi2ekzAOToCTVFNnjjk_dmOPA65eYfgWUw2H1gjmE32g&client_id=" + id + "&client_secret=" + secret, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            //dynamic resp = JObject.Parse(response.Content);
            //String token = resp.access_token;
        }
        public string ProcessRestAPIRequest(string strProcessURL, string method, bool isAuthenticateRequet, string requestObject, string AccessToken, int ConfigURl = 0, string configGroup = null)
        {

            //isAuthenticateRequet login->false
            string strJSON = String.Empty;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            string authenticationToken = String.Empty; ;
            //Prepare json object for Product/Authentication
            var requiredJsonObj = requestObject;
            //check request type
            if (isAuthenticateRequet)
                authenticationToken = AccessToken;
            try
            {
                //create request
                request = (HttpWebRequest)WebRequest.Create(strProcessURL);
                //Add required headers
                if (isAuthenticateRequet)
                {
                    //request.Headers.Set("Authorization", authenticationToken); // Add Authorization Token
                    if (ConfigURl == 0)
                    {
                        var clientToken = GenerateClientAccessToken();
                        request.Headers.Set("X-Infor-MongooseConfig", RestApiConstants.CongfigurationGroup.Trim() + "_" + LoginStorage.Site);// Add MongooseConfig Authorization 
                        request.Headers.Set("Authorization", string.Format("Bearer {0}", clientToken));  // Add Authorization Token
                    }
                    else
                    {
                        request.Headers.Set("Authorization", authenticationToken); // Add Authorization Token
                    }
                }
                else
                {
                    var clientToken = GenerateClientAccessToken();
                    request.Headers.Set("Authorization", string.Format("Bearer {0}", clientToken));
                    if (ConfigURl == 0)
                    {
                        request.Headers.Set("X-Infor-MongooseConfig", LoginStorage.Site);// Add MongooseConfig Authorization 
                    }
                }
                request.Method = method;
                request.ContentType = "application/json";
                request.Accept = "application/json";

                if (method.Equals("POST"))
                {
                    if (requiredJsonObj != null)
                    {
                        request.ContentLength = requiredJsonObj.Length;
                        // Writes data to request
                        using (Stream writeStream = request.GetRequestStream())
                        {
                            UTF8Encoding encoding = new UTF8Encoding();
                            byte[] bytes = encoding.GetBytes(requiredJsonObj);
                            writeStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                //get response from API
                response = (HttpWebResponse)request.GetResponse();
                //fetch results

                var apiResult = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //convert into Json
                var apiResultJson = JObject.Parse(apiResult);
                if (!isAuthenticateRequet)
                {
                    if (ConfigURl == 1)
                    {
                        strJSON = apiResultJson["Configurations"].ToString();
                    }
                    else
                    {
                        strJSON = apiResultJson["Token"].ToString();
                    }
                    //var message = "<b>JasciRestAPI PostJob - Success</b> \n\n" + "<b>Authenticate Token Info:</b> \n\n" + "<b>AccessToken :</b> " + strJSON + "\n\n" + " <b>Date Time :</b> " + DateTime.Now;
                    //Telegram(message);
                    // DeveloperEmailNotification("JasciRestAPI Job : Authenticate Token Info ", "Sucess: " + apiResultJson + "<br/>" + " Date Time :" + DateTime.Now,1);

                }

                else
                {
                    strJSON = apiResultJson.ToString();
                }

                return strJSON;

            }
            catch (WebException ex)
            {
                using (WebResponse response1 = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response1;
                    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response1.GetResponseStream())
                    {
                        strJSON = new StreamReader(data).ReadToEnd();
                        response = (System.Net.HttpWebResponse)response1;
                        response1.Close();
                        //  DeveloperEmailNotification
                        strJSON = "";
                    }
                }
            }
            finally
            {
                response.Close();

            }
            return strJSON;
        }


        public string GetAPIKeysProcessRequest(string strProcessURL, string method)
        {

            //isAuthenticateRequet login->false
            string strJSON = String.Empty;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            string authenticationToken = String.Empty; ;
            //Prepare json object for Product/Authentication
            var requiredJsonObj = "";
            //check request type

            try
            {
                //create request
                request = (HttpWebRequest)WebRequest.Create(strProcessURL);
                //Add required headers
                request.Method = method;
                request.ContentType = "application/json";
                request.Accept = "application/json";

                if (method.Equals("POST"))
                {
                    if (requiredJsonObj != null)
                    {
                        request.ContentLength = requiredJsonObj.Length;
                        // Writes data to request
                        using (Stream writeStream = request.GetRequestStream())
                        {
                            UTF8Encoding encoding = new UTF8Encoding();
                            byte[] bytes = encoding.GetBytes(requiredJsonObj);
                            writeStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                //get response from API
                response = (HttpWebResponse)request.GetResponse();
                //fetch results

                var apiResult = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //convert into Json
                var apiResultJson = JObject.Parse(apiResult);
                if (apiResultJson != null)
                {
                    clientId = apiResultJson["Client_Id"].ToString().Trim();
                    clientSecret = apiResultJson["Client_Screet"].ToString().Trim(); ;
                    userName = apiResultJson["SAAK"].ToString().Trim();
                    password = apiResultJson["SASK"].ToString().Trim();
                    clientCode = apiResultJson["TI"].ToString().Trim();
                    baseURL = apiResultJson["PU"].ToString() + apiResultJson["OT"].ToString().Trim();
                    strJSON = apiResultJson.ToString();
                }
                else
                {
                    strJSON = null;
                }

                return strJSON;

            }
            catch (WebException ex)
            {
                using (WebResponse response1 = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response1;
                    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response1.GetResponseStream())
                    {
                        strJSON = new StreamReader(data).ReadToEnd();
                        response = (System.Net.HttpWebResponse)response1;
                        response1.Close();
                        //  DeveloperEmailNotification
                        strJSON = "";
                    }
                }
            }
            finally
            {
                response.Close();

            }
            return strJSON;
        }
    }

}

