using Gamemanager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameFramework.Actors
{
    public class CameraCursor : MonoBehaviour
    {
        // ====================================================================
        // 1. 菜單/UI 切換輸入
        // 由於游標常駐可見，此輸入（例如 ESC, Tab）現在用於切換菜單或物品欄。
        // ====================================================================
        [Header("游標/菜單 切換 輸入")]
        [SerializeField] private InputActionReference cameraToggleInputAction;
        // startHidden 在 ARPG 模式下已無意義，但保留。
        [SerializeField] private bool startHidden = false; 

        // ====================================================================
        // 2. ARPG 相機自由調整 輸入 (新增：控制旋轉/俯仰)
        // 建議設定為滑鼠中鍵 (Middle Button)
        // ====================================================================
        [Header("ARPG 相機自由調整 輸入")]
        [SerializeField] private InputActionReference cameraAdjustAction;


        // ====================================================================
        // 3. Cinemachine 控制器配置
        // ====================================================================
        [Header("Cinemachine 控制器")]
        [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;
        // 以下三個欄位在 Top-Down ARPG 模式下（游標常駐顯示）影響較小，可以忽略或移除。
        [SerializeField] private bool disableCameraLookOnCursorVisible;
        [SerializeField] private bool disableCameraZoomOnCursorVisible;
        [Tooltip(
            "If you're using Cinemachine 2.8.4 or under, untick this option.\nIf unticked, both Look and Zoom will be disabled.")]
        [SerializeField]
        private bool fixedCinemachineVersion = true;

        [Header("Input References")] public InputActionReference lookAction;
        
        // ====================================================================
        // 4. 狀態追蹤變數 (新增)
        // ====================================================================
        private bool isCameraAdjusting = false; // 追蹤玩家是否按住 MMB 進行相機調整
        private bool isMenuOpen = false;        // 追蹤菜單/物品欄是否開啟 (由 cameraToggleInputAction 控制)
        
        private void Awake()
        {
            Debug.Log("Camera Cursor Awake");
            
            // 確保游標在 Top-Down ARPG 中始終可見且自由
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 1. 訂閱菜單切換輸入
            cameraToggleInputAction.action.started += OnCameraCursorToggled;

            // 2. 訂閱相機調整輸入 (按下/放開)
            if (cameraAdjustAction != null)
            {
                cameraAdjustAction.action.started += OnCameraAdjustStarted;
                cameraAdjustAction.action.canceled += OnCameraAdjustCanceled;
            }

            // 3. 訂閱 GameManager 的強制菜單事件
            GameManager.Instance.MainGameEvent.SetSubscribe(
                GameManager.Instance.MainGameEvent.OnCursorToggledEvent, 
                // 將事件傳入的 ShowCursor 視為 ShowMenu
                cmd => { ToggleCursor(cmd.ShowCursor ?? false); }
            );
        }

        private void OnEnable()
        {
            Debug.Log("輸入資產開啟");
            cameraToggleInputAction.asset.Enable();
            if (cameraAdjustAction != null)
            {
                cameraAdjustAction.asset.Enable();
            }
        }

        private void OnDisable()
        {
            Debug.Log("輸入資產關閉");
            cameraToggleInputAction.action.started -= OnCameraCursorToggled;
            if (cameraAdjustAction != null)
            {
                cameraAdjustAction.action.started -= OnCameraAdjustStarted;
                cameraAdjustAction.action.canceled -= OnCameraAdjustCanceled;
            }
            
            cameraToggleInputAction.asset.Disable();
            if (cameraAdjustAction != null)
            {
                cameraAdjustAction.asset.Disable();
            }
            GameManager.Instance.MainGameEvent.Unsubscribe<CursorToggledEvent>();
        }

        // ====================================================================
        // 核心邏輯 - MMB 調整相機
        // ====================================================================
        private void OnCameraAdjustStarted(InputAction.CallbackContext context)
        {
            // 只有在菜單未開啟 (isMenuOpen == false) 時，才允許啟用相機調整
            if (!isMenuOpen)
            {
                isCameraAdjusting = true;
                // 啟用 Cinemachine 控制器，讓滑鼠 X/Y 移動控制相機的旋轉/俯仰
                if (!fixedCinemachineVersion)
                {
                    cinemachineInputAxisController.enabled = true;
                }
            }
        }

        private void OnCameraAdjustCanceled(InputAction.CallbackContext context)
        {
            isCameraAdjusting = false;
            // 玩家放開 MMB，禁用 Cinemachine 輸入
            if (!fixedCinemachineVersion)
            {
                cinemachineInputAxisController.enabled = false;
            }
        }

        // ====================================================================
        // 核心邏輯 - 菜單切換 (原游標切換邏輯)
        // ====================================================================
        private void OnCameraCursorToggled(InputAction.CallbackContext context)
        {
            // 如果 isMenuOpen 為 true，則切換到 false (關閉菜單)；反之切換到 true (開啟菜單)
            ToggleCursor(!isMenuOpen);
        }

        /**
         * @summary 執行菜單切換邏輯，並同步控制相機輸入。
         * @param showMenu 是否要開啟菜單 (true) 或關閉菜單 (false)。
         */
        private void ToggleCursor(bool? showMenu = null)
        {
            // 決定新的菜單狀態
            if (showMenu == null)
            {
                isMenuOpen = !isMenuOpen;
            }
            else
            {
                isMenuOpen = showMenu.Value;
            }

            // 由於游標必須常駐顯示，我們不需要改變 Cursor.visible 和 Cursor.lockState
            
            if (isMenuOpen)
            {
                // 狀態：菜單開啟
                // 進入菜單模式時，無論玩家是否按住 MMB，都必須禁用相機控制
                isCameraAdjusting = false;
                if (!fixedCinemachineVersion)
                {
                    cinemachineInputAxisController.enabled = false;
                }
            }
            else
            {
                // 狀態：遊戲中
                // 退出菜單模式時，相機控制的啟用狀態取決於玩家是否按住 MMB
                if (!fixedCinemachineVersion)
                {
                    // 由於剛從菜單退出，isCameraAdjusting 必然為 false，所以這裡會禁用
                    // 玩家必須再次按下 MMB 才能調整相機
                    cinemachineInputAxisController.enabled = isCameraAdjusting; 
                }
            }
        }
    }
}