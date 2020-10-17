using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages game logic and controls the UI
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Slider hoopSlider;

    [Tooltip("Game ends after this many seconds have elapsed")]
    public float timerAmount = 60f;

    [Tooltip("The UI Controller")]
    public UIController uiController;

    [Tooltip("The player witch")]
    public Witch player;

    [Tooltip("The ML-Agent opponent witch")]
    public Witch opponent;

    [Tooltip("The hoop area")]
    public HoopSpawner hoopArea;

    [Tooltip("The main camera for the scene")]
    public Camera mainCamera;

    [Tooltip("Sound played at the start of the game")]
    public AudioSource startSound;
    //volume of the sound played
    private float volume;

    // When the game timer started
    private float gameTimerStartTime;

    /// <summary>
    /// All possible game states
    /// </summary>
    public enum GameState
    {
        Default,
        MainMenu,
        Preparing,
        Playing,
        Gameover
    }

    /// <summary>
    /// The current game state
    /// </summary>
    public GameState State { get; private set; } = GameState.Default;

    /// <summary>
    /// Gets the time remaining in the game
    /// </summary>
    public float TimeRemaining
    {
        get
        {
            if (State == GameState.Playing)
            {
                float timeRemaining = timerAmount - (Time.time - gameTimerStartTime);
                return Mathf.Max(0f, timeRemaining);
            }
            else
            {
                return 0f;
            }
        }
    }

    /// <summary>
    /// Handles a button click in different states
    /// </summary>
    public void ButtonClicked()
    {
        if (State == GameState.Gameover)
        {
            // In the Gameover state, button click should go to the main menu
            MainMenu();
        }
        else if (State == GameState.MainMenu)
        {
            // In the MainMenu state, button click should
            // Play the start sound
            startSound.Play();

            // Reset and generate the level
            hoopArea.ResetHoops();

            // Show the score panels
            uiController.ShowPlayerPanel(true);
            uiController.ShowOpponentPanel(true);

            // Start the game
            StartCoroutine(StartGame());
        }
        else
        {
            Debug.LogWarning("Button clicked in unexpected state: " + State.ToString());
        }
    }

    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        if(!Instance)
        {
            Instance = this;
        }

        // Subscribe to button click events from the UI
        uiController.OnButtonClicked += ButtonClicked;

        // Generate a level
        hoopArea.ResetHoops();

        // Start the main menu
        MainMenu();

        // Get starting volume of the music
        volume = GetComponent<AudioSource>().volume;    
    }

    /// <summary>
    /// Called on destroy
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from button click events from the UI
        uiController.OnButtonClicked -= ButtonClicked;
    }

    /// <summary>
    /// Shows the main menu
    /// </summary>
    private void MainMenu()
    {
        // Reset and generate the level
        hoopArea.ResetHoops();

        // Set the state to "main menu"
        State = GameState.MainMenu;

        // Update the UI
        uiController.ShowBanner("");
        uiController.ShowButton("Start");
        uiController.ShowSlider(true);
        uiController.ShowPlayerPanel(false);
        uiController.ShowOpponentPanel(false);

        // Use the main camera, disable agent cameras
        mainCamera.gameObject.SetActive(true);
        player.agentCamera.gameObject.SetActive(false);
        opponent.agentCamera.gameObject.SetActive(false); // Never turn this back on

        // Reset the agents
        player.OnEpisodeBegin();
        opponent.OnEpisodeBegin();

        // Freeze the agents
        player.FreezeAgent();
        opponent.FreezeAgent();
    }

    /// <summary>
    /// Starts the game with a countdown
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator StartGame()
    {
        //Lock the players cursor for better flying controlls with the mouse
        Cursor.lockState = CursorLockMode.Locked;

        // Set the state to "preparing"
        State = GameState.Preparing;

        // Update the UI (hide it)
        uiController.ShowBanner("");
        uiController.HideButton();
        uiController.ShowSlider(false);

        // Use the player camera, disable the main camera
        mainCamera.gameObject.SetActive(false);
        player.agentCamera.gameObject.SetActive(true);

        // Show countdown
        uiController.ShowBanner("3");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("2");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("1");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("Go!");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("");

        // Set the state to "playing"
        State = GameState.Playing;

        // Start the game timer
        gameTimerStartTime = Time.time;

        // Unfreeze the agents
        player.UnfreezeAgent();
        opponent.UnfreezeAgent();

    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private void EndGame()
    {
        //Unlock the cursor so the player can press buttons on the UI
        Cursor.lockState = CursorLockMode.None;

        // Set the game state to "game over"
        State = GameState.Gameover;

        // Freeze the agents
        player.FreezeAgent();
        opponent.FreezeAgent();

        //Update banner text depending on win/ lose
        if (player.pointsEarned >= opponent.pointsEarned)
        {
            uiController.ShowBanner("You won!");

            // if the player beats the game with max hoops, unlock the ability to have up to 50
            if(hoopSlider.value == hoopSlider.maxValue)
            {
                hoopSlider.maxValue = 50;
            }
        }
        else
        {
            uiController.ShowBanner("You lost!");
        }

        // Update button text
        uiController.ShowButton("Main Menu");
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        if (State == GameState.Playing)
        {
            // Check to see if time has run out or either agent got the max nectar amount
            if (TimeRemaining <= 0f ||
                player.pointsEarned >= hoopSlider.value ||
                opponent.pointsEarned >= hoopSlider.value)
            {
                EndGame();
            }

            // Update the timer and nectar progress bars
            uiController.SetTimer(TimeRemaining);
            uiController.SetPlayerNectar(player.pointsEarned / hoopSlider.value);
            uiController.SetOpponentNectar(opponent.pointsEarned / hoopSlider.value);
        }
        else if (State == GameState.Preparing || State == GameState.Gameover)
        {
            // Update the timer
            uiController.SetTimer(TimeRemaining);
        }
        else
        {
            // Hide the timer
            uiController.SetTimer(-1f);

            // Update the progress bars
            uiController.SetPlayerNectar(0f);
            uiController.SetOpponentNectar(0f);
        }

        //While the start up sound is playing, make sure the volume is at the right level
        if(startSound.isPlaying)
        {
            GetComponent<AudioSource>().volume = volume/2;
        }
        else if(GetComponent<AudioSource>().volume < volume)
        {
            GetComponent<AudioSource>().volume += 0.05f;
        }
    }
}
