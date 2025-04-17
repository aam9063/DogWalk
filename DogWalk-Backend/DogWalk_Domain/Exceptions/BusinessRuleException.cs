using System;

namespace DogWalk_Domain.Exceptions;

 public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string message) : base(message) { }
    }