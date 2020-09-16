﻿using System;

namespace EkwClicker.Models
{
    internal class PropertyNumber
    {
        public PropertyNumber(Guid id, string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentNullException(nameof(number));
            
            Id = id;
            Number = number;
        }

        public Guid Id { get; }
        public string Number { get; }
    }
}