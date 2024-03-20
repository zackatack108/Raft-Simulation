using System.Net.Http.Json;

namespace Services.Services;

public class APIService
{
    private readonly HttpClient client;

    public APIService(HttpClient client)
    {
        this.client = client;
    }

    public async Task AddMoney(string name, decimal amount)
    {
        (string, decimal) usernameBalance = new(name, amount);
        await client.PostAsJsonAsync($"Gateway/Add", usernameBalance);
    }

    public async Task<decimal> ViewBalance(string name)
    {
        return await client.GetFromJsonAsync<decimal>($"Gateway/Get?key={name}");
    }

    public async Task AddProduct(string name, int numItems)
    {
        (string, int) usernameBalance = new(name, numItems);
        await client.PostAsJsonAsync($"Gateway/Add", usernameBalance);
    }

    public async Task UpdateProduct(string name, int numItems)
    {
        (string, int) usernameBalance = new(name, numItems);
        await client.PostAsJsonAsync($"Gateway/Add", usernameBalance);
    }
}
