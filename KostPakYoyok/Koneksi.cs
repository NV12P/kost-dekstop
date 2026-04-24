using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();

        var data = new
        {
            email = "test@gmail.com",
            password = "123456"
        };

        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://kost.arcv.web.id/api/login", content);

        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine(result);
    }
}