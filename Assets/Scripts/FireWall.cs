using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireWall : MonoBehaviour
{
    private PlayerControls _controls;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerStats playerStats;
    public GameObject wallPrefab;
    private GameObject wallObject;
    public float maxWallLength = 15;
    public float maxWallHeight = 10;
    private float wallLength;
    public float cooldownTime = 3f;
    private bool walled;
    public float manaCost = 700;
    private IEnumerator coroutine;

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Wall.started += Wall;
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();
        walled = false;
        wallLength = maxWallLength;
    }

    // Update is called once per frame
    void Update()
    {
        if (walled)
        {
            Vector3 wallScale = wallObject.transform.localScale;
            Vector3 pos = wallObject.transform.forward * wallObject.transform.localScale.z + wallObject.transform.position;
            // Debug.DrawRay(pos, Vector3.up, Color.green);
            Vector3 groundRay = new Vector3(pos.x, pos.y + 1f, pos.z);
            RaycastHit ground;

            if (!Physics.Raycast(groundRay, Vector3.down, out ground, 1) && wallLength == maxWallLength)
            {
                wallLength = Vector3.Distance(groundRay, transform.position) - 1f;
            }
            else if (Physics.Raycast(groundRay, wallObject.transform.forward, out ground, 0.1f) && wallLength == maxWallLength && ground.collider != null && ground.collider.tag != "Enemy")
            {
                wallLength = Vector3.Distance(groundRay, transform.position);
            }

            Debug.DrawRay(groundRay, Vector3.down, Color.green);
            Debug.DrawRay(groundRay, wallObject.transform.forward * 0.1f, Color.red);

            Vector3 targetScale = Vector3.Lerp(wallScale, new Vector3(wallScale.x, maxWallHeight, wallLength), Time.deltaTime * 2);
            wallObject.transform.localScale = targetScale;
        }
    }

    private void Wall(InputAction.CallbackContext context)
    {
        if (playerAttack.hasSword && !walled && playerStats.CheckWall(manaCost))
        {
            animator.SetTrigger("FireWall");
        }
    }


    private void WallEvent()
    {
        if (!walled)
        {
            wallObject = Instantiate(wallPrefab, transform.position + transform.forward * 0.5f + Vector3.up * -0.4f, transform.rotation);
            walled = true;
            coroutine = cooldown(cooldownTime);
            playerStats.useMana(manaCost);
            StartCoroutine(coroutine);
        }
    }

    private void PlaySwordSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Sword/Sword Swing", gameObject);
    }

    private IEnumerator cooldown(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Object.Destroy(wallObject);
        walled = false;
        wallLength = maxWallLength;
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }
}
