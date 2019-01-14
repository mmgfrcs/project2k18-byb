using UnityEngine.UI;
using UnityEngine;

public class VictoryDefeatEventPanel : EventPanel {
    public Text daysValue, daysScore, netWorthScore, victoryValue, victoryScore, totalScore;
    public GameObject victoryLine;

    public void OnMainMenu()
    {
        GameManager.OnMainMenu();
    }
}
