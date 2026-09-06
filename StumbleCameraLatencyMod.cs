using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(StumbleCameraLatencyMod.Main), "PIKACHU MOD", "3.0.0", "PIKACHU")]
[assembly: MelonGame("Kitka Games", "Stumble Guys")]

namespace StumbleCameraLatencyMod
{
    public sealed class Main : MelonMod
    {
        private const int LowLatencyFps = 288;
        private bool menuVisible, lowLatencyEnabled = true, cameraEnabled = true;
        private int originalVSync, originalTargetFps;
        private Camera currentCamera;
        private float baseFov, extraFov = 15f, lastAppliedFov;
        private bool baseFovCaptured, hasLastApplied;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        public override void OnInitializeMelon()
        {
            originalVSync = QualitySettings.vSyncCount;
            originalTargetFps = Application.targetFrameRate;
            ApplyLatencySettings();
            LoggerInstance.Msg("PIKACHU MOD v3 loaded. Press F2 to show or hide the menu.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                menuVisible = !menuVisible;
                if (menuVisible)
                {
                    previousCursorLock = Cursor.lockState;
                    previousCursorVisible = Cursor.visible;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = previousCursorLock;
                    Cursor.visible = previousCursorVisible;
                }
            }
            if (lowLatencyEnabled) ApplyLatencySettings();
            FindMainCamera();
        }

        public override void OnLateUpdate()
        {
            FindMainCamera();
            if (currentCamera == null || !baseFovCaptured) return;
            if (!cameraEnabled)
            {
                if (hasLastApplied) { currentCamera.fieldOfView = baseFov; hasLastApplied = false; }
                return;
            }
            float gameFov = currentCamera.fieldOfView;
            if (!hasLastApplied || Mathf.Abs(gameFov - lastAppliedFov) > 0.01f) baseFov = gameFov;
            lastAppliedFov = Mathf.Clamp(baseFov + extraFov, 40f, 120f);
            currentCamera.fieldOfView = lastAppliedFov;
            hasLastApplied = true;
        }

        public override void OnGUI()
        {
            if (!menuVisible) return;
            const float width = 390f, height = 330f, x = 30f, y = 100f;
            Color oldBackground = GUI.backgroundColor, oldContent = GUI.contentColor;
            GUI.backgroundColor = new Color(0.12f, 0.05f, 0.20f, 0.97f);
            GUI.Box(new Rect(x, y, width, height), "");
            GUI.backgroundColor = new Color(0.55f, 0.15f, 0.95f, 1f);
            GUI.Box(new Rect(x + 10f, y + 10f, width - 20f, 52f), "PIKACHU MOD");
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(x + 25f, y + 72f, width - 50f, 25f), "F2  -  SHOW / HIDE MENU");
            GUI.backgroundColor = lowLatencyEnabled ? new Color(0.20f, 0.75f, 0.35f, 1f) : new Color(0.75f, 0.20f, 0.20f, 1f);
            if (GUI.Button(new Rect(x + 25f, y + 105f, width - 50f, 42f), lowLatencyEnabled ? "LOW INPUT DELAY: ON" : "LOW INPUT DELAY: OFF"))
            {
                lowLatencyEnabled = !lowLatencyEnabled;
                if (lowLatencyEnabled) ApplyLatencySettings(); else RestoreLatencySettings();
            }
            GUI.backgroundColor = cameraEnabled ? new Color(0.20f, 0.75f, 0.35f, 1f) : new Color(0.75f, 0.20f, 0.20f, 1f);
            if (GUI.Button(new Rect(x + 25f, y + 157f, width - 50f, 42f), cameraEnabled ? "CAMERA MOD: ON" : "CAMERA MOD: OFF"))
            {
                cameraEnabled = !cameraEnabled;
                if (!cameraEnabled && currentCamera != null) { currentCamera.fieldOfView = baseFov; hasLastApplied = false; }
            }
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(x + 25f, y + 213f, 220f, 25f), "CAMERA DISTANCE");
            GUI.Label(new Rect(x + width - 90f, y + 213f, 65f, 25f), extraFov.ToString("0"));
            extraFov = GUI.HorizontalSlider(new Rect(x + 25f, y + 246f, width - 50f, 25f), extraFov, 0f, 45f);
            GUI.backgroundColor = new Color(0.35f, 0.20f, 0.55f, 1f);
            if (GUI.Button(new Rect(x + 25f, y + 278f, width - 50f, 35f), "RESET CAMERA"))
            {
                extraFov = 0f;
                if (currentCamera != null) currentCamera.fieldOfView = baseFov;
                hasLastApplied = false;
            }
            GUI.backgroundColor = oldBackground;
            GUI.contentColor = oldContent;
        }

        private void ApplyLatencySettings() { QualitySettings.vSyncCount = 0; Application.targetFrameRate = LowLatencyFps; }
        private void RestoreLatencySettings() { QualitySettings.vSyncCount = originalVSync; Application.targetFrameRate = originalTargetFps; }
        private void FindMainCamera()
        {
            Camera foundCamera = Camera.main;
            if (foundCamera == null || foundCamera == currentCamera) return;
            currentCamera = foundCamera;
            baseFov = currentCamera.fieldOfView;
            lastAppliedFov = baseFov;
            baseFovCaptured = true;
            hasLastApplied = false;
            LoggerInstance.Msg($"Camera found: {currentCamera.name}, FOV: {baseFov:0.0}");
        }
    }
}
