// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Translations.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 12:42 PM
//    Created Date:     11/01/2023 12:42 PM
// -----------------------------------------

namespace Scp3114Mods;

#if EXILED
public class Translations : Exiled.API.Interfaces.ITranslation
#else
public class Translations
#endif
{
    public string CannotStrangleInnocentPlayer { get; set; } = $"Cannot strangle innocent players. They must have a weapon or candy.";
    public string CannotStrangleInnocentPlayerEmptyHand { get; set; } = $"Innocent players must have an empty hand to be strangled.";
    public string CannotStranglePlayerEmptyHand { get; set; } = $"Players must have an empty hand to be strangled.";
    public string CannotStrangleTutorials { get; set; } = "Cannot strangle tutorials.";
}