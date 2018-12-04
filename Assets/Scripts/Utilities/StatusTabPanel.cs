using UnityEngine.UI;
using UnityEngine;

public class StatusTabPanel : MonoBehaviour {
    internal DepartmentBase department;
    public Text departmentName;
    public Slider trustSlider, employeeSlider;
    public Toggle managerToggle;
    public Button hireButton, fireButton, managerHireButton;

    Text hireText, managerHireText;

    private void Start()
    {
        managerHireButton.interactable = false;
        hireText = hireButton.GetComponentInChildren<Text>();
        managerHireText = managerHireButton.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if (GameManager.Cash < department.StaffHireCost || department.CurrentStaff >= department.MaximumStaff) hireButton.interactable = false;
        else hireButton.interactable = true;
        if (department.CurrentStaff > 0) fireButton.interactable = true;
        else fireButton.interactable = false;

        hireText.text = string.Format("Hire (${0:N0})", department.StaffHireCost);
        managerHireText.text = string.Format("Hire (${0:N0})", department.ManagerHireCost);
        //TODO Manager
        managerToggle.isOn = false;
    }

    public void OnHireStaff()
    {
        GameManager.Cash -= department.StaffHireCost;
        department.AddStaff();
    }

    public void OnFireStaff()
    {
        department.RemoveStaff();
    }

    public void OnHireManager()
    {

    }
}
