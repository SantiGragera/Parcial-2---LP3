using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParcialLP3.Pages
{
    public class RegistroModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El formato del correo electrónico no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Nombre de usuario")]
        public string? NombreUsuario { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(16, 120, ErrorMessage = "Debe ingresar un valor numérico y ser mayor de 15 años.")]
        [Display(Name = "Edad")]
        public int? Edad { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Contrasena { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string? ConfirmarContrasena { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostRegistrar()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var opciones = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            };

            Response.Cookies.Append("Contrasena", Contrasena!, opciones);
            HttpContext.Session.SetString("Usuario", NombreUsuario!);

            return RedirectToPage("/Documentos");
        }
    }
}
