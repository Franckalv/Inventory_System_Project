namespace ProyectoCatedraDES.Services;

public interface IInventarioService
{
    Task RegistrarEntradaAsync(int productoId, int cantidad, string? usuarioId, string? comentario = null);
    Task RegistrarSalidaAsync(int productoId, int cantidad, string? usuarioId, string? comentario = null);
}
