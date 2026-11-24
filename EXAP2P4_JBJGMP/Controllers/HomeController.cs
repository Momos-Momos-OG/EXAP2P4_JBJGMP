using Microsoft.AspNetCore.Mvc;
using GroqChatbotMvc.Services;

namespace GroqChatbotMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroqChatService _chatService;

        public HomeController(GroqChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_chatService.ObtenerEstadoActual());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enviar(string mensajeUsuario)
        {
            var modeloVista = await _chatService.EnviarMensajeAsync(mensajeUsuario);
            return View("Index", modeloVista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevoChat()
        {
            _chatService.LimpiarConversacion();
            return RedirectToAction(nameof(Index));
        }
    }
}
