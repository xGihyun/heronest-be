namespace Heronest.Features.Api;

public class PaginationQuery
{
    public int? Page { get; private set; }
    public int? Limit { get; private set; }

    public PaginationQuery(HttpContext context)
    {
        this.ParseQueryParameters(context.Request.Query);
    }

    private void ParseQueryParameters(IQueryCollection query)
    {
        if (query.TryGetValue("page", out var pageValue))
        {
            if (!int.TryParse(pageValue.ToString(), out int parsedPage) || parsedPage < 1)
            {
                throw new ArgumentException(
                    "The 'page' query parameter must be a positive integer."
                );
            }
            Page = parsedPage;
        }

        if (query.TryGetValue("limit", out var limitValue))
        {
            if (!int.TryParse(limitValue.ToString(), out int parsedLimit) || parsedLimit < 1)
            {
                throw new ArgumentException(
                    "The 'limit' query parameter must be a positive integer."
                );
            }
            Limit = parsedLimit;
        }
    }

    public (int? Offset, int? Limit) GetValues()
    {
        if(this.Limit is null || this.Page is null) {
            return (null, null);
        }

        if (Limit <= 0)
        {
            throw new ArgumentException("The 'limit' value must be greater than zero.");
        }

        if (Page <= 0)
        {
            throw new ArgumentException("The 'page' value must be greater than zero.");
        }

        return ((Page - 1) * Limit, Limit);
    }
}
