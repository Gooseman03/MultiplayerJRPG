using UnityEngine;
using UnityEngine.UIElements;

public class StatsUiController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement HealthBar => uiDocument.rootVisualElement.Q<VisualElement>("HealthBar");
    private VisualElement ManaBar => uiDocument.rootVisualElement.Q<VisualElement>("ManaBar");
    [SerializeField] private TempPlayerStats Health = new();
    [SerializeField] private TempPlayerStats Mana = new();
    private void Awake()
    {
        HealthBar.dataSource = Health;
        ManaBar.dataSource = Mana;
    }
}
