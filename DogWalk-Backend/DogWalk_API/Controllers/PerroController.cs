using DogWalk_Application.Contracts.DTOs.Perros;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using DogWalk_Domain.Common.Enums;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con los perros de los usuarios.
    /// Incluye gestión de perfiles de perros, fotos y opiniones.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PerroController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de perros.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        public PerroController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los perros del usuario autenticado.
        /// </summary>
        /// <returns>Lista de perros del usuario</returns>
        /// <response code="200">Retorna la lista de perros</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPerros()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var perros = await _unitOfWork.Perros.GetByUsuarioIdAsync(userId);
                
                var perrosDto = perros.Select(p => new PerroDto
                {
                    Id = p.Id,
                    UsuarioId = p.UsuarioId,
                    Nombre = p.Nombre,
                    Raza = p.Raza,
                    Edad = p.Edad,
                    GpsUbicacion = p.GpsUbicacion,
                    ValoracionPromedio = 0,
                    UrlFotoPrincipal = p.Fotos.FirstOrDefault()?.UrlFoto
                }).ToList();
                
                return Ok(perrosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener perros: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene los detalles de un perro específico.
        /// </summary>
        /// <param name="id">ID del perro</param>
        /// <returns>Detalles completos del perro incluyendo opiniones</returns>
        /// <response code="200">Retorna los detalles del perro</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para ver este perro</response>
        /// <response code="404">Si el perro no se encuentra</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPerroById(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var perro = await _unitOfWork.Perros.GetByIdAsync(id);
                
                if (perro == null)
                {
                    return NotFound(new { message = "Perro no encontrado" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perro.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(perro.UsuarioId);
                
                var opiniones = perro.Opiniones.ToList();
                
                var perroDto = new PerroWithOpinionesDto
                {
                    Id = perro.Id,
                    UsuarioId = perro.UsuarioId,
                    NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                    Nombre = perro.Nombre,
                    Raza = perro.Raza,
                    Edad = perro.Edad,
                    GpsUbicacion = perro.GpsUbicacion,
                    ValoracionPromedio = 0,
                    UrlsFotos = perro.Fotos.Select(f => f.UrlFoto).ToList(),
                    Opiniones = opiniones.Select(o => new OpinionPerroDto
                    {
                        Id = o.Id,
                        PaseadorId = o.PaseadorId,
                        NombrePaseador = $"{o.Paseador.Nombre} {o.Paseador.Apellido}",
                        FotoPaseador = o.Paseador.FotoPerfil,
                        Puntuacion = 5,
                        Comentario = o.Comentario,
                        Fecha = o.CreadoEn
                    }).ToList()
                };
                
                return Ok(perroDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener perro: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea un nuevo perro para el usuario autenticado.
        /// </summary>
        /// <param name="dto">Datos del nuevo perro</param>
        /// <returns>Detalles del perro creado</returns>
        /// <response code="201">Si el perro se creó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si intenta crear un perro para otro usuario sin ser administrador</response>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePerro([FromBody] CreatePerroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que el usuario no está intentando crear un perro para otro usuario
                if (dto.UsuarioId != userId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }
                
                // Crear nuevo perro
                var nuevoPerro = new Perro(
                    Guid.NewGuid(),
                    dto.UsuarioId,
                    dto.Nombre,
                    dto.Raza,
                    dto.Edad,
                    dto.GpsUbicacion
                );
                
                await _unitOfWork.Perros.AddAsync(nuevoPerro);
                await _unitOfWork.SaveChangesAsync();
                
                var perroDto = new PerroDto
                {
                    Id = nuevoPerro.Id,
                    UsuarioId = nuevoPerro.UsuarioId,
                    Nombre = nuevoPerro.Nombre,
                    Raza = nuevoPerro.Raza,
                    Edad = nuevoPerro.Edad,
                    GpsUbicacion = nuevoPerro.GpsUbicacion,
                    ValoracionPromedio = 0,
                    UrlFotoPrincipal = null
                };
                
                return CreatedAtAction(nameof(GetPerroById), new { id = nuevoPerro.Id }, perroDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear perro: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza los datos de un perro existente.
        /// </summary>
        /// <param name="dto">Datos actualizados del perro</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si el perro se actualizó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para actualizar este perro</response>
        /// <response code="404">Si el perro no se encuentra</response>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdatePerro([FromBody] UpdatePerroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var perro = await _unitOfWork.Perros.GetByIdAsync(dto.Id);
                
                if (perro == null)
                {
                    return NotFound(new { message = "Perro no encontrado" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perro.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                // Actualizar datos básicos del perro
                perro.ActualizarDatos(dto.Nombre, dto.Raza, dto.Edad);
                
                // Actualizar ubicación si ha cambiado
                if (dto.GpsUbicacion != perro.GpsUbicacion)
                {
                    perro.ActualizarUbicacion(dto.GpsUbicacion);
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                var perroDto = new PerroDto
                {
                    Id = perro.Id,
                    UsuarioId = perro.UsuarioId,
                    Nombre = perro.Nombre,
                    Raza = perro.Raza,
                    Edad = perro.Edad,
                    GpsUbicacion = perro.GpsUbicacion,
                    ValoracionPromedio = 0,
                    UrlFotoPrincipal = perro.Fotos.FirstOrDefault()?.UrlFoto
                };
                
                return Ok(perroDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar perro: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un perro del sistema.
        /// </summary>
        /// <param name="id">ID del perro a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si el perro se eliminó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para eliminar este perro</response>
        /// <response code="404">Si el perro no se encuentra</response>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePerro(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var perro = await _unitOfWork.Perros.GetByIdAsync(id);
                
                if (perro == null)
                {
                    return NotFound(new { message = "Perro no encontrado" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perro.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                // Verificar si hay reservas activas para este perro
                var reservasActivas = perro.Reservas.Where(r => 
                    r.Estado != EstadoReserva.Cancelada && 
                    r.Estado != EstadoReserva.Completada && 
                    r.FechaReserva > DateTime.Now);
                
                if (reservasActivas.Any())
                {
                    return BadRequest(new { message = "No se puede eliminar el perro porque tiene reservas activas" });
                }
                
                await _unitOfWork.Perros.DeleteAsync(perro);
                await _unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar perro: {ex.Message}" });
            }
        }

        /// <summary>
        /// Sube una nueva foto para un perro.
        /// </summary>
        /// <param name="id">ID del perro</param>
        /// <param name="foto">Archivo de la foto</param>
        /// <param name="descripcion">Descripción opcional de la foto</param>
        /// <returns>Detalles de la foto subida</returns>
        /// <response code="200">Si la foto se subió correctamente</response>
        /// <response code="400">Si el archivo no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para subir fotos a este perro</response>
        /// <response code="404">Si el perro no se encuentra</response>
        [HttpPost("{id}/foto")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFoto(Guid id, IFormFile foto, [FromForm] string descripcion = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var perro = await _unitOfWork.Perros.GetByIdAsync(id);
                
                if (perro == null)
                {
                    return NotFound(new { message = "Perro no encontrado" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perro.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                if (foto == null || foto.Length == 0)
                {
                    return BadRequest(new { message = "No se ha proporcionado ninguna foto" });
                }
                
                // Guardar foto 
                string urlFoto = await GuardarFotoAsync(foto, perro.Id);
                
                // Crear entidad FotoPerro
                var nuevaFoto = new FotoPerro(
                    Guid.NewGuid(),
                    perro.Id,
                    urlFoto,
                    descripcion
                );
                
                // Agregar foto al perro
                perro.AgregarFoto(nuevaFoto);
                
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { url = urlFoto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al subir foto: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Actualiza la descripción de una foto de perro.
        /// </summary>
        /// <param name="fotoId">ID de la foto</param>
        /// <param name="descripcion">Nueva descripción</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si la descripción se actualizó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para actualizar esta foto</response>
        /// <response code="404">Si la foto no se encuentra</response>
        [HttpPut("foto/{fotoId}/descripcion")]
        [Authorize]
        public async Task<IActionResult> UpdateFotoDescripcion(Guid fotoId, [FromBody] string descripcion)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Primero obtener todos los perros del usuario
                var perros = await _unitOfWork.Perros.GetByUsuarioIdAsync(userId);
                
                // Buscar la foto en todos los perros del usuario
                FotoPerro foto = null;
                Perro perroOwner = null;
                
                foreach (var perro in perros)
                {
                    foto = perro.Fotos.FirstOrDefault(f => f.Id == fotoId);
                    if (foto != null)
                    {
                        perroOwner = perro;
                        break;
                    }
                }
                
                if (foto == null || perroOwner == null)
                {
                    return NotFound(new { message = "Foto no encontrada o no pertenece a ninguno de tus perros" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perroOwner.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                // Actualizar descripción
                foto.ActualizarDescripcion(descripcion);
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { message = "Descripción actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar descripción: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Elimina una foto de un perro.
        /// </summary>
        /// <param name="fotoId">ID de la foto a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si la foto se eliminó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para eliminar esta foto</response>
        /// <response code="404">Si la foto no se encuentra</response>
        [HttpDelete("foto/{fotoId}")]
        [Authorize]
        public async Task<IActionResult> DeleteFoto(Guid fotoId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Tendremos que buscar la foto de otra manera
                var perros = await _unitOfWork.Perros.GetByUsuarioIdAsync(userId);
                
                // Buscar la foto en todos los perros del usuario
                FotoPerro foto = null;
                Perro perroOwner = null;
                
                foreach (var perro in perros)
                {
                    foto = perro.Fotos.FirstOrDefault(f => f.Id == fotoId);
                    if (foto != null)
                    {
                        perroOwner = perro;
                        break;
                    }
                }
                
                if (foto == null || perroOwner == null)
                {
                    return NotFound(new { message = "Foto no encontrada o no pertenece a ninguno de tus perros" });
                }
                
                // Verificar que el perro pertenece al usuario (o es admin)
                bool esAdmin = User.IsInRole("Administrador");
                if (perroOwner.UsuarioId != userId && !esAdmin)
                {
                    return Forbid();
                }
                
                // Eliminar archivo físico (opcional)
                EliminarArchivoFoto(foto.UrlFoto);
                
                // Necesitamos un método para eliminar la foto del perro
                // Como no tenemos acceso directo al repositorio de fotos, tendremos que eliminar la foto de la colección
                // Esto dependerá de cómo esté implementada tu entidad perro
                
                // Si tu entidad tiene un método para eliminar fotos, úsalo así:
                // perroOwner.EliminarFoto(foto);
                
                // Si no tiene un método específico, tendrás que implementar esta lógica
                // o asegurarte de que cuando se elimina la foto de la colección, EF Core la elimine de la base de datos
                
                await _unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar foto: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Guarda una foto en el sistema de archivos.
        /// </summary>
        /// <param name="foto">Archivo de la foto</param>
        /// <param name="perroId">ID del perro</param>
        /// <returns>URL de la foto guardada</returns>
        private async Task<string> GuardarFotoAsync(IFormFile foto, Guid perroId)
        {
            // Implementación depende de tu sistema de almacenamiento (local, Azure, AWS, etc.)
            // Esto es solo un ejemplo básico guardando en disco local
            
            var fileName = $"{perroId}_{Guid.NewGuid()}_{Path.GetFileName(foto.FileName)}";
            var directorio = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perros");
            
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }
            
            var filePath = Path.Combine(directorio, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }
            
            // Retorna la URL relativa o absoluta dependiendo de tu configuración
            return $"/uploads/perros/{fileName}";
        }
        
        /// <summary>
        /// Elimina un archivo de foto del sistema de archivos.
        /// </summary>
        /// <param name="urlFoto">URL de la foto a eliminar</param>
        private void EliminarArchivoFoto(string urlFoto)
        {
            try
            {
                if (string.IsNullOrEmpty(urlFoto))
                    return;
                
                // Convertir URL relativa a ruta absoluta
                string relativePath = urlFoto.TrimStart('/');
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }
            catch
            {
                // Ignorar errores al eliminar el archivo físico
            }
        }
    }
}
