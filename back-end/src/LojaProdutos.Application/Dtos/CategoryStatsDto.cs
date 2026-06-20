namespace LojaProdutos.Application.Dtos;

public class CategoryStatsDto
{
    public int Total { get; set; }
    public int CreatedThisMonth { get; set; }
    public int UpdatedToday { get; set; }
    public int Favorites { get; set; }
    public int Deleted { get; set; }
}
