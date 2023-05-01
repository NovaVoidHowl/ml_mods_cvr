﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ml_prm
{
    static class Settings
    {
        public enum ModSetting
        {
            Hotkey = 0,
            VelocityMultiplier,
            MovementDrag,
            AngularDrag,
            Gravity,
            PointersReaction,
            IgnoreLocal,
            CombatReaction,
            AutoRecover,
            RecoverDelay,
            Slipperiness,
            Bounciness,
            ViewVelocity
        }

        public static bool Hotkey { get; private set; } = true;
        public static float VelocityMultiplier { get; private set; } = 2f;
        public static float MovementDrag { get; private set; } = 2f;
        public static float AngularDrag { get; private set; } = 2f;
        public static bool Gravity { get; private set; } = true;
        public static bool PointersReaction { get; private set; } = true;
        public static bool IgnoreLocal { get; private set; } = true;
        public static bool CombatReaction { get; private set; } = true;
        public static bool AutoRecover { get; private set; } = false;
        public static float RecoverDelay { get; private set; } = 3f;
        public static bool Slipperiness { get; private set; } = false;
        public static bool Bounciness { get; private set; } = false;
        public static bool ViewVelocity { get; private set; } = false;

        static public event Action<bool> HotkeyChange;
        static public event Action<float> VelocityMultiplierChange;
        static public event Action<float> MovementDragChange;
        static public event Action<float> AngularDragChange;
        static public event Action<bool> GravityChange;
        static public event Action<bool> PointersReactionChange;
        static public event Action<bool> IgnoreLocalChange;
        static public event Action<bool> CombatReactionChange;
        static public event Action<bool> AutoRecoverChange;
        static public event Action<float> RecoverDelayChange;
        static public event Action<bool> SlipperinessChange;
        static public event Action<bool> BouncinessChange;
        static public event Action<bool> ViewVelocityChange;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PRM", null, true);
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Hotkey.ToString(), Hotkey),
                ms_category.CreateEntry(ModSetting.VelocityMultiplier.ToString(), VelocityMultiplier),
                ms_category.CreateEntry(ModSetting.MovementDrag.ToString(), MovementDrag),
                ms_category.CreateEntry(ModSetting.AngularDrag.ToString(), AngularDrag),
                ms_category.CreateEntry(ModSetting.Gravity.ToString(), Gravity),
                ms_category.CreateEntry(ModSetting.PointersReaction.ToString(), PointersReaction),
                ms_category.CreateEntry(ModSetting.IgnoreLocal.ToString(), IgnoreLocal),
                ms_category.CreateEntry(ModSetting.CombatReaction.ToString(), CombatReaction),
                ms_category.CreateEntry(ModSetting.AutoRecover.ToString(), AutoRecover),
                ms_category.CreateEntry(ModSetting.RecoverDelay.ToString(), RecoverDelay),
                ms_category.CreateEntry(ModSetting.Slipperiness.ToString(), Slipperiness),
                ms_category.CreateEntry(ModSetting.Bounciness.ToString(), Bounciness),
                ms_category.CreateEntry(ModSetting.ViewVelocity.ToString(), ViewVelocity)
            };

            Hotkey = (bool)ms_entries[(int)ModSetting.Hotkey].BoxedValue;
            VelocityMultiplier = Mathf.Clamp((float)ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue, 1f, 50f);
            MovementDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0f, 50f);
            AngularDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.AngularDrag].BoxedValue, 0f, 50f);
            Gravity = (bool)ms_entries[(int)ModSetting.Gravity].BoxedValue;
            PointersReaction = (bool)ms_entries[(int)ModSetting.PointersReaction].BoxedValue;
            IgnoreLocal = (bool)ms_entries[(int)ModSetting.IgnoreLocal].BoxedValue;
            CombatReaction = (bool)ms_entries[(int)ModSetting.CombatReaction].BoxedValue;
            AutoRecover = (bool)ms_entries[(int)ModSetting.AutoRecover].BoxedValue;
            RecoverDelay = Mathf.Clamp((float)ms_entries[(int)ModSetting.RecoverDelay].BoxedValue, 1f, 10f);
            Slipperiness = (bool)ms_entries[(int)ModSetting.Slipperiness].BoxedValue;
            Bounciness = (bool)ms_entries[(int)ModSetting.Bounciness].BoxedValue;
            ViewVelocity = (bool)ms_entries[(int)ModSetting.ViewVelocity].BoxedValue;
        }

        public static void SetSetting(ModSetting p_settings, object p_value)
        {
            switch(p_settings)
            {
                // Booleans
                case ModSetting.Hotkey:
                {
                    Hotkey = (bool)p_value;
                    HotkeyChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.Gravity:
                {
                    Gravity = (bool)p_value;
                    GravityChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.PointersReaction:
                {
                    PointersReaction = (bool)p_value;
                    PointersReactionChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.IgnoreLocal:
                {
                    IgnoreLocal = (bool)p_value;
                    IgnoreLocalChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.CombatReaction:
                {
                    CombatReaction = (bool)p_value;
                    CombatReactionChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.AutoRecover:
                {
                    AutoRecover = (bool)p_value;
                    AutoRecoverChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.Slipperiness:
                {
                    Slipperiness = (bool)p_value;
                    SlipperinessChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.Bounciness:
                {
                    Bounciness = (bool)p_value;
                    BouncinessChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.ViewVelocity:
                {
                    ViewVelocity = (bool)p_value;
                    ViewVelocityChange?.Invoke((bool)p_value);
                }
                break;

                // Floats
                case ModSetting.VelocityMultiplier:
                {
                    VelocityMultiplier = (float)p_value;
                    VelocityMultiplierChange?.Invoke((float)p_value);
                }
                break;

                case ModSetting.MovementDrag:
                {
                    MovementDrag = (float)p_value;
                    MovementDragChange?.Invoke((float)p_value);
                }
                break;

                case ModSetting.AngularDrag:
                {
                    AngularDrag = (float)p_value;
                    AngularDragChange?.Invoke((float)p_value);
                }
                break;

                case ModSetting.RecoverDelay:
                {
                    RecoverDelay = (float)p_value;
                    RecoverDelayChange?.Invoke((float)p_value);
                }
                break;
            }

            if(ms_entries != null)
                ms_entries[(int)p_settings].BoxedValue = p_value;
        }
    }
}
