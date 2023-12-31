﻿// <copyright file="IPageItemManager.cs" company="DaanStout">
// Copyright (c) DaanStout. All rights reserved.
// </copyright>

using IdleOfTheAgesLib.Models.ModJsonData;
using IdleOfTheAgesLib.UI.Elements;

using System;

namespace IdleOfTheAgesLib.UI.Managers {
    /// <summary>
    /// Manages the <see cref="IPageItemElement"/>.
    /// </summary>
    public interface IPageItemManager : IUIManager<IPageItemElement> {
        /// <summary>
        /// An event that gets fired when the item gets clicked on.
        /// </summary>
        event Action<PageData> PageItemClickedEvent;

        /// <summary>
        /// Injects Page Data into the manager.
        /// </summary>
        /// <param name="page">The page data this manager should manage.</param>
        void InjectPageData(PageData page);
    }
}
