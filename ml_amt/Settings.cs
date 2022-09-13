﻿using ABI_RC.Core.InteractionSystem;
using cohtml;
using System;
using System.Collections.Generic;

namespace ml_amt
{
    static class Settings
    {
        enum ModSetting
        {
            IKOverride = 0,
            CrouchLimit
        };

        static bool ms_ikOverride = true;
        static float ms_crouchLimit = 0.65f;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> IKOverrideChange;
        static public event Action<float> CrouchLimitChange;

        public static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("AMT");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>();
            ms_entries.Add(ms_category.CreateEntry(ModSetting.IKOverride.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.CrouchLimit.ToString(), 65));

            Load();

            MelonLoader.MelonCoroutines.Start(WaitMainMenuUi());
        }

        static System.Collections.IEnumerator WaitMainMenuUi()
        {
            while(ViewManager.Instance == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView.Listener == null)
                yield return null;

            ViewManager.Instance.gameMenuView.Listener.ReadyForBindings += () =>
            {
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_AMT_Call_InpSlider", new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_AMT_Call_InpToggle", new Action<string, string>(OnToggleUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSettingAMT", l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void Load()
        {
            ms_ikOverride = (bool)ms_entries[(int)ModSetting.IKOverride].BoxedValue;
            ms_crouchLimit = ((int)ms_entries[(int)ModSetting.CrouchLimit].BoxedValue) * 0.01f;
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.CrouchLimit:
                    {
                        ms_crouchLimit = int.Parse(p_value) * 0.01f;
                        CrouchLimitChange?.Invoke(ms_crouchLimit);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.IKOverride:
                    {
                        ms_ikOverride = bool.Parse(p_value);
                        IKOverrideChange?.Invoke(ms_ikOverride);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }

        public static float CrouchLimit
        {
            get => ms_crouchLimit;
        }

        public static bool IKOverride
        {
            get => ms_ikOverride;
        }
    }
}
