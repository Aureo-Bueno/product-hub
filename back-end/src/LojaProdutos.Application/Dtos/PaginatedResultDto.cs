namespace LojaProdutos.Application.Dtos;

public class PaginatedResultDto<T>
{
    public List<T> Data { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
