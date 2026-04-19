using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

var articles = new ConcurrentDictionary<int, Article>();
int nextId = 2;

SeedArticles();

app.MapGet("/", () => Results.Ok("Articles API is running"));

app.MapGet("/api/articles", (string? title, string? author) =>
{
    IEnumerable<Article> result = articles.Values.OrderBy(article => article.Id);

    if (!string.IsNullOrWhiteSpace(title))
    {
        result = result.Where(article =>
            article.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(author))
    {
        result = result.Where(article =>
            article.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(result);
});

app.MapGet("/api/articles/{id:int}", (int id) =>
{
    return articles.TryGetValue(id, out Article? article)
        ? Results.Ok(article)
        : Results.NotFound();
});

app.MapPost("/api/articles", (Article article) =>
{
    int id = Interlocked.Increment(ref nextId);
    Article createdArticle = article with { Id = id };
    articles[id] = createdArticle;
    return Results.Ok(createdArticle);
});

app.MapDelete("/api/articles/{id:int}", (int id) =>
{
    return articles.TryRemove(id, out _)
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapMethods("/api/articles/{id:int}", new[] { "PATCH" }, (int id, ArticlePatch patch) =>
{
    if (!articles.TryGetValue(id, out Article? currentArticle))
    {
        return Results.NotFound();
    }

    Article updatedArticle = currentArticle with
    {
        Title = patch.Title ?? currentArticle.Title,
        Description = patch.Description ?? currentArticle.Description,
        Image = patch.Image ?? currentArticle.Image,
        Author = patch.Author ?? currentArticle.Author,
        Date = patch.Date ?? currentArticle.Date
    };

    articles[id] = updatedArticle;
    return Results.Ok(updatedArticle);
});

app.Run();

void SeedArticles()
{
    articles[1] = new Article(
        1,
        "Async C# Basics",
        "How async and await work in C# applications.",
        null,
        "Helen",
        new DateTime(2026, 4, 1));

    articles[2] = new Article(
        2,
        "Working with REST APIs",
        "Using HttpClient for GET, POST, PATCH and DELETE requests.",
        "https://example.com/api.png",
        "Codex",
        new DateTime(2026, 4, 10));
}

internal record Article(
    int? Id,
    string Title,
    string Description,
    string? Image,
    string Author,
    DateTime Date);

internal record ArticlePatch(
    string? Title,
    string? Description,
    string? Image,
    string? Author,
    DateTime? Date);
