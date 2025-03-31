using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class IrasasNaudotojas
{
    [ForeignKey("Irasas")]
    public int IrasasId { get; set; }

    [ForeignKey("Naudotojas")]
    public string NaudotojasId { get; set; }

    public bool Prekes_Adminas { get; set; } = false;

    // Navigation properties
    public Irasas Irasas { get; set; }
    public Naudotojas Naudotojas { get; set; }
}