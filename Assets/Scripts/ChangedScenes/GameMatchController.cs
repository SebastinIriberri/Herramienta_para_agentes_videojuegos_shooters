using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMatchController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Health del jugador (arrástralo desde el Player)")]
    public Health playerHealth;

    [Tooltip("Opcional: tu SceneLoader existente. Si no lo asignas, lo busca en este mismo GameObject.")]
    public SceneLoader sceneLoader;

    [Header("Escenas")]
    public string victorySceneName = "Victory";
    public string gameOverSceneName = "GameOver";

    [Header("Tiempo antes de cambiar de escena")]
    [Min(0f)] public float endSceneDelay = 1.5f;

    [Header("Debug")]
    [SerializeField] private int enemiesAlive = 0;

    // Para evitar que se dispare victoria/derrota dos veces
    private bool gameEnded = false;

    // Guardamos enemigos registrados para no duplicarlos
    private readonly HashSet<Health> registeredEnemies = new HashSet<Health>();

    void Awake()
    {
        if (sceneLoader == null)
            sceneLoader = GetComponent<SceneLoader>();

        // Extra: si SceneLoader está en otro objeto de la escena
        if (sceneLoader == null)
            sceneLoader = FindFirstObjectByType<SceneLoader>();
    }

    void Start()
    {
        // 1) Buscar/validar player
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerHealth = playerObj.GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("[GameMatchController] No se encontró playerHealth. Asigna el Health del Player en el inspector.");
            return;
        }

        // Suscribir derrota del jugador
        playerHealth.onDied.AddListener(OnPlayerDied);

        // 2) Registrar enemigos iniciales en escena
        RegisterEnemiesInScene();

        // Si por alguna razón no hay enemigos en escena, puedes decidir si ganar automático
        if (enemiesAlive <= 0)
        {
            Debug.LogWarning("[GameMatchController] No hay enemigos registrados al iniciar la escena.");
            // Si quieres victoria inmediata, descomenta:
            // StartCoroutine(LoadEndScene(victorySceneName));
        }
    }

    void RegisterEnemiesInScene()
    {
        // Busca todos los EnemyManager activos en la escena
        EnemyManager[] enemies = FindObjectsOfType<EnemyManager>();

        foreach (EnemyManager enemy in enemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                RegisterEnemy(enemyHealth);
            }
        }

        Debug.Log($"[GameMatchController] Enemigos registrados: {enemiesAlive}");
    }

    /// <summary>
    /// Registra un enemigo para contar su muerte.
    /// Útil también si en el futuro spawneas enemigos dinámicamente.
    /// </summary>
    public void RegisterEnemy(Health enemyHealth)
    {
        if (enemyHealth == null) return;
        if (registeredEnemies.Contains(enemyHealth)) return;
        if (enemyHealth.IsDead) return; // No contar enemigos ya muertos

        registeredEnemies.Add(enemyHealth);
        enemiesAlive++;

        // Escuchar muerte de este enemigo
        enemyHealth.onDied.AddListener(() => OnEnemyDied(enemyHealth));
    }

    void OnEnemyDied(Health enemyHealth)
    {
        if (gameEnded) return;
        if (enemyHealth == null) return;

        // Evitar restar dos veces por seguridad
        if (!registeredEnemies.Contains(enemyHealth)) return;

        registeredEnemies.Remove(enemyHealth);
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        Debug.Log($"[GameMatchController] Enemigo eliminado. Restantes: {enemiesAlive}");

        if (enemiesAlive == 0)
        {
            gameEnded = true;
            StartCoroutine(LoadEndScene(victorySceneName));
        }
    }

    void OnPlayerDied()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("[GameMatchController] Player murió -> Game Over");
        StartCoroutine(LoadEndScene(gameOverSceneName));
    }

    IEnumerator LoadEndScene(string sceneName)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Usa realtime por si en algún momento pausas el juego
        yield return new WaitForSecondsRealtime(endSceneDelay);

        if (sceneLoader != null)
            sceneLoader.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
