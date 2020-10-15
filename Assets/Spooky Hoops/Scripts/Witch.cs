using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A hummingbird Machine Learning Agent
/// </summary>
public class Witch : Agent
{
    [Tooltip("Force to apply when moving")]
    public float moveForce = 2f;

    [Tooltip("Speed to pitch up or down")]
    public float pitchSpeed = 100f;

    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    [Tooltip("Transform of the witch AI")]
    public Transform witchTransform;

    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    // The rigidbody of the agent
    new private Rigidbody rigidbody;

    public HoopSpawner hoopArea;

    //The nearest hoop to the agent
    private GameObject nearestHoop;
    private Hoop nearestHoopScript;

    // Allows for smoother pitch changes
    private float smoothPitchChange = 0f;

    // Allows for smoother yaw changes
    private float smoothYawChange = 0f;

    // Maximum angle that the bird can pitch up or down
    private const float MaxPitchAngle = 80f;

    // Whether the agent is frozen (intentionally not flying)
    private bool frozen = false;

    //use to calculate if the agent is moving towards or away from the nearest hoop
    private float currentDistanceToHoop;
    private float lastDistanceToHoop;

    //Boolian used to check if the agent is human
    private bool humanPlayer = false;

    //Rotation speed for the camera
    public float speedH = 5.0f;
    public float speedV = 5.0f;

    //Rotation value for the camera
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    //Candy game objects
    public GameObject candy1;
    public GameObject candy2;

    //Sound player for when collecting the hoops
    private AudioSource soundPlayer;

