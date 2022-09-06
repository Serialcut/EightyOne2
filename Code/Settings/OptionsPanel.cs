﻿// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using EightyOne2.Patches;
    using ICities;

    /// <summary>
    /// The mod's options panel..
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        public OptionsPanel()
        {
            // Auto layout.
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            UIHelper helper = new UIHelper(this);

            // Language options.
            UIHelperBase languageGroup = helper.AddGroup(Translations.Translate("SET_LANGUAGE"));
            UIDropDown languageDropDown = (UIDropDown)languageGroup.AddDropdown(Translations.Translate("SET_LANGUAGE"), Translations.LanguageList, Translations.Index, (value) =>
            {
                Translations.Index = value;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            });
            languageDropDown.autoSize = false;
            languageDropDown.width = 270f;

            if (Loading.IsLoaded)
            {
                UIHelperBase unlockGroup = helper.AddGroup(Translations.Translate("UNLOCK"));
                unlockGroup.AddButton(Translations.Translate("UNLOCK_25"), () => Singleton<SimulationManager>.instance.AddAction(() => Unlock(5)));
                unlockGroup.AddButton(Translations.Translate("UNLOCK_ALL"), () => Singleton<SimulationManager>.instance.AddAction(() => Unlock(9)));
            }
        }

        /// <summary>
        /// Unlocks game tiles.
        /// </summary>
        /// <param name="unlockWidth">Grid with to unlock (centered); e.g. 5 to unock 25-tile area, 9 to unlock 81.</param>
        private void Unlock(int unlockWidth)
        {
            // Local references
            GameAreaManager gameAreaManager = Singleton<GameAreaManager>.instance;

            // Calculate margin.
            int tileMargin = (GameAreaManagerPatches.ExpandedAreaGridResolution - unlockWidth) / 2;
            int maxCoord = tileMargin + unlockWidth;

            // Keep going recursively until all tiles have been unlocked.
            bool changingTiles = true;
            while (changingTiles)
            {
                // Reset flag.
                changingTiles = false;

                // Iterate through grid and unlock any tiles that already aren't.
                for (int z = tileMargin; z < maxCoord; ++z)
                {
                    for (int x = tileMargin; x < maxCoord; ++x)
                    {
                        // Check if this tile is unlocked.
                        if (!GameAreaManagerPatches.IsUnlocked(gameAreaManager, x, z))
                        {
                            // Not unlocked - record that we're still changing tiles.
                            changingTiles = true;

                            // Attempt to unlock tile (will fail if not unlockable, i.e. no unlocked adjacent areas).
                            gameAreaManager.UnlockArea((z * GameAreaManagerPatches.ExpandedAreaGridResolution) + x);
                        }
                    }
                }
            }

            Logging.Message("unlocking done");
        }
    }
}