using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace SystemProgramming;

internal class Hw5AsyncAwaitDemo
{
    private readonly FakeStoreApiClient _apiClient = new();

    public void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Async/Await demo");
            Console.WriteLine("1 - View all products");
            Console.WriteLine("2 - View one product by ID");
            Console.WriteLine("0 - Exit");
            Console.Write("Choose option: ");

            string? choice = Console.ReadLine();

            if (choice == "0")
            {
                Console.WriteLine("Exit");
                break;
            }

            if (choice == "1")
            {
                await ShowAllProductsAsync();
                continue;
            }

            if (choice == "2")
            {
                await ShowOneProductAsync();
                continue;
            }

            Console.WriteLine("Unknown option");
        }
    }

    private async Task ShowAllProductsAsync()
    {
        try
        {
            var productsTask = _apiClient.GetAllProductsAsync();
            List<ProductDto> products = await WaitWithLoadingAsync(productsTask);

            Console.WriteLine($"Total products: {products.Count}");
            foreach (var product in products)
            {
                Console.WriteLine($"#{product.Id} | {product.Title} | ${product.Price:F2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
        }
    }

    private async Task ShowOneProductAsync()
    {
        Console.Write("Enter product ID: ");
        string? rawId = Console.ReadLine();

        if (!int.TryParse(rawId, out int productId) || productId <= 0)
        {
            Console.WriteLine("ID must be a positive integer");
            return;
        }

        try
        {
            var productTask = _apiClient.GetProductByIdAsync(productId);
            ProductDto? product = await WaitWithLoadingAsync(productTask);

            if (product is null)
            {
                Console.WriteLine("Product not found");
                return;
            }

            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Title: {product.Title}");
            Console.WriteLine($"Price: ${product.Price:F2}");
            Console.WriteLine($"Category: {product.Category}");
            Console.WriteLine($"Description: {product.Description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
        }
    }

    private async Task<T> WaitWithLoadingAsync<T>(Task<T> operation)
    {
        Console.Write("Loading");

        while (!operation.IsCompleted)
        {
            Console.Write("*");
            Thread.Sleep(150);
        }

        Console.WriteLine();
        return await operation;
    }
}

internal class FakeStoreApiClient
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string ProductsUrl = "https://fakestoreapi.com/products";

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(ProductsUrl);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<ProductDto>>(json, JsonOptions) ?? new List<ProductDto>();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync($"{ProductsUrl}/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductDto>(json, JsonOptions);
    }
}

internal class ProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;
}
