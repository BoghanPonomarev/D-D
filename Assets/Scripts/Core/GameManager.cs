using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] AssetLibrary assetLibrary;

    public AssetLibrary Assets => assetLibrary;
    public ObjectPoolManager Pool { get; private set; }
    public SoundManager Sound { get; private set; }
    public UIManager UI { get; private set; }
    public DataManager Data { get; private set; }
    public LocalizationManager Localization { get; private set; }
    public TimeManager GameTime { get; private set; }
    public ResidentManager Residents { get; private set; }
    
    public BuildingManager BuildingManager { get; private set; }
    public CrowdManager Crowd { get; private set; }

    StateMachine stateMachine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Pool = GetComponentInChildren<ObjectPoolManager>();
        Sound = GetComponentInChildren<SoundManager>();
        UI = GetComponentInChildren<UIManager>();
        Data = GetComponentInChildren<DataManager>();
        Localization = GetComponentInChildren<LocalizationManager>();
        GameTime = GetComponentInChildren<TimeManager>();
        Residents = GetComponentInChildren<ResidentManager>();
        BuildingManager = GetComponentInChildren<BuildingManager>();
        Crowd = GetComponentInChildren<CrowdManager>();

        stateMachine = new StateMachine();
        stateMachine.ChangeState(new InitState(this));
    }

    void Update()
    {
        stateMachine.Tick();
    }

    public void TransitionTo(GameState state)
    {
        stateMachine.ChangeState(state);
    }
}