    /// <summary>
    /// The amount of nectar the agent has obtained this episode
    /// </summary>
    public float pointsEarned { get; private set; }

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        //Obtain values for private variables
        rigidbody = GetComponent<Rigidbody>();
        soundPlayer = GetComponent<AudioSource>();

        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            //Reset the hoops in the training area
            hoopArea.ResetHoops();
           // UpdateNearestHoop();
        }

        // Reset points obtained
        pointsEarned = 0f;

        // Zero out velocities so that movement stops before a new episode begins
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Default to spawning in front of a hoop
        bool inFontofHoop = true;
        if (trainingMode)
        {
            // Spawn in front a hoop 50% of the time during training
            inFontofHoop = UnityEngine.Random.value > .5f;
        }

        // Move the agent to a new random position
        MoveToSafeRandomPosition(inFontofHoop);

        // Recalculate the nearest hoop now that the agent has moved
        UpdateNearestHoop();
    }

    /// <summary>
    /// Called when and action is received from either the player input or the neural network
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector y (+1 = up, -1 = down)
    /// Index 2: move vector z (+1 = forward, -1 = backward)
    /// Index 3: pitch angle (+1 = pitch up, -1 = pitch down)
    /// Index 4: yaw angle (+1 = turn right, -1 = turn left)
    /// </summary>
    /// <param name="vectorAction">The actions to take</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Don't take actions if frozen
        if (frozen) return;

        // Calculate movement vector
        Vector3 move = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);

        // Add force in the direction of the move vector
        rigidbody.AddForce(move * moveForce);

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // Calculate pitch and yaw rotation
        float pitchChange = vectorAction[3];
        float yawChange = vectorAction[4];

        // Calculate smooth rotation changes
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        // Calculate new pitch and yaw based on smoothed values
        // Clamp  pitch to avoid flipping upside down
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // If nearestHoop is null, observe an empty array and return early
        if (nearestHoop == null)
        {
            sensor.AddObservation(new float[10]);
            return;
        }

        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        // Get a vector from the beak tip to the nearest hoop
        Vector3 toRing = nearestHoopScript.RingCenterPosition - witchTransform.position;

        // Observe a normalized vector pointing to the nearest hoop (3 observations)
        sensor.AddObservation(toRing.normalized);

        // Observe a dot product that indicates whether the beak tip is in front of the flower (1 observation)
        // (+1 means that the agent is directly in front of the hoop, -1 means directly behind)
        sensor.AddObservation(Vector3.Dot(toRing.normalized, -nearestHoopScript.HoopUpVector.normalized));

        // Observe a dot product that indicates whether the agent is pointing toward the hoop (1 observation)
        // (+1 means that the agent is pointing directly at the hoop, -1 means directly away)
        sensor.AddObservation(Vector3.Dot(witchTransform.forward.normalized, -nearestHoopScript.HoopUpVector.normalized));

        // Observe the relative distance from the agent to the hoop (1 observation)
        sensor.AddObservation(toRing.magnitude / FlowerArea.AreaDiameter);

        // 10 total observations
    }

    /// <summary>
    /// When Behavior Type is set to "Heuristic Only" on the agent's Behavior Parameters,
    /// this function will be called. Its return values will be fed into
    /// <see cref="OnActionReceived(float[])"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">And output action array</param>
    public override void Heuristic(float[] actionsOut)
    {
        humanPlayer = true;
        // Create placeholders for all movement/turning
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;
        Vector3 up = Vector3.zero;
        //float pitch = 0f;
        //float yaw = 0f;

        // Convert keyboard inputs to movement and turning
        // All values should be between -1 and +1

        //// Forward/backward
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;

        ////// Left/right
        //if (Input.GetKey(KeyCode.A)) left = -transform.right;
        //else if (Input.GetKey(KeyCode.D)) left = transform.right;

        // Up/down
        if (Input.GetKey(KeyCode.E)) up = transform.up;
        else if (Input.GetKey(KeyCode.C)) up = -transform.up;

        //// Pitch up/down
        //if (Input.GetKey(KeyCode.UpArrow)) pitch = 1f;
        //else if (Input.GetKey(KeyCode.DownArrow)) pitch = -1f;

        //// Turn left/right
        //if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f;
        //else if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;

        //float y = Input.GetAxis("Mouse X") * yawSpeed;
        //rotX += Input.GetAxis("Mouse Y") * pitchSpeed;

        ////clamp the vertical rotation
        //rotX = Mathf.Clamp(rotX, -90, 90);

        ////rotate the camera
        //transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);

        // Combine the movement vectors and normalize
        Vector3 combined = (forward + left + up).normalized;

        // Add the 3 movement values, pitch, and yaw to the actionsOut array
        actionsOut[0] = combined.x;
        actionsOut[1] = combined.y;
        actionsOut[2] = combined.z;
        //actionsOut[3] = pitch;
        //actionsOut[4] = yaw;
    }

    /// <summary>
    /// Prevent the agent from moving and taking actions
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        rigidbody.Sleep();

        Debug.Log("Agent frozen");
    }

    /// <summary>
    /// Resume agent movement and actions
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        rigidbody.WakeUp();

        Debug.Log("Agent unfrozen");
    }

    /// <summary>
    /// Move the agent to a safe random position (i.e. does not collide with anything)
    /// If in front of hoop, also point the agent at the hoop
    /// </summary>
    /// <param name="inFrontOfFHoop">Whether to choose a spot in front of a flower</param>
    private void MoveToSafeRandomPosition(bool inFrontOfFHoop)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 200; // Prevent an infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // Loop until a safe position is found or we run out of attempts
        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            if (inFrontOfFHoop)
            {
                // Pick a random flower
                GameObject randomHoop = hoopArea.hoops[UnityEngine.Random.Range(0, hoopArea.hoops.Count)];
                Hoop randomHoopScript = randomHoop.GetComponent<Hoop>();

                // Position 30 to 40 cm in front of the hoop
                float distanceFromFlower = UnityEngine.Random.Range(.3f, .4f);
                potentialPosition = randomHoop.transform.position + randomHoopScript.HoopUpVector * distanceFromFlower;

                // Point agent at hoop
                Vector3 toHoop = randomHoopScript.RingCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toHoop, Vector3.up);
            }
            else
            {
                // Pick a random height from the ground
                float height = UnityEngine.Random.Range(1.2f, 2.5f);

                // Pick a random radius from the center of the area
                float radius = UnityEngine.Random.Range(2f, 7f);

                // Pick a random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // Combine height, radius, and direction to pick a potential position
                potentialPosition = hoopArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

                // Choose and set random starting pitch and yaw
                float pitch = UnityEngine.Random.Range(-60f, 60f);
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Check to see if the agent will collide with anything
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

            // Safe position has been found if only one collider overlapped
            safePositionFound = colliders.Length == 1;

            //if statemate that is only active in training mode, used for recording the last known position to a hoop
            if(trainingMode)
            lastDistanceToHoop = Vector3.Distance(transform.position, nearestHoop.transform.position);
        }

        //If the loop is unable to find a safe space to spawn based on the collider check, exit the loop
        Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

        // Set the position and rotation
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }

    /// <summary>
    /// Update the nearest flower to the agent
    /// </summary>
    private void UpdateNearestHoop()
    {
        foreach (GameObject hoop in hoopArea.hoops)
        {
            if (nearestHoop == null && hoop != null)
            {
                // No current nearest hoop, so set to this hoop
                nearestHoop = hoop;
                nearestHoopScript = nearestHoop.GetComponent<Hoop>();
            }
            else if (hoop != null)
            {
                // Calculate distance to this hoop and distance to the current nearest hoop
                float distanceToFlower = Vector3.Distance(hoop.transform.position, witchTransform.position);
                float distanceToCurrentNearestFlower = Vector3.Distance(nearestHoop.transform.position, witchTransform.position);

                // If current nearest hoop is empty OR this flower is hoop, update the nearest flower
                if (nearestHoop == null || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestHoop = hoop;
                    nearestHoopScript = nearestHoop.GetComponent<Hoop>();
                }
            }
        }
    }

    /// <summary>
    /// Handles when the agen'ts collider enters or stays in a trigger collider
    /// </summary>
    /// <param name="collider">The trigger collider</param>
    private void OnTriggerEnter(Collider collider)
    {
        // Check if agent is colliding with the middle of the hoop
        if (collider.CompareTag("target"))
        {
            Instantiate(candy1, collider.transform.position, Quaternion.identity);
            Instantiate(candy2, collider.transform.position, Quaternion.identity);

            Destroy(collider.gameObject);

            pointsEarned++;

            soundPlayer.Play();

            //Reward the agent for going through the hoop
            if (trainingMode)
            {
                AddReward(5f);
            }

            // If hoop is null, update the nearest hoop
            if (nearestHoop == null)
            {
                UpdateNearestHoop();
            }
        }
    }

    /// <summary>
    /// Called when the agent collides with something solid
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("boundary"))
        {
            // Collided with the area boundary, give a negative reward
            AddReward(-.5f);
        }
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        // Draw a line from the agent to the nearest hoop
        if (nearestHoop != null)
            Debug.DrawLine(witchTransform.position, nearestHoopScript.RingCenterPosition, Color.green);

        //Rotate the player with the use of the mouse if the game has started
        if (humanPlayer && GameManager.Instance.State == GameManager.GameState.Playing || humanPlayer && GameManager.Instance.State == GameManager.GameState.Preparing)
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

    /// <summary>
    /// Called every .02 seconds
    /// </summary>
    private void FixedUpdate()
    {
        // Avoids scenario where nearest hoop is stolen by opponent and not updated
        if (nearestHoop == null)
            UpdateNearestHoop();

        //if statement to check if the agent is in training mode and the nearest hoop is not null
        if (trainingMode && nearestHoop != null)
        {
            //Calculate the current distance to the hoop
            currentDistanceToHoop = Vector3.Distance(transform.position, nearestHoop.transform.position);

            //if the agent is closer to the hoop in this frame, then reward the agent
            if (currentDistanceToHoop > lastDistanceToHoop)
            {
                AddReward(-0.001f);
            }
            //if the agent is farther or the same distance from the hoop than the last frame, then punish the agent
            else
            {
                AddReward(0.001f);
            }

            //Update the last distance to hoop for a future check next frame
            lastDistanceToHoop = currentDistanceToHoop;
        }
    }
}
