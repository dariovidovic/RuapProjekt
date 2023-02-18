using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace RUAP_Projekt
{

    class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class Output1
    {
        public string type { get; set; }
        public Value value { get; set; }
    }

    public class Results
    {
        public Output1 output1 { get; set; }
    }

    public class Root
    {
        public Results Results { get; set; }
    }

    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Values { get; set; }
    }



    class Program
    {
        static void Main(string[] args)
        {
            string url;

            //-1 = legitimate, 0 = suspicous, 1 = phishing

            int having_ip_address;
            int url_Length;
            int shortening_Services;
            int having_At_Symbol;
            int double_slash_redirecting;
            int preffix_suffix;
            int url_Of_Anchor;
            int on_Mouse_over;
            int right_Click;
            int iframe;
            int having_Sub_Domain;
            int domain_Registration_Length;
            int protocol;

            float differencePercentage;
            int slashIndex;


            Console.WriteLine("Please enter the URL: \n");
            do
            {
                url = Console.ReadLine();
                if ((url.StartsWith("www") || url.StartsWith("http") || url.StartsWith("https")))
                    continue;
                else
                    Console.WriteLine("\nInvalid URL, please try again.\n");


            } while (!(url.StartsWith("www") || url.StartsWith("http") || url.StartsWith("https")));

            Console.WriteLine("\nURL submitted!");

            //--------------------UNOS PODATAKA KOJI SE NE MOGU DOHVATITI NA TEMELJU URL-A--------------------//

            do
            {
                Console.WriteLine("\nIs the real URL equal to the hovered URL?\nEnter -1 if it is\nEnter 1 if it isn't\n");
                int.TryParse(Console.ReadLine(), out on_Mouse_over);

            } while (!(on_Mouse_over == 1 || on_Mouse_over == -1));

            do
            {
                Console.WriteLine("\nCan you view the source code of the web page with the right click?\nEnter -1 if you can\nEnter 1 if you can't.\n");
                int.TryParse(Console.ReadLine(), out right_Click);

            } while (!(right_Click == 1 || right_Click == -1));

            do
            {
                Console.WriteLine("\nIs the page's domain paid for more than 1 year?\nEnter -1 if it is\nEnter 1 if it isn't.\n");
                int.TryParse(Console.ReadLine(), out domain_Registration_Length);

            } while (!(domain_Registration_Length == 1 || domain_Registration_Length == -1));

            do
            {
                Console.WriteLine("\nDoes the page use HTTPS protocol?\nEnter -1 if does\nEnter 1 if doesn't.\n");
                int.TryParse(Console.ReadLine(), out protocol);
            } while (!(protocol == 1 || protocol == -1));


            if (protocol == -1)
            {
                do
                {
                    Console.WriteLine("\nNow, is the page's age of certificate over 1 year? \nEnter -1 if it is\nEnter 0 if the issuer is not trusted.\n");
                    int.TryParse(Console.ReadLine(), out protocol);
                } while (!(protocol == -1 || protocol == 0));

            }



            if (url.Length < 54) url_Length = -1;
            else if (url.Length >= 54 && url.Length <= 75) url_Length = 0;
            else url_Length = 1;


            if (url.Contains("0x"))
                having_ip_address = 1;
            else
            {
                var match = Regex.Match(url, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                if (match.Success) having_ip_address = 1;
                else having_ip_address = -1;
            }


            if (url.Contains("bit.ly") || url.Contains("tinyurl")) shortening_Services = 1;
            else shortening_Services = -1;


            if (url.Contains("@")) having_At_Symbol = 1;
            else having_At_Symbol = -1;


            slashIndex = url.LastIndexOf('/');
            if (slashIndex > 7) double_slash_redirecting = 1;
            else double_slash_redirecting = -1;

            if (url.Contains("-")) preffix_suffix = 1;
            else preffix_suffix = -1;

            //--------------------DOHVACANJE HTML TAGOVA--------------------//

            var urlAnchor = @"<a href=" + url + "></a>";
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(urlAnchor);
            var tempValue = document.DocumentNode.SelectSingleNode("//a");
            var link = tempValue.Attributes["href"].Value;


            differencePercentage = (link.Length - url.Length) * 100;

            if (differencePercentage < 31)
                url_Of_Anchor = -1;
            else if (differencePercentage >= 31 && differencePercentage <= 67)
                url_Of_Anchor = 0;
            else
                url_Of_Anchor = 1;


            //Ukoliko se može dohvatiti frameBorder s iframe HTML tag, koristi se metoda phishinga
            //Ukoliko nema iframe argument dogoditi će se catch block, argument iframe se postavlja na 1 (legitimate)

            try
            {

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                var iframeNode = doc.DocumentNode.SelectNodes("//iframe[@frameborder]").LastOrDefault();
                var iframeValue = iframeNode.Attributes["frameborder"].Value;

                iframe = 1;

            }
            catch
            {
                iframe = -1;

            }


            //Pretpostavka da svaki URL ce imat top level domenu (.com, .hr itd.), ona se mora izbrisati tako da se mogu prebrojati preostale tocke
            //Brisanje top level domene ce se prikazati kao jedan character (.) manje;  www. bez top level domene se prikazuje kao legitimate (1 tocka u URL)

            int frequency = url.Count(f => (f == '.'));
            int dotsCount = frequency - 1;

            if (dotsCount == 1)
                having_Sub_Domain = -1;
            else if (dotsCount == 2)
                having_Sub_Domain = 0;
            else
                having_Sub_Domain = 1;


            InvokeRequestResponseService(new string[,] { { having_ip_address.ToString(), url_Length.ToString(), shortening_Services.ToString(), having_At_Symbol.ToString(), double_slash_redirecting.ToString(), preffix_suffix.ToString(), having_Sub_Domain.ToString(), protocol.ToString(), domain_Registration_Length.ToString(), url_Of_Anchor.ToString(), on_Mouse_over.ToString(), right_Click.ToString(), iframe.ToString(), "-1" } }).Wait();



        }


        static async Task InvokeRequestResponseService(string[,] mainValues)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"having_IP_Address", "URL_Length", "Shortining_Service", "having_At_Symbol", "double_slash_redirecting", "Prefix_Suffix", "having_Sub_Domain", "SSLfinal_State", "Domain_registeration_length", "URL_of_Anchor", "on_mouseover", "RightClick", "Iframe", "Result"},
                                Values = mainValues
                            }

                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "442ANm2tlDxRW1CWTCGfY7w4u3TDSOfuEpqCTgLIVkjMvQQDN95md6Gt6czV+J1swWYfz+stqE28+AMCIyhEjg==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/d4dc0e77b00e4ea89457e28d2b9efb9b/services/a89ebc1afadb410689729f48972c173f/execute?api-version=2.0&details=true%22");

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    Root result = await response.Content.ReadAsAsync<Root>();
                    Console.WriteLine("\nResult: {0}", result.Results.output1.value.Values[0][0]);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }



    }
}
