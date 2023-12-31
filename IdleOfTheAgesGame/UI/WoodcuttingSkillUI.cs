﻿// <copyright file="WoodcuttingSkillUI.cs" company="DaanStout">
// Copyright (c) DaanStout. All rights reserved.
// </copyright>

using IdleOfTheAgesLib.Translation;
using IdleOfTheAgesLib.UI.Elements;

using UnityEngine.UIElements;

namespace IdleOfTheAgesGame.UI {
    /// <summary>
    /// Implements the UI for the woodcutting skill.
    /// </summary>
    public class WoodcuttingSkillUI : Element<Box, object> {
        private readonly ITranslationService translationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WoodcuttingSkillUI"/> class.
        /// </summary>
        /// <param name="translationService">The translation service to translate text.</param>
        public WoodcuttingSkillUI(ITranslationService translationService) {
            this.translationService = translationService;
        }

        /// <inheritdoc/>
        protected override void BuildElement(Box targetElement) {
            targetElement.Add(new Label(translationService.GetLanguageString(Constants.Skill.WOODCUTTING.TranslationKey)));
        }
    }
}
