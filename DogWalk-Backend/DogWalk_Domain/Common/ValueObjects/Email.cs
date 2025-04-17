using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Email
    {
        public string Valor { get; }
        
        private Email(string valor)
        {
            Valor = valor;
        }
        
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));
                
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new ArgumentException("Email inválido", nameof(email));
            }
            catch
            {
                throw new ArgumentException("Email inválido", nameof(email));
            }
            
            return new Email(email);
        }
        
        public override string ToString() => Valor;
    }
