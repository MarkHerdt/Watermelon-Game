using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Menus.Dropdown;

namespace Watermelon_Game.Menus.Languages
{
    /// <summary>
    /// Contains logic for language selection
    /// </summary>
    internal sealed class LanguageController : DropdownBase
    {
        #region Constants
        /// <summary>
        /// The default language
        /// </summary>
        private const string DEFAULT_LANGUAGE = "English";
        /// <summary>
        /// Key for the last set language in <see cref="PlayerPrefs"/>
        /// </summary>
        private const string SAVED_LANGUAGE = "Saved_Language";
        #endregion
        
        #region Fields
        /// <summary>
        /// Maps the language options in this <see cref="TMP_Dropdown"/> to a localization table id
        /// </summary>
        private readonly ReadOnlyDictionary<string, int> languageTableMap = new(new Dictionary<string, int>
        {
            { DEFAULT_LANGUAGE, 0 },
            { "中文", 1 },
            { "Русский", 8 },
            { "Español", 9 },
            { "Português", 7 },
            { "Deutsch", 3 },
            { "Français", 2 },
            { "日本語", 5 },
            { "한국어", 6 },
            { "Italiano", 4 }
        });
        #endregion

        #region Events
        /// <summary>
        /// Is called when the language was changed
        /// </summary>
        public static event Action OnLanguageChanged;
        #endregion
        
        #region Methods
        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            // Awake is called when exiting playmode
            // Doesn't break anything, just to remove the error message
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            this.LoadLanguage();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
#if UNITY_EDITOR
            // OnDestroy is called when starting playmode
            // Doesn't break anything, just to remove the error message
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            this.SaveLanguage();
        }

        /// <summary>
        /// Saves the currently active language
        /// </summary>
        private void SaveLanguage()
        {
            // IMPORTAND: Must be saved as a string because ints default value is 0, and 0 is a valid value in this context
            PlayerPrefs.SetString(SAVED_LANGUAGE, this.languageTableMap[base.CurrentActiveToggle].ToString());
        }

        /// <summary>
        /// Loads and sets the active language
        /// </summary>
        private void LoadLanguage()
        {
            // Gets the saved language from PlayerPrefs, if none exists, use the CurrentUICulture, if that's not supported use the default language
            var _tableIndex = PlayerPrefs.GetString(SAVED_LANGUAGE);
            var _availableLocales = LocalizationSettings.AvailableLocales.Locales;
            var _iso2 = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var _locale = string.IsNullOrWhiteSpace(_tableIndex) 
                ? _availableLocales.FirstOrDefault(_Locale => _Locale.Identifier.CultureInfo.TwoLetterISOLanguageName == _iso2) 
                : _availableLocales[int.Parse(_tableIndex)];
            
            // Sets the language
            LocalizationSettings.SelectedLocale = _locale == null ? _availableLocales[this.languageTableMap[DEFAULT_LANGUAGE]] : _locale;
            
            // Sets the selected dropdown language to the currently active locale
            base.value = this.languageTableMap.Values.FindIndex(_Value => _Value.ToString() == _tableIndex);
            base.CurrentActiveToggle = base.options[base.value].text;
        }
        
        /// <summary>
        /// Sets the currently active language <br/>
        /// <i>Is used on the "OnValueChanged"-event in the dropdown</i>
        /// </summary>
        public void SetLanguage()
        {
            var _language = base.options[base.value].text;
            var _tableId = this.languageTableMap[_language];
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_tableId];
        }
        
        public override void OnToggleChange(Toggle _Toggle)
        {
            base.OnToggleChange(_Toggle.name, _Toggle.isOn, OnLanguageChanged);
        }
        #endregion
    }
}