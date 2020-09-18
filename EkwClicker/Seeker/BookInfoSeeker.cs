﻿using System;
using System.Collections.Generic;
using EkwClicker.Core;
using EkwClicker.Models;

namespace EkwClicker.Seeker
{
    internal class BookInfoSeeker
    {
        private readonly IClicker _clicker;

        public BookInfoSeeker(IClicker clicker)
        {
            _clicker = clicker;
        }
        
        public bool ReadBookInfo(BookInfo bookInfo)
        {
            if (!bookInfo.Number.ControlDigit.HasValue)
            {
                throw new ArgumentException(
                    "must be filled with value", nameof(bookInfo.Number.ControlDigit));
            }
            
            var bookNumber = bookInfo.Number;
            
            _clicker.FillTextbox("kodWydzialuInput", bookNumber.CourtCode);
            _clicker.FillTextbox("numerKsiegiWieczystej", bookNumber.Number);
            _clicker.FillTextbox("cyfraKontrolna", bookNumber.ControlDigit.ToString());
             
            _clicker.ClickButtonById("wyszukaj");

            if (_clicker.CheckIfAnyError()) throw new Exception("captcha error");

            if (!_clicker.CheckIfNotFound())
            {
                bookInfo.BookType = _clicker.GetValueFromTable("Typ księgi wieczystej");
                bookInfo.OpeningDate = _clicker.GetValueFromTable("Data zapisania księgi wieczystej");
                bookInfo.ClosureDate = _clicker.GetValueFromTable("Data zamknięcia księgi wieczystej");
                bookInfo.Location = _clicker.GetValueFromTable("Położenie");
                bookInfo.Owner = _clicker.GetValueFromTable("Właściciel");

                return true;
            }

            return false;
        }

        public IReadOnlyList<string> ReadProperties()
        {
            _clicker.ClickButtonById("przyciskWydrukZwykly");

            var numbers = _clicker.GetPropertyNumbers();

            _clicker.ClickButtonByName("Wykaz");

            return numbers;
        }
    }
}