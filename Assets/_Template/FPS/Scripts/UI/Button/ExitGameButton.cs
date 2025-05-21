using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required if using UI Button
using TMPro; // Required if using TextMeshPro Button

namespace Unity.FPS.UI.Buttons
{

    /// <summary>
    /// Script that manages exiting the game and an optional confirmation dialog.
    /// </summary>
    public class ExitGameButton : MonoBehaviour
    {
        [Header("Confirmation Panel (Optional)")]
        [Tooltip("The confirmation panel to display before exiting the game. If not assigned, logs to console.")]
        public GameObject confirmationPanel; // UI panel where the user can confirm or cancel

        [Header("Confirmation Buttons (Optional)")]
        [Tooltip("The 'Yes' button on the confirmation panel.")]
        public Button confirmExitButton; // 'Yes' button on the confirmation panel
        [Tooltip("The 'No' button on the confirmation panel.")]
        public Button cancelExitButton; // 'No' button on the confirmation panel

        void Start()
        {
            // If there's a Button component, add TryShowConfirmation to its onClick event.
            Button thisButton = GetComponent<Button>();
            if (thisButton != null)
            {
                thisButton.onClick.AddListener(TryShowConfirmation);
            }
            // Alternatively, if using a custom button script like InteractiveButton:
            // InteractiveButton customButton = GetComponent<InteractiveButton>();
            // if (customButton != null)
            // {
            //     customButton.onClick.AddListener(TryShowConfirmation);
            // }

            // Hide the confirmation panel at the start
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            // Add listeners to the confirmation buttons (if assigned)
            if (confirmExitButton != null)
            {
                confirmExitButton.onClick.AddListener(ConfirmExit);
            }
            if (cancelExitButton != null)
            {
                cancelExitButton.onClick.AddListener(CancelExit);
            }
        }

        /// <summary>
        /// Tries to show a confirmation dialog before exiting.
        /// If a confirmation panel is assigned, it shows it. Otherwise, logs to the console.
        /// </summary>
        public void TryShowConfirmation()
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
                Debug.Log("Displaying confirmation panel.");
            }
            else
            {
                // Simple console confirmation (no UI)
                Debug.Log("Are you sure? (Yes/No) - Assign a confirmationPanel for a real UI.");
                // In this case, you might exit immediately or wait for other input.
                // For now, do not exit immediately to guide the developer to add UI.
                // ConfirmExit(); // Uncomment this line to exit immediately without UI
            }
        }

        /// <summary>
        /// Confirms exit and quits the application.
        /// </summary>
        public void ConfirmExit()
        {
            Debug.Log("Exiting the game...");
            Application.Quit();

            // Application.Quit() does not always work in the Unity Editor.
            // Use the line below to stop play mode in the Editor.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Cancels exit and hides the confirmation panel.
        /// </summary>
        public void CancelExit()
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
                Debug.Log("Game exit canceled.");
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Typically nothing is needed in Update for an exit button;
            // it's event-driven.
        }
    }
}
