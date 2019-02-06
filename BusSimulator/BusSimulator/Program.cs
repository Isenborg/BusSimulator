using System;
using System.IO;
using System.Collections.Generic;
using IronWebScraper;
using Newtonsoft.Json;

namespace Bussen
{
    class Program
    {
        static void Main(string[] args)
        {
            Buss.run();
        }
    }

















    class Buss
    {
        public static void run()
        {
            BusManaging.StartupMessage();
            Console.ReadKey();
            Console.Clear();
            List<string> Passenger = new List<string>();
            Random r = new Random();
            var NameScraper = new NameScraper();
            NameScraper.Start();
            var JobScraper = new JobScraper();
            JobScraper.Start();
            Console.WriteLine("\nWaiting for program to begin...");
            System.Threading.Thread.Sleep(2500);
            Console.Clear();
            int StationNumber = 0;
            do
            {
                BusManaging.BusStation(StationNumber);
                if (Passenger.Count < 25)
                {
                    Passenger.AddRange(BusManaging.PassengerHandler());
                }
                Console.WriteLine("Passengerlist: ");
                foreach (var info in Passenger)
                {
                    Console.WriteLine(info);
                }

                for (int i = 0; i < r.Next(0, 15); i++)
                {
                    int index = 0;
                    int count = 0;
                    if (Passenger.Count > 0)
                    {
                        index = r.Next(0, Passenger.Count - 1);
                        count = 1;
                    }
                    else
                    {
                        index = 0;
                        count = 0;
                    }
                    Passenger.RemoveRange(index, count);
                }
                System.Threading.Thread.Sleep(5 * 1000);
                //Console.ReadKey();
                Console.Clear();
                if (StationNumber < 11) StationNumber++;
                else StationNumber = 0;

            } while (true);
        }
    }

    class BusManaging
    {
        public static Random r = new Random();

        public static string GeneratePerson()
        {
            int age = AgeRandomizer();
            string gender = GenderRandomizer();
            string name = NameRandomizer(gender);
            string job = JobRandomizer(age);
            int Income = IncomeRandomizer(age, job);
            return ($"Name: {name} | Age: {age} | Gender: {gender} | Salary: {Income}kr | Job: {job}");
        }

        public static void BusStation(int StationNumber)
        {
            string[] Station = new string[13];
            Station[0] = "Brommaplan";
            Station[1] = "Abrahamsberg";
            Station[2] = "Stora mossen";
            Station[3] = "Alvik";
            Station[4] = "Kristineberg";
            Station[5] = "Thorhildsplan";
            Station[6] = "Fridhemsplan";
            Station[7] = "St:eriksplan";
            Station[8] = "Odenplan";
            Station[9] = "Rådmansgatan";
            Station[10] = "Hötorget";
            Station[11] = "T-centralen";
            Console.WriteLine($"Current station: {Station[StationNumber]} | Next station: {Station[StationNumber + 1]}");
        }

        public static List<string> PassengerHandler()
        {
            List<string> Passenger = new List<string>();
            for (int i = 0; i < r.Next(0, 15); i++)
            {
                Passenger.Add(BusManaging.GeneratePerson());
            }
            return Passenger;
        }

        public static void StartupMessage()
        {
            Console.WriteLine("Welcome to the Bus Simulator!\nPress any button to start the Bus.");
        }

        public static string GenderRandomizer()
        {
            String gender;
            if (r.Next(1, 3) == 1) gender = "Man";
            else gender = "Woman";
            return gender;
        }

        public static int AgeRandomizer()
        {
            int Age = 0;
            if (r.Next(1, 50) > 10) Age = r.Next(12, 60);
            else
            {
                if (r.Next(1, 3) == 1) Age = r.Next(1, 12);
                else Age = r.Next(61, 90);
            }
            return Age;
        }

        public static string NameRandomizer(string gender)
        {
            string Name;
            if (gender == "Man")
                Name = BoyNames(r.Next(101, 201));
            else Name = GirlNames(r.Next(0, 101));
            return Name;
        }


        public static string BoyNames(int pos)
        {
            string[] json;
            json = File.ReadAllLines("c:Scrape/Names.json");
            JsonData name = JsonConvert.DeserializeObject<JsonData>(json[pos]);
            return name.Name;
        }

        public static string GirlNames(int pos)
        {
            string[] json;
            json = File.ReadAllLines("c:Scrape/Names.json");
            JsonData name = JsonConvert.DeserializeObject<JsonData>(json[pos]);
            return name.Name;
        }

        public static string JobRandomizer(int age)
        {
            string Job;
            if (age < 20) Job = "unemployed";
            else if (age > 65) Job = "pensioner";
            else Job = Jobs(r.Next(2, 93));

            return Job;
        }

        public static string Jobs(int pos)
        {
            string[] json;
            json = File.ReadAllLines("c:Scrape/Jobs.json");
            JsonData job = JsonConvert.DeserializeObject<JsonData>(json[pos]);

            return job.Job;
        }

        public static int IncomeRandomizer(int age, string job)
        {
            double income = 0;
            if (job == "unemployed") return 1250;
            else if (job == "pensioner") return 25000;
            else income = Math.Pow(age, 2) * Math.Sqrt(age) + 20000;

            return (int)income;
        }
    }




    public class JsonData
    {
        public string Name { get; set; }
        public string Job { get; set; }
    }





    public class NameScraper : WebScraper
    {
        string url = "https://svenskanamn.alltforforaldrar.se/namn/namntoppen/2000";
        public override void Init()
        {
            if (!File.Exists("c:scrape/Names.json"))
            {
                this.LoggingLevel = WebScraper.LogLevel.All;
                this.Request(url, Parse);
            }
            else Console.WriteLine("Names Have already been scraped!");
        }

        public override void Parse(Response response)
        {

            foreach (var Name_link in response.Css("table u"))
            {
                //save results to file
                Scrape(new ScrapedData() { { "name", Name_link.TextContentClean } }, "Names.json");
            }
            if (response.CssExists("div.prev-post > a[href]"))
            {
                // Get Link URL
                var next_page = response.Css("div.prev-post > a[href]")[0].Attributes["href"];
                // Scrape Next URL
                this.Request(next_page, Parse);
            }
        }
    }

    class JobScraper : WebScraper
    {
        string url = "https://foxhugh.com/list-of-lists/list-of-common-jobs/";
        public override void Init()
        {
            if (!File.Exists("c:scrape/Jobs.json"))
            {
                this.LoggingLevel = WebScraper.LogLevel.All;
                this.Request(url, Parse);
            }
            else Console.WriteLine("Jobs Have already been scraped!");
        }

        public override void Parse(Response response)
        {

            foreach (var Jobs_link in response.Css("div p"))
            {
                //save results to file
                Scrape(new ScrapedData() { { "job", Jobs_link.TextContentClean } }, "Jobs.json");
            }
            if (response.CssExists("div.prev-post > a[href]"))
            {
                // Get Link URL
                var next_page = response.Css("div.prev-post > a[href]")[0].Attributes["href"];
                // Scrape Next URL
                this.Request(next_page, Parse);
            }
        }
    }
}
