using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] Animator animator;
    [SerializeField] string deathTriggerName = "Death";
    [SerializeField] string deathStateName = "Rifle Death"; // <-- nombre del STATE en el Animator
    [SerializeField] float extraDelay = 0.1f;

    [Header("Opcional")]
    [SerializeField] bool disablePlayerController = true;
    [SerializeField] bool disableShooter = true;
    [SerializeField] bool deactivateAfterAnimation = false; // <-- si quieres apagar el GO al final

    Health health;
    int deathTriggerHash;

    PlayerController playerController;
    PlayerShooter playerShooter;

    void Awake()
    {
        health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        deathTriggerHash = Animator.StringToHash(deathTriggerName);

        playerController = GetComponent<PlayerController>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    void OnEnable() => health.onDied.AddListener(OnPlayerDied);
    void OnDisable() => health.onDied.RemoveListener(OnPlayerDied);

    void OnPlayerDied()
    {
        if (animator) animator.SetTrigger(deathTriggerHash);

        if (disablePlayerController && playerController) playerController.enabled = false;
        if (disableShooter && playerShooter) playerShooter.enabled = false;

        Debug.Log("JUEGO TERMINADO (Player murió)");

        if (deactivateAfterAnimation)
            StartCoroutine(WaitDeathAnimThenDeactivate());
    }

    IEnumerator WaitDeathAnimThenDeactivate()
    {
        // espera a que entre al estado
        yield return null;

        // espera mientras NO esté en el state de muerte
        while (animator && !animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName))
            yield return null;

        // espera a que termine el state
        while (animator && animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        yield return new WaitForSeconds(extraDelay);
        gameObject.SetActive(false);
    }
}
