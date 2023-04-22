﻿using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapInput : CVRInputModule
    {
        CVRInputManager m_inputManager = null;
        InputModuleSteamVR m_steamVrModule = null;
        bool m_inVR = false;
        bool m_gripToGrab = true;

        ControllerRay m_handRayLeft = null;
        ControllerRay m_handRayRight = null;
        LineRenderer m_lineLeft = null;
        LineRenderer m_lineRight = null;
        bool m_interactLeft = false;
        bool m_interactRight = false;
        bool m_gripLeft = false;
        bool m_gripRight = false;

        public new void Start()
        {
            base.Start();

            m_inputManager = CVRInputManager.Instance; // _inputManager is stripped out, cool beans
            m_steamVrModule = m_inputManager.GetComponent<InputModuleSteamVR>();
            m_inVR = Utils.IsInVR();

            m_handRayLeft = LeapTracking.GetInstance().GetLeftHand().gameObject.AddComponent<ControllerRay>();
            m_handRayLeft.hand = true;
            m_handRayLeft.generalMask = -1485;
            m_handRayLeft.isInteractionRay = true;
            m_handRayLeft.triggerGazeEvents = false;
            m_handRayLeft.holderRoot = m_handRayLeft.gameObject;

            m_handRayRight = LeapTracking.GetInstance().GetRightHand().gameObject.AddComponent<ControllerRay>();
            m_handRayRight.hand = false;
            m_handRayRight.generalMask = -1485;
            m_handRayRight.isInteractionRay = true;
            m_handRayRight.triggerGazeEvents = false;
            m_handRayRight.holderRoot = m_handRayRight.gameObject;

            m_lineLeft = m_handRayLeft.gameObject.AddComponent<LineRenderer>();
            m_lineLeft.endWidth = 1f;
            m_lineLeft.startWidth = 1f;
            m_lineLeft.textureMode = LineTextureMode.Tile;
            m_lineLeft.useWorldSpace = false;
            m_lineLeft.widthMultiplier = 1f;
            m_lineLeft.allowOcclusionWhenDynamic = false;
            m_lineLeft.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_lineLeft.enabled = false;
            m_lineLeft.receiveShadows = false;
            m_handRayLeft.lineRenderer = m_lineLeft;

            m_lineRight = m_handRayRight.gameObject.AddComponent<LineRenderer>();
            m_lineRight.endWidth = 1f;
            m_lineRight.startWidth = 1f;
            m_lineRight.textureMode = LineTextureMode.Tile;
            m_lineRight.useWorldSpace = false;
            m_lineRight.widthMultiplier = 1f;
            m_lineRight.allowOcclusionWhenDynamic = false;
            m_lineRight.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_lineRight.enabled = false;
            m_lineRight.receiveShadows = false;
            m_handRayRight.lineRenderer = m_lineRight;

            Settings.EnabledChange += this.OnEnableChange;
            Settings.InputChange += this.OnInputChange;

            OnEnableChange(Settings.Enabled);
            OnInputChange(Settings.Input);

            MelonLoader.MelonCoroutines.Start(WaitForSettings());
            MelonLoader.MelonCoroutines.Start(WaitForMaterial());
        }

        IEnumerator WaitForSettings()
        {
            while(MetaPort.Instance == null)
                yield return null;
            while(MetaPort.Instance.settings == null)
                yield return null;

            m_gripToGrab = MetaPort.Instance.settings.GetSettingsBool("ControlUseGripToGrab", true);
            MetaPort.Instance.settings.settingBoolChanged.AddListener(this.OnGameSettingBoolChange);
        }

        IEnumerator WaitForMaterial()
        {
            while(PlayerSetup.Instance == null)
                yield return null;
            while(PlayerSetup.Instance.leftRay == null)
                yield return null;
            while(PlayerSetup.Instance.leftRay.lineRenderer == null)
                yield return null;

            m_lineLeft.material = PlayerSetup.Instance.leftRay.lineRenderer.material;
            m_lineLeft.gameObject.layer = PlayerSetup.Instance.leftRay.gameObject.layer;
            m_lineRight.material = PlayerSetup.Instance.leftRay.lineRenderer.material;
            m_lineRight.gameObject.layer = PlayerSetup.Instance.leftRay.gameObject.layer;
        }

        void OnDestroy()
        {
            Settings.EnabledChange -= this.OnEnableChange;
            Settings.InputChange -= this.OnInputChange;

            MetaPort.Instance.settings.settingBoolChanged.RemoveListener(this.OnGameSettingBoolChange);
        }

        void Update()
        {
            GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

            if(Settings.Enabled)
            {
                if(l_data.m_leftHand.m_present)
                    SetFingersInput(l_data.m_leftHand, true);

                if(l_data.m_rightHand.m_present)
                    SetFingersInput(l_data.m_rightHand, false);

                if(m_inVR)
                {
                    m_inputManager.individualFingerTracking = !m_steamVrModule.GetIndexGestureToggle();
                    m_inputManager.individualFingerTracking |= (l_data.m_leftHand.m_present || l_data.m_rightHand.m_present);
                }
                else
                    m_inputManager.individualFingerTracking = (l_data.m_leftHand.m_present || l_data.m_rightHand.m_present);
                IKSystem.Instance.FingerSystem.controlActive = m_inputManager.individualFingerTracking;
            }

            m_handRayLeft.enabled = (l_data.m_leftHand.m_present && (!m_inVR || !Utils.IsLeftHandTracked() || !Settings.FingersOnly));
            m_handRayRight.enabled = (l_data.m_rightHand.m_present && (!m_inVR || !Utils.IsRightHandTracked() || !Settings.FingersOnly));
        }

        public override void UpdateInput()
        {
            if(Settings.Enabled && Settings.Input)
            {
                GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

                if(l_data.m_leftHand.m_present && (!m_inVR || !Utils.IsLeftHandTracked() || !Settings.FingersOnly))
                {
                    float l_strength = l_data.m_leftHand.m_grabStrength;

                    float l_interactValue = 0f;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    m_inputManager.interactLeftValue = Mathf.Max(l_interactValue, m_inputManager.interactLeftValue);

                    if(m_interactLeft != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactLeft = (l_strength > Settings.InteractThreadhold);
                        m_inputManager.interactLeftDown |= m_interactLeft;
                        m_inputManager.interactLeftUp |= !m_interactLeft;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    m_inputManager.gripLeftValue = Mathf.Max(l_gripValue, m_inputManager.gripLeftValue);
                    if(m_gripLeft != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripLeft = (l_strength > Settings.GripThreadhold);
                        m_inputManager.gripLeftDown |= m_gripLeft;
                        m_inputManager.gripLeftUp |= !m_gripLeft;
                    }
                }

                if(l_data.m_rightHand.m_present && (!m_inVR || !Utils.IsRightHandTracked() || !Settings.FingersOnly))
                {
                    float l_strength = l_data.m_rightHand.m_grabStrength;

                    float l_interactValue = 0f;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    m_inputManager.interactRightValue = Mathf.Max(l_interactValue, m_inputManager.interactRightValue);

                    if(m_interactRight != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactRight = (l_strength > Settings.InteractThreadhold);
                        m_inputManager.interactRightDown |= m_interactRight;
                        m_inputManager.interactRightUp |= !m_interactRight;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    m_inputManager.gripRightValue = Mathf.Max(l_gripValue, m_inputManager.gripRightValue);
                    if(m_gripRight != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripRight = (l_strength > Settings.GripThreadhold);
                        m_inputManager.gripRightDown |= m_gripRight;
                        m_inputManager.gripRightUp |= !m_gripRight;
                    }
                }
            }
        }

        // Settings changes
        void OnEnableChange(bool p_state)
        {
            OnInputChange(p_state && Settings.Input);
            UpdateFingerTracking();
        }

        void OnInputChange(bool p_state)
        {
            ((MonoBehaviour)m_handRayLeft).enabled = (p_state && Settings.Enabled);
            ((MonoBehaviour)m_handRayRight).enabled = (p_state && Settings.Enabled);
            m_lineLeft.enabled = (p_state && Settings.Enabled);
            m_lineRight.enabled = (p_state && Settings.Enabled);

            if(!p_state)
            {
                m_handRayLeft.DropObject(true);
                m_handRayLeft.ClearGrabbedObject();

                m_handRayRight.DropObject(true);
                m_handRayRight.ClearGrabbedObject();

                m_interactLeft = false;
                m_interactRight = false;
                m_gripLeft = false;
                m_gripRight = false;
            }
        }

        // Game events
        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            UpdateFingerTracking();
        }

        internal void OnRayScale(float p_scale)
        {
            m_handRayLeft.SetRayScale(p_scale);
            m_handRayRight.SetRayScale(p_scale);
        }

        // Arbitrary
        void UpdateFingerTracking()
        {
            m_inputManager.individualFingerTracking = (Settings.Enabled || (m_inVR && Utils.AreKnucklesInUse() && !m_steamVrModule.GetIndexGestureToggle()));
            IKSystem.Instance.FingerSystem.controlActive = m_inputManager.individualFingerTracking;
        }

        void SetFingersInput(GestureMatcher.HandData p_hand, bool p_left)
        {
            if(p_left)
            {
                m_inputManager.fingerCurlLeftThumb = p_hand.m_bends[0];
                m_inputManager.fingerCurlLeftIndex = p_hand.m_bends[1];
                m_inputManager.fingerCurlLeftMiddle = p_hand.m_bends[2];
                m_inputManager.fingerCurlLeftRing = p_hand.m_bends[3];
                m_inputManager.fingerCurlLeftPinky = p_hand.m_bends[4];
                IKSystem.Instance.FingerSystem.leftThumbCurl = p_hand.m_bends[0];
                IKSystem.Instance.FingerSystem.leftIndexCurl = p_hand.m_bends[1];
                IKSystem.Instance.FingerSystem.leftMiddleCurl = p_hand.m_bends[2];
                IKSystem.Instance.FingerSystem.leftRingCurl = p_hand.m_bends[3];
                IKSystem.Instance.FingerSystem.leftPinkyCurl = p_hand.m_bends[4];
            }
            else
            {
                m_inputManager.fingerCurlRightThumb = p_hand.m_bends[0];
                m_inputManager.fingerCurlRightIndex = p_hand.m_bends[1];
                m_inputManager.fingerCurlRightMiddle = p_hand.m_bends[2];
                m_inputManager.fingerCurlRightRing = p_hand.m_bends[3];
                m_inputManager.fingerCurlRightPinky = p_hand.m_bends[4];
                IKSystem.Instance.FingerSystem.rightThumbCurl = p_hand.m_bends[0];
                IKSystem.Instance.FingerSystem.rightIndexCurl = p_hand.m_bends[1];
                IKSystem.Instance.FingerSystem.rightMiddleCurl = p_hand.m_bends[2];
                IKSystem.Instance.FingerSystem.rightRingCurl = p_hand.m_bends[3];
                IKSystem.Instance.FingerSystem.rightPinkyCurl = p_hand.m_bends[4];
            }
        }

        // Game settings
        void OnGameSettingBoolChange(string p_name, bool p_state)
        {
            if(p_name == "ControlUseGripToGrab")
                m_gripToGrab = p_state;
        }
    }
}
