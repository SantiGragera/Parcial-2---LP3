using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParcialLP3.Pages
{
    public class DocumentoInfo
    {
        public string Nombre { get; set; } = string.Empty;
        public long Tamano { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class DocumentosModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ExtensionesPermitidas = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
        private const long TamanoMaximo = 5 * 1024 * 1024;

        public DocumentosModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        [BindProperty]
        public IFormFile? Archivo { get; set; }

        public string? Usuario { get; set; }
        public string CarpetaSegura { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public bool Exito { get; set; }
        public List<DocumentoInfo> Documentos { get; set; } = new();

        [TempData]
        public string? MensajeTemp { get; set; }

        [TempData]
        public bool ExitoTemp { get; set; }

        public IActionResult OnGet()
        {
            Usuario = HttpContext.Session.GetString("Usuario");
            if (Usuario is null)
            {
                return RedirectToPage("/Registro");
            }

            Mensaje = MensajeTemp;
            Exito = ExitoTemp;
            CargarDocumentos();
            return Page();
        }

        public IActionResult OnPostSubir()
        {
            Usuario = HttpContext.Session.GetString("Usuario");
            if (Usuario is null)
            {
                return RedirectToPage("/Registro");
            }

            if (Archivo is null || Archivo.Length == 0)
            {
                Mensaje = "Debe seleccionar un archivo.";
                Exito = false;
                CargarDocumentos();
                return Page();
            }

            var nombre = Path.GetFileName(Archivo.FileName);
            var extension = Path.GetExtension(nombre).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
            {
                Mensaje = "El tipo de archivo no está permitido.";
                Exito = false;
                CargarDocumentos();
                return Page();
            }

            if (Archivo.Length > TamanoMaximo)
            {
                Mensaje = "El archivo supera el tamaño máximo de 5 MB.";
                Exito = false;
                CargarDocumentos();
                return Page();
            }

            var carpeta = ObtenerCarpetaUsuario(Usuario);
            Directory.CreateDirectory(carpeta);

            var nombreFinal = nombre;
            var rutaFinal = Path.Combine(carpeta, nombreFinal);
            if (System.IO.File.Exists(rutaFinal))
            {
                nombreFinal = Path.GetFileNameWithoutExtension(nombre) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                rutaFinal = Path.Combine(carpeta, nombreFinal);
            }

            using (var stream = new FileStream(rutaFinal, FileMode.Create))
            {
                Archivo.CopyTo(stream);
            }

            MensajeTemp = "Documento cargado correctamente.";
            ExitoTemp = true;
            return RedirectToPage();
        }

        public IActionResult OnPostBorrar(string nombreArchivo)
        {
            Usuario = HttpContext.Session.GetString("Usuario");
            if (Usuario is null)
            {
                return RedirectToPage("/Registro");
            }

            var nombre = Path.GetFileName(nombreArchivo ?? string.Empty);
            var ruta = Path.Combine(ObtenerCarpetaUsuario(Usuario), nombre);

            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
                MensajeTemp = "Documento eliminado.";
                ExitoTemp = true;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostSalir()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        private string ObtenerCarpetaUsuario(string usuario)
        {
            CarpetaSegura = string.Concat(usuario.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
            return Path.Combine(_env.WebRootPath, "documentos", CarpetaSegura);
        }

        private void CargarDocumentos()
        {
            var carpeta = ObtenerCarpetaUsuario(Usuario!);
            if (!Directory.Exists(carpeta))
            {
                return;
            }

            Documentos = new DirectoryInfo(carpeta)
                .GetFiles()
                .OrderByDescending(f => f.CreationTime)
                .Select(f => new DocumentoInfo
                {
                    Nombre = f.Name,
                    Tamano = f.Length,
                    Fecha = f.CreationTime
                })
                .ToList();
        }
    }
}
