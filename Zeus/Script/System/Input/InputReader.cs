using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Zeus
{
    [Flags]
    public enum TypeInputActionMap
    {
        NONE = 0,

        PLAYERCONTROLS = 1 << 0,
        PEACEMOD = 1 << 1,
        BATTLEMOD = 1 << 2,
        UI = 1 << 3,
        QUICKTAB = 1 << 4,

        BATTLE = PLAYERCONTROLS | BATTLEMOD,
        UI_QUICKTAB = UI | QUICKTAB,

        ALL = ~NONE,
    }
    //[CreateAssetMenu(fileName = "InputReader", menuName = "Zeus/Game/Input Reader")]
    public class InputReader : UnitySingleton<InputReader>, GameInput.IPlayerControlsActions, GameInput.IPeaceModActions, GameInput.IBattleModActions, GameInput.IUIActions, GameInput.IQuickTabActions
    {
        private GameInput _gameInput;

        internal GameInput GameInput => _gameInput;
        internal bool Enable;

        ReadOnlyArray<InputActionMap> _prevActions;

        #region Events
        // PlayerControls
        public event UnityAction<Vector2> CallMove = delegate { };
        public event UnityAction<Vector2> CallRotateCamera = delegate { };
        public event UnityAction CallParry = delegate { };
        public event UnityAction CallJump = delegate { };
        public event UnityAction CallPause = delegate { };
        public event UnityAction CallFocusedListening = delegate { };
        public event UnityAction CallPotion = delegate { };
        public event UnityAction CallEvasionEnable = delegate { };
        public event UnityAction CallEvasionDIsable = delegate { };
        public event UnityAction CallRunEnable = delegate { };
        public event UnityAction CallRunDisable = delegate { };
        public event UnityAction CallQuickTab = delegate { };

        // PeaceMod
        public event UnityAction CallWeatherChange = delegate { };
        public event UnityAction CallStealth = delegate { };
        public event UnityAction CallFightMod = delegate { };

        // BattleMod
        public event UnityAction CallItemChangeLeft = delegate { };
        public event UnityAction CallItemChangeRight = delegate { };
        public event UnityAction CallPeaceMod = delegate { };
        public event UnityAction CallThrowItemPick = delegate { };
        public event UnityAction CallGuardEnable = delegate { };
        public event UnityAction CallGuardDiable = delegate { };
        public event UnityAction CallSkillCancel = delegate { };
        public event UnityAction CallLightAttack = delegate { };
        public event UnityAction CallHeavyAttack = delegate { };
        public event UnityAction CallHeavyAttackDisable = delegate { };
        public event UnityAction CallEquipBow = delegate { };
        public event UnityAction CallUnEquipBow = delegate { };
        public event UnityAction CallLockOn = delegate { };
        public event UnityAction CallNextTarget = delegate { };
        public event UnityAction CallPreviousTarget = delegate { };
        public event UnityAction CallWeaponSkill_1 = delegate { };
        public event UnityAction CallWeaponSkill_2 = delegate { };
        public event UnityAction CallRuneSkill = delegate { };
        public event UnityAction<bool> CallLeftShoulder = delegate { };

        // UI
        public event UnityAction<Vector2> CallNavigate = delegate { };
        public event UnityAction CallSubmit = delegate { };
        public event UnityAction CallCancel = delegate { };
        public event UnityAction CallClick = delegate { };
        public event UnityAction<bool> CallMousePress = delegate { };
        public event UnityAction CallExitUI = delegate { };

        // QuickTab
        public event UnityAction CallCategoryChangeLeft = delegate { };
        public event UnityAction CallCategoryChangeRight = delegate { };
        public event UnityAction CallSelectChangeLeft = delegate { };
        public event UnityAction CallSelectChangeRight = delegate { };
        public event UnityAction CallQuickTabSelect = delegate { };
        public event UnityAction CallQuickTabExit = delegate { };

        // Rebind
        public event Action<string> CallRebindStarted;
        public event Action CallRebindUpdated;
        public event Action<bool> CallRebindStopped;

        //public event Action CallRebindComplete;
        //public event Action CallRebindCanceled;
        //public event Action<string> CallRebindStatus;
        //public event Action<InputAction, int> CallRebindStarted;
        #endregion

        protected override void _OnAwake()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.PlayerControls.SetCallbacks(this);
                _gameInput.PeaceMod.SetCallbacks(this);
                _gameInput.BattleMod.SetCallbacks(this);
                _gameInput.UI.SetCallbacks(this);
                _gameInput.QuickTab.SetCallbacks(this);

                Enable = true;
            }
        }

        public bool IncludeActionMapType(TypeInputActionMap type1, TypeInputActionMap type2)
        {
            return (type1 & type2) != TypeInputActionMap.NONE;
        }
        public string GetActionMapName(TypeInputActionMap actionMapType)
        {
            if (actionMapType == TypeInputActionMap.PLAYERCONTROLS)
                return "PlayerControls";
            if (actionMapType == TypeInputActionMap.PEACEMOD)
                return "PeaceMod";
            if (actionMapType == TypeInputActionMap.BATTLEMOD)
                return "BattleMod";
            if (actionMapType == TypeInputActionMap.UI)
                return "UI";
            if (actionMapType == TypeInputActionMap.QUICKTAB)
                return "QuickTab";
            return string.Empty;
        }
        public List<string> GetActionMapNames(TypeInputActionMap actionMapType)
        {
            List<string> names = new List<string>();

            foreach (TypeInputActionMap type in Enum.GetValues(typeof(TypeInputActionMap)))
            {
                if (IncludeActionMapType(actionMapType, type))
                {
                    var actionMapName = GetActionMapName(type);
                    if (!string.IsNullOrEmpty(actionMapName))
                        names.Add(actionMapName);
                }
            }
            return names;
        }
        private void EnableActionMap(InputActionMap actionMap, bool enabled)
        {
            if (enabled) actionMap.Enable();
            else actionMap.Disable();
        }
        public void EnableActionMap(TypeInputActionMap actionMapType)
        {
            var actionMapNames = GetActionMapNames(actionMapType);

            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < _gameInput.asset.actionMaps.Count; i++)
            {
                var actionMap = _gameInput.asset.actionMaps[i];

                if (actionMap == null) continue;

                var enable = false;

                if (actionMapType == TypeInputActionMap.NONE)
                    enable = false;
                else if (actionMapType == TypeInputActionMap.ALL || actionMapNames.Contains(actionMap.name))
                    enable = true;

                EnableActionMap(actionMap, enable);

                if (enable)
                    strBuilder.Append($"{actionMap.name} / ");
            }
            if (actionMapNames.Count > 0)
            {
                strBuilder.Insert(0, "::: InputReader :::\n");
                strBuilder.Remove(strBuilder.Length - 3, 3);
                Debug.Log(strBuilder.ToString());
            }
        }

        #region IPlayerControlsActions
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            CallMove.Invoke(context.ReadValue<Vector2>());
        }
        public void OnRotateCamera(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            CallRotateCamera.Invoke(context.ReadValue<Vector2>());
        }
        public void OnParry(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallParry.Invoke();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallJump.Invoke();
        }
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallPause.Invoke();
        }
        public void OnFocusedListening(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallFocusedListening.Invoke();
        }
        public void OnPotion(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallPotion.Invoke();
        }
        public void OnEvasion(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallEvasionEnable.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                CallEvasionDIsable.Invoke();
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallRunEnable.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                CallRunDisable.Invoke();
        }


        public void OnQuickTab(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallQuickTab.Invoke();
        }
        #endregion

        #region IPeaceModActions
        public void OnWeatherChange(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallWeatherChange.Invoke();
        }
        public void OnStealth(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallStealth.Invoke();
        }
        public void OnFightMod(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallFightMod.Invoke();
        }
        #endregion

        #region IBattleModActions
        public void OnItemChangeLeft(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallItemChangeLeft.Invoke();
        }
        public void OnItemChangeRight(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallItemChangeRight.Invoke();
        }
        public void OnPeaceMod(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallPeaceMod.Invoke();
        }
        public void OnThrowItemPick(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallThrowItemPick.Invoke();
        }
        public void OnGuard(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallGuardEnable.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                CallGuardDiable.Invoke();
        }
        public void OnSkillCancel(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallSkillCancel.Invoke();
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallHeavyAttack.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                CallHeavyAttackDisable.Invoke();
        }

        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallLightAttack.Invoke();
        }

        public void OnEquipBow(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallEquipBow.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                CallUnEquipBow.Invoke();
        }

        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallLockOn.Invoke();
        }

        public void OnNextTarget(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallNextTarget.Invoke();
        }

        public void OnPreviousTarget(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallPreviousTarget.Invoke();
        }

        public void OnWeaponSkill_1(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallWeaponSkill_1.Invoke();
        }

        public void OnWeaponSkill_2(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallWeaponSkill_2.Invoke();
        }

        public void OnRuneSkill(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallRuneSkill.Invoke();
        }

        public void OnLeftShoulder(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallLeftShoulder.Invoke(true);
            if (context.phase == InputActionPhase.Canceled)
                CallLeftShoulder.Invoke(false);
        }
        #endregion

        #region IUIActions
        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallNavigate.Invoke(context.ReadValue<Vector2>());
        }
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                CallSubmit.Invoke();
        }
        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallCancel.Invoke();
        }
        public void OnPoint(InputAction.CallbackContext context)
        {
            if (!Enable) return;
        }
        public void OnClick(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallClick.Invoke();
        }
        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            if (!Enable) return;
        }

        public void OnMouseClickPosition(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Started)
                CallMousePress.Invoke(true);
            if (context.phase == InputActionPhase.Canceled)
                CallMousePress.Invoke(false);

        }
        public void OnExit(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallExitUI.Invoke();
        }
        #endregion

        #region QuickTab

        public void OnCaterotyChangeLeft(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallCategoryChangeLeft.Invoke();
        }

        public void OnCaterotyChangeRight(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallCategoryChangeRight.Invoke();
        }

        public void OnSelectChangeLeft(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallSelectChangeLeft.Invoke();
        }

        public void OnSelectChangeRight(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallSelectChangeRight.Invoke();
        }

        public void OnQuickTabSelect(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallQuickTabSelect.Invoke();
        }

        public void OnQuickTabExit(InputAction.CallbackContext context)
        {
            if (!Enable) return;
            if (context.phase == InputActionPhase.Performed)
                CallQuickTabExit.Invoke();
        }
        #endregion

        #region Rebind
        public InputAction GetAction(string actionName)
        {
            return _gameInput.asset.FindAction(actionName);
        }
        public void StartRebind(string actionName, int bindingIndex, TextMeshProUGUI statusText)
        {
            var action = GetAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Action 혹은 Binding을 찾을 수 없음.");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformRebind(action, firstPartIndex, true);
            }
            else
                PerformRebind(action, bindingIndex, false);
        }
        private void PerformRebind(InputAction action, int bindingIndex, bool allCompositeParts)
        {
            if (action == null || bindingIndex < 0) return;

            action.Disable();

            var rebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithExpectedControlType("Button")
                .WithControlsExcluding("<Mouse>/leftButton")
                .WithControlsExcluding("<Mouse>/rightButton")
                .WithControlsExcluding("<Mouse>/press")
                .WithCancelingThrough("<Any>/Cancel")
                .OnCancel(operation =>
                {
                    //m_RebindStopEvent?.Invoke(this, operation);
                    //m_RebindOverlay?.SetActive(false);
                    //UpdateBindingDisplay();

                    //action.Enable();
                    operation.Dispose();
                    CallRebindUpdated?.Invoke();
                    CallRebindStopped?.Invoke(false);
                })
                .OnComplete(operation =>
                {
                    //m_RebindOverlay?.SetActive(false);
                    //m_RebindStopEvent?.Invoke(this, operation);
                    //UpdateBindingDisplay();

                    if (CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                    {
                        action.RemoveBindingOverride(bindingIndex);
                        operation.Dispose();
                        PerformRebind(action, bindingIndex, allCompositeParts);
                        return;
                    }

                    //action.Enable();
                    operation.Dispose();
                    CallRebindUpdated?.Invoke();
                    CallRebindStopped?.Invoke(true);

                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformRebind(action, nextBindingIndex, allCompositeParts);
                    }

                    SaveBindingOverride(action);
                });

            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            var rebindText = !string.IsNullOrEmpty(action.expectedControlType)
                ? $"{partName}Waiting for {action.expectedControlType} input..."
                : $"{partName}Waiting for input...";

            CallRebindStarted?.Invoke(rebindText);

            rebind.Start();
        }
        private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts)
        {
            var newBinding = action.bindings[bindingIndex];
            foreach (InputBinding binding in action.actionMap.bindings)
            {
                if (binding.action == newBinding.action) continue;

                if (binding.effectivePath == newBinding.effectivePath)
                {
                    Debug.Log($"바인딩 중복 : {newBinding.effectivePath}");
                    return true;
                }
            }

            if (allCompositeParts)
            {
                for (int i = 0; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.effectivePath)
                    {
                        Debug.Log($"바인딩 중복 : {newBinding.effectivePath}");
                        return true;
                    }
                }
            }
            return false;
        }

        private void SaveBindingOverride(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
            }
        }
        public void LoadBindingOverride(string actionName)
        {
            var action = GetAction(actionName);
            if (action == null)
            {
                Debug.Log("Action 을 찾을 수 없음.");
                return;
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                    action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }
        public void ResetBinding(string actionName, int bindingIndex)
        {
            var action = GetAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Action 혹은 Binding을 찾을 수 없음.");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            CallRebindUpdated?.Invoke();
            SaveBindingOverride(action);
        }
        #endregion
    }
}