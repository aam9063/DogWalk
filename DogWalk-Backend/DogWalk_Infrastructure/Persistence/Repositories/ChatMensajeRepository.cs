using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class ChatMensajeRepository : GenericRepository<ChatMensaje>, IChatMensajeRepository
{
    public ChatMensajeRepository(DogWalkDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<ChatMensaje>> GetConversacionAsync(Guid usuarioId, Guid paseadorId)
    {
        return await _context.ChatMensajes
            .Where(m => m.UsuarioId == usuarioId && m.PaseadorId == paseadorId)
            .OrderBy(m => m.FechaHora)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ChatMensaje>> GetMensajesUsuarioAsync(Guid usuarioId)
    {
        return await _context.ChatMensajes
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaHora)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ChatMensaje>> GetMensajesPaseadorAsync(Guid paseadorId)
    {
        return await _context.ChatMensajes
            .Where(m => m.PaseadorId == paseadorId)
            .OrderByDescending(m => m.FechaHora)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ChatMensaje>> GetMensajesNoLeidosUsuarioAsync(Guid usuarioId)
    {
        return await _context.ChatMensajes
            .Where(m => m.UsuarioId == usuarioId && !m.LeidoPorUsuario)
            .OrderBy(m => m.FechaHora)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ChatMensaje>> GetMensajesNoLeidosPaseadorAsync(Guid paseadorId)
    {
        return await _context.ChatMensajes
            .Where(m => m.PaseadorId == paseadorId && !m.LeidoPorPaseador)
            .OrderBy(m => m.FechaHora)
            .ToListAsync();
    }
    
    public async Task MarcarLeidosPorUsuarioAsync(Guid usuarioId, Guid paseadorId)
    {
        var mensajes = await _context.ChatMensajes
            .Where(m => m.UsuarioId == usuarioId && 
                        m.PaseadorId == paseadorId && 
                        !m.LeidoPorUsuario)
            .ToListAsync();
            
        foreach (var mensaje in mensajes)
        {
            mensaje.MarcarComoLeidoUsuario();
        }
    }
    
    public async Task MarcarLeidosPorPaseadorAsync(Guid usuarioId, Guid paseadorId)
    {
        var mensajes = await _context.ChatMensajes
            .Where(m => m.UsuarioId == usuarioId && 
                        m.PaseadorId == paseadorId && 
                        !m.LeidoPorPaseador)
            .ToListAsync();
            
        foreach (var mensaje in mensajes)
        {
            mensaje.MarcarComoLeidoPaseador();
        }
    }
}
