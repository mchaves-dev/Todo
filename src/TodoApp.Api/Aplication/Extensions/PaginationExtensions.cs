namespace TodoApp.Api.Aplication.Extensions;

public static class PaginationExtensions
{
    public static IDictionary<string, string[]>? ValidatePagination(int page, int pageSize)
    {
        Dictionary<string, string[]> errors = [];

        if (page <= 0)
        {
            errors["page"] = ["deve ser maior que zero."];
        }

        if (pageSize <= 0)
        {
            errors["pageSize"] = ["deve ser maior que zero."];
        }

        return errors.Count == 0 ? null : errors;
    }
}
