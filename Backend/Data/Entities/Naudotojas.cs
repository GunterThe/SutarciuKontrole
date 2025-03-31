using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Naudotojas
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(30)]
    public string Vardas { get; set; }

    [Required]
    [StringLength(30)]
    public string Pavarde { get; set; }

    [Required]
    [StringLength(100)]
    public string Pareigos { get; set; }

    [Required]
    public DateTime Gimimo_data { get; set; }

    [Required]
    [StringLength(100)]
    public string El_pastas { get; set; }

    public bool Adminas { get; set; } = false;

    public ICollection<IrasasNaudotojas> Irasai { get; set; }
}