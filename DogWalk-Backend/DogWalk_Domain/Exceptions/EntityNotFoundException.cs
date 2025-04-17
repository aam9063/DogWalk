using System;

namespace DogWalk_Domain.Exceptions;

public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, Guid id) 
            : base($"La entidad {entityName} con ID {id} no fue encontrada") { }
    }
