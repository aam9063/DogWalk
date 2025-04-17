using System;

namespace DogWalk_Domain.Exceptions;

public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
