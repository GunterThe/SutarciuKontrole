using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Irasas
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Id_dokumento { get; set; }

    [Required]
    [StringLength(100)]
    public string Pavadinimas { get; set; }

    [Required]
    public DateTime Isigaliojimo_data { get; set; }

    [Required]
    public DateTime Pabaigos_data { get; set; }

    [Required]
    public int Dienos_pries { get; set; } = 0;

    [Required]
    public int Dienu_daznumas { get; set; } = 0;

    public bool Archyvuotas { get; set; } = false;
    public DateTime Kita_data { get; set; }
    [Required]
    [StringLength(100)]
    public string Pastas_kreiptis { get; set; }

    public ICollection<IrasasNaudotojas> Naudotojai { get; set; }
}