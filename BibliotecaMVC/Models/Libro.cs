using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaMVC.Models
{
    public class Libro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ISBN { get; set; }

        [Range(0, 9999, ErrorMessage = "El año de publicación debe estar entre 0 y 9999.")]
        public int? AnioPublicacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un autor.")]
        public int AutorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
        public int CategoriaId { get; set; }

        [NotMapped]
        public decimal Precio { get; set; }

        [NotMapped]
        public bool Disponible { get; set; }

        [NotMapped]
        public string? ImagenUrl { get; set; }
        public Autor? Autor { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
