using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleParser.Domain;
using SimpleParser.Domain.Entities;
using SimpleParser.Domain.Enums;
using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SimpleParser.App
{
    internal class Program
    {
        static ApplicationDbContext _dbContext;
        static int _size;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _dbContext = new ApplicationDbContext(connectionString);
            _size = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Size"));

            Timer timer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Timer_Elapsed(null, null);

            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Host", "www.lesegais.ru");
                client.DefaultRequestHeaders.Add("User-Agent", "0");

                int number = 0;
                JToken json = default;
                while (json == null || json.HasValues)
                {
                    StringContent payload = new StringContent($"{{\"query\": \"query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {{ searchReportWoodDeal(filter: $filter, pageable: {{number: $number, size: $size}}, orders: $orders) {{ content {{ sellerName sellerInn buyerName buyerInn woodVolumeBuyer woodVolumeSeller dealDate dealNumber __typename }} __typename }}}}\", \"variables\": {{ \"size\": {_size}, \"number\": {number++}, \"filter\": null, \"orders\": null }}, \"operationName\": \"SearchReportWoodDeal\"}}", Encoding.UTF8, "json/application");
                    string response = client.PostAsync("https://www.lesegais.ru/open-area/graphql", payload).Result.Content.ReadAsStringAsync().Result;
                    
                    json = JObject.Parse(response)["data"]["searchReportWoodDeal"]["content"];
                    WoodDeal[] woodDeals = JsonConvert.DeserializeObject<WoodDeal[]>(json.ToString());

                    Parallel.ForEach(woodDeals, deal =>
                    {
                        deal.SellerName = deal.SellerName?.Replace("'", "''");
                        deal.BuyerName = deal.BuyerName?.Replace("'", "''");

                        EntityState state = _dbContext.GetEntityState(deal);
                        if (state == EntityState.Exists) return;

                        if (state == EntityState.Modified)
                            _dbContext.UpdateWoodDeal(deal);
                        else
                            _dbContext.AddWoodDeal(deal);
                    });

                    _dbContext.SaveChanges();
                }
            }
        }
    }
}