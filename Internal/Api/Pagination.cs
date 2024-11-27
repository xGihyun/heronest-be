namespace Heronest.Internal.Api;

public class PaginationResult
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
}

public class Pagination
{
    HttpContext context;

    public Pagination(HttpContext context)
    {
        this.context = context;
    }

    public PaginationResult Parse()
    {
        var result = new PaginationResult();

        if (this.context.Request.Query.TryGetValue("page", out var pageValue))
        {
            if (!int.TryParse(pageValue.ToString(), out int parsedPage))
            {
                throw new ArgumentException("Invalid page query.");
            }
            result.Page = parsedPage;
        }

        if (this.context.Request.Query.TryGetValue("limit", out var limitValue))
        {
            if (!int.TryParse(limitValue.ToString(), out int parsedLimit))
            {
                throw new ArgumentException("Invalid limit query.");
            }
            result.Limit = parsedLimit;
        }

        return result;
    }
}
