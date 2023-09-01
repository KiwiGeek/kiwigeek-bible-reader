using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KiwiGeekBible.WPF.Classes;

namespace KiwiGeekBible.WPF.BibleRenderer
{
    internal interface IBibleRenderer
    {
        Task NavigateToScripture(BibleReference reference);
        void HighlightScriptureRange(BibleReference start, BibleReference end);
    }
}
