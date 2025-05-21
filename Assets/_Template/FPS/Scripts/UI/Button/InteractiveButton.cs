using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvents
using UnityEngine.EventSystems; // Required for event system interfaces
using TMPro; // Required for TextMeshPro components


namespace Unity.FPS.UI.Buttons
{
    /// <summary>
    /// Script for interactive button created with TextMeshPro.
    /// Handles hover, click, sound, and other interactions.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))] // Ensure a TextMeshProUGUI component is attached
    [RequireComponent(typeof(RectTransform))] // Buttons are UI elements, typically with a RectTransform
    public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        // --- Public Fields for Inspector Customization ---
        [Header("Button Text & Colors")]
        public string buttonText = "Button";
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
        public Color pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Darker gray
        public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent gray

        [Header("Button State")]
        public bool isInteractable = true;

        [Header("Sound Interaction (Optional)")]
        public bool playSounds = false;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        [Tooltip("If not assigned, an AudioSource will be added automatically.")]
        public AudioSource audioSource;


        [Header("Events")]
        public UnityEvent onClick = new UnityEvent(); // Event to trigger when the button is clicked

        // --- Private Fields ---
        private TextMeshProUGUI tmProText;
        private bool isPointerOver = false;
        private bool isPointerDown = false;

        // --- Unity Lifecycle Methods ---

        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            tmProText = GetComponent<TextMeshProUGUI>();
            if (tmProText == null)
            {
                Debug.LogError("InteractiveButton: No TextMeshProUGUI component found on this GameObject. Please add one.", this);
                enabled = false; // Disable the script if the component is missing
                return;
            }

            // Setup AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) // If still null, add one
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (audioSource != null)
            {
                audioSource.playOnAwake = false; // Ensure sounds don't play on start
            }
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// Used for initialization.
        /// </summary>
        void Start()
        {
            if (tmProText != null)
            {
                tmProText.text = buttonText; // Set initial button text
                UpdateColor(); // Set initial color based on state
            }
        }

        /// <summary>
        /// Update is called once per frame.
        /// Can be used for continuous updates if needed, though most logic here is event-driven.
        /// </summary>
        void Update()
        {
            // You could add logic here that needs to run every frame,
            // for example, if you wanted to animate something continuously.
            // For this button, event-driven updates are generally sufficient.
        }

        // --- EventSystem Interface Implementations ---

        /// <summary>
        /// Called by the EventSystem when the pointer enters the object.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isPointerOver = true;
            UpdateColor();
            PlaySound(hoverSound);
            // Debug.Log("Pointer Entered: " + gameObject.name);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer exits the object.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isPointerOver = false;
            isPointerDown = false; // Reset pressed state if pointer leaves while pressed
            UpdateColor();
            // Debug.Log("Pointer Exited: " + gameObject.name);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer is pressed down on the object.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable || eventData.button != PointerEventData.InputButton.Left) return;

            isPointerDown = true;
            UpdateColor();
            // Debug.Log("Pointer Down: " + gameObject.name);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer is released (if it was previously pressed on this object).
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable || eventData.button != PointerEventData.InputButton.Left) return;

            // bool wasPressed = isPointerDown; // Keep this if you need to differentiate between drag-off and click
            isPointerDown = false;
            UpdateColor();

            // If the pointer was released over the button (it was pressed and is still over it)
            // then it's considered a click. The OnPointerClick event handles the actual click action.
            // if (wasPressed && isPointerOver)
            // {
            //     Debug.Log("Pointer Up (considered click): " + gameObject.name);
            // }
        }

        /// <summary>
        /// Called by the EventSystem when a click event occurs.
        /// A click is typically a PointerDown followed by a PointerUp on the same object.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || eventData.button != PointerEventData.InputButton.Left) return;

            // Debug.Log("Pointer Clicked: " + gameObject.name);
            PlaySound(clickSound);
            onClick.Invoke(); // Trigger the UnityEvent
        }


        // --- Helper Methods ---

        /// <summary>
        /// Updates the color of the TextMeshPro text based on the button's current state.
        /// </summary>
        private void UpdateColor()
        {
            if (tmProText == null) return;

            if (!isInteractable)
            {
                tmProText.color = disabledColor;
            }
            else if (isPointerDown && isPointerOver) // Check isPointerOver as well for pressed state
            {
                tmProText.color = pressedColor;
            }
            else if (isPointerOver)
            {
                tmProText.color = hoverColor;
            }
            else
            {
                tmProText.color = normalColor;
            }
        }

        /// <summary>
        /// Plays the provided AudioClip if sounds are enabled and the clip is assigned.
        /// </summary>
        /// <param name="clipToPlay">The AudioClip to play.</param>
        private void PlaySound(AudioClip clipToPlay)
        {
            if (playSounds && clipToPlay != null && audioSource != null)
            {
                // audioSource.PlayOneShot(clipToPlay); // PlayOneShot is good for UI sounds as it doesn't interrupt other sounds playing on the same source
                // For more control, you might want to use audioSource.clip = clipToPlay; audioSource.Play();
                // However, PlayOneShot is generally recommended for brief, non-looping sounds like UI feedback.
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        /// <summary>
        /// Sets the interactable state of the button.
        /// </summary>
        /// <param name="interactable">True to make the button interactable, false otherwise.</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            isPointerDown = false; // Reset pressed state when interactability changes
                                   // isPointerOver will be naturally handled by EventSystem
            UpdateColor();
        }

        /// <summary>
        /// Updates the button's displayed text.
        /// </summary>
        /// <param name="newText">The new text to display.</param>
        public void SetText(string newText)
        {
            buttonText = newText;
            if (tmProText != null)
            {
                tmProText.text = buttonText;
            }
        }
    }
}