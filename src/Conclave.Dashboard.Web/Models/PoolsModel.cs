namespace Conclave.Dashboard.Web.Models;

public record PoolsModel
{
  public int Id { get; set; }

  public string Ticker { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public int Blocks { get; set; }

  public int Interest { get; set; }

  public double Saturation { get; set; }

  public int Pledge { get; set; }

  public bool IsConclave { get; set; }
}