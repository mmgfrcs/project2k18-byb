using UnityEngine.UI;
using UnityEngine;

public class DepartmentPanel : MonoBehaviour {

    public Text departmentName;
    public Slider trustBar;
    public Image[] specialAbilityImages;
    public Text workSpeedValue, workSpeedLbl, stat1Label, stat1Value, stat2Label, stat2Value, managerBtnText;

    public void OnActivate()
    {
        GameManager.ActivateSelected();
    }
}
