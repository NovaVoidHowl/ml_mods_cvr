﻿using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapManager : MonoBehaviour
    {
        static LeapManager ms_instance = null;

        readonly Leap.Controller m_leapController = null;
        readonly GestureMatcher.LeapData m_leapData = null;

        LeapTracking m_leapTracking = null;
        LeapTracked m_leapTracked = null;
        LeapInput m_leapInput = null;

        public static LeapManager GetInstance() => ms_instance;

        internal LeapManager()
        {
            m_leapController = new Leap.Controller();
            m_leapData = new GestureMatcher.LeapData();
        }
        ~LeapManager()
        {
            m_leapController.StopConnection();
            m_leapController.Dispose();
        }

        void Start()
        {
            if(ms_instance == null)
                ms_instance = this;

            DontDestroyOnLoad(this);

            m_leapController.Device += this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure += this.OnLeapDeviceFailure;
            m_leapController.DeviceLost += this.OnLeapDeviceLost;
            m_leapController.Connect += this.OnLeapServiceConnect;
            m_leapController.Disconnect += this.OnLeapServiceDisconnect;

            Settings.EnabledChange += this.OnEnableChange;
            Settings.TrackingModeChange += this.OnTrackingModeChange;

            m_leapTracking = new GameObject("[LeapTrackingRoot]").AddComponent<LeapTracking>();
            m_leapTracking.transform.parent = this.transform;

            MelonLoader.MelonCoroutines.Start(WaitForInputManager());
            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());

            OnEnableChange(Settings.Enabled);
            OnTrackingModeChange(Settings.TrackingMode);
        }

        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_leapController.Device -= this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure -= this.OnLeapDeviceFailure;
            m_leapController.DeviceLost -= this.OnLeapDeviceLost;
            m_leapController.Connect -= this.OnLeapServiceConnect;
            m_leapController.Disconnect -= this.OnLeapServiceDisconnect;

            Settings.EnabledChange -= this.OnEnableChange;
            Settings.TrackingModeChange -= this.OnTrackingModeChange;
        }

        IEnumerator WaitForInputManager()
        {
            while(CVRInputManager.Instance == null)
                yield return null;

            m_leapInput = CVRInputManager.Instance.gameObject.AddComponent<LeapInput>();
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_leapTracked = PlayerSetup.Instance.gameObject.AddComponent<LeapTracked>();
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                m_leapData.Reset();

                if(m_leapController.IsConnected)
                {
                    Leap.Frame l_frame = m_leapController.Frame();
                    GestureMatcher.GetFrameData(l_frame, m_leapData);
                }
            }
        }

        public GestureMatcher.LeapData GetLatestData() => m_leapData;

        // Device events
        void OnLeapDeviceInitialized(object p_sender, Leap.DeviceEventArgs p_args)
        {
            if(Settings.Enabled)
            {
                m_leapController.SubscribeToDeviceEvents(p_args.Device);
                UpdateDeviceTrackingMode();
            }

            Utils.ShowHUDNotification("Leap Motion Extension", "Device initialized");
        }

        void OnLeapDeviceFailure(object p_sender, Leap.DeviceFailureEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Device failure", "Code " + p_args.ErrorCode + ": " + p_args.ErrorMessage);
        }

        void OnLeapDeviceLost(object p_sender, Leap.DeviceEventArgs p_args)
        {
            m_leapController.UnsubscribeFromDeviceEvents(p_args.Device);

            Utils.ShowHUDNotification("Leap Motion Extension", "Device lost");
        }

        void OnLeapServiceConnect(object p_sender, Leap.ConnectionEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Service connected");
        }

        void OnLeapServiceDisconnect(object p_sender, Leap.ConnectionLostEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Service disconnected");
        }

        // Settings
        void OnEnableChange(bool p_state)
        {
            if(p_state)
            {
                m_leapController.StartConnection();
                UpdateDeviceTrackingMode();
            }
            else
                m_leapController.StopConnection();
        }

        void OnTrackingModeChange(Settings.LeapTrackingMode p_mode)
        {
            if(Settings.Enabled)
                UpdateDeviceTrackingMode();
        }

        // Game events
        internal void OnAvatarClear()
        {
            if(m_leapTracking != null)
                m_leapTracking.OnAvatarClear();
            if(m_leapTracked != null)
                m_leapTracked.OnAvatarClear();
        }

        internal void OnAvatarSetup()
        {
            if(m_leapTracking != null)
                m_leapTracking.OnAvatarSetup();
            if(m_leapInput != null)
                m_leapInput.OnAvatarSetup();
            if(m_leapTracked != null)
                m_leapTracked.OnAvatarSetup();
        }

        internal void OnCalibrate()
        {
            if(m_leapTracked != null)
                m_leapTracked.OnCalibrate();
        }

        internal void OnRayScale(float p_scale)
        {
            if(m_leapInput != null)
                m_leapInput.OnRayScale(p_scale);
        }

        internal void OnPlayspaceScale(float p_relation)
        {
            if(m_leapTracking != null)
                m_leapTracking.OnPlayspaceScale(p_relation);
        }

        // Arbitrary
        void UpdateDeviceTrackingMode()
        {
            m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
            m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);

            switch(Settings.TrackingMode)
            {
                case Settings.LeapTrackingMode.Screentop:
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
                    break;
                case Settings.LeapTrackingMode.HMD:
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);
                    break;
            }
        }
    }
}
