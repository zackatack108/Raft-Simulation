using RaftShared;
using System;
using System.Net.Http.Json;

namespace Services;

public class APIService
{
    private readonly HttpClient client;

    public APIService(string gateway)
    {
        client = new HttpClient { BaseAddress = new Uri(gateway) };
    }

    public async Task<List<RaftItem>> GetItems(string keyword)
    {
        return await client.GetFromJsonAsync<List<RaftItem>>($"Gateway/GetItems?keyword={keyword}");
    }

    public async Task<RaftItem> GetItem(string keyword)
    {
        return await client.GetFromJsonAsync<RaftItem>($"Gateway/GetItem?keyword={keyword}");
    }

    public async Task<RaftItem> StrongGetItem(string keyword)
    {
        return await client.GetFromJsonAsync<RaftItem>($"Gateway/StrongGetItem?keyword={keyword}");
    }

    public async Task SaveItem(RaftItem item)
    {
        await client.PostAsJsonAsync("Gateway/SaveItem", item);
    }
}
