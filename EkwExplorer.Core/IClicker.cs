﻿using System;
using System.Collections.Generic;

namespace EkwExplorer.Core
{
    public interface IClicker : IDisposable
    {
        void GotoHome();
        void FillTextbox(string textboxId, string text);
        void ClickButtonById(string buttonId);
        void ClickButtonByName(string buttonName);
        void ClickButtonByValue(string buttonValue);
        string GetValueFromTable(string rowCaption);
        bool CheckIfAnyError();
        bool CheckIfNotFound();
        void CloseCookiesInfo();
        IReadOnlyList<string> GetPropertyNumbers();
        string GetPropertyOwners();
    }
}