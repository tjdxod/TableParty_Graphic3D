using System.Text;
using Dive.VRModule;
using UnityEngine;

public class TestInput : MonoBehaviour
{
    [SerializeField]
    private PXRInputHandlerBase controller;

    [SerializeField]
    private HandSide handSide;
    
    [SerializeField]
    private bool useTriggerDebug = false;
    
    [SerializeField]
    private bool useGripDebug = false;
    
    [SerializeField]
    private bool useMenuDebug = false;
    
    [SerializeField]
    private bool usePrimaryButtonDebug = false;
    
    [SerializeField]
    private bool useSecondaryButtonDebug = false;
    
    [SerializeField]
    private bool usePrimary2DAxisDebug = false;
    
    [SerializeField]
    private bool useSecondary2DAxisDebug = false;
    
    private StringBuilder builder = null;
    private string result = string.Empty;

    void Start()
    {
        builder = new StringBuilder();
    }
    
    void Update()
    {
        if(controller.handSide != handSide)
            return;
        
        if(useTriggerDebug == false && useGripDebug == false && useMenuDebug == false && usePrimaryButtonDebug == false && useSecondaryButtonDebug == false && usePrimary2DAxisDebug == false && useSecondary2DAxisDebug == false)
            return;
        
        builder.Clear();
        
        if(useTriggerDebug)
            Trigger();
        
        if(useGripDebug)
            Grip();
    
        if(useMenuDebug)
            Menu();
    
        if(usePrimaryButtonDebug)
            PrimaryButton();
    
        if(useSecondaryButtonDebug)
            SecondaryButton();
    
        if(usePrimary2DAxisDebug)
            Primary2DAxis();
    
        if(useSecondary2DAxisDebug)
            Secondary2DAxis();

        Debug.Log(builder.ToString());
    }

    private void Trigger()
    {
        var trigger = controller.GetButtonState(Buttons.Trigger);
        builder.AppendLine($"Trigger isTouch: {trigger.isTouch}, Trigger isDown : {trigger.isDown}, Trigger Value: {trigger.value}");
    }

    private void Grip()
    {
        var grip = controller.GetButtonState(Buttons.Grip);
        builder.AppendLine($"Grip isTouch: {grip.isTouch}, Grip isDown : {grip.isDown}, Grip Value: {grip.value}");
    }
    
    private void Menu()
    {
        var menu = controller.GetButtonState(Buttons.Menu);
        builder.AppendLine($"Menu isTouch: {menu.isTouch}, Menu isDown : {menu.isDown}, Menu Value: {menu.value}");
    }

    private void PrimaryButton()
    {
        var primaryButton = controller.GetButtonState(Buttons.Primary);
        builder.AppendLine($"PrimaryButton isTouch: {primaryButton.isTouch}, PrimaryButton isDown : {primaryButton.isDown}, PrimaryButton Value: {primaryButton.value}");
    }

    private void SecondaryButton()
    {
        var secondaryButton = controller.GetButtonState(Buttons.Secondary);
        builder.AppendLine($"SecondaryButton isTouch: {secondaryButton.isTouch}, SecondaryButton isDown : {secondaryButton.isDown}, SecondaryButton Value: {secondaryButton.value}");

    }

    private void Primary2DAxis()
    {
        var primary2DAxis = PXRInputBridge.GetXRController(HandSide.Right).GetAxisValue(ControllerAxis.Primary);
        var buttonState = controller.GetButtonState(Buttons.Primary);
        builder.AppendLine($"Primary2DAxis: {primary2DAxis}, Primary2DAxis isTouch: {buttonState.isTouch}, Primary2DAxis isDown : {buttonState.isDown}, Primary2DAxis Value: {buttonState.value}");
    }

    private void Secondary2DAxis()
    {
        var secondary2DAxis = PXRInputBridge.GetXRController(HandSide.Right).GetAxisValue(ControllerAxis.Secondary);
        var buttonState = controller.GetButtonState(Buttons.Secondary);
        builder.AppendLine($"Secondary2DAxis: {secondary2DAxis}, Secondary2DAxis isTouch: {buttonState.isTouch}, Secondary2DAxis isDown : {buttonState.isDown}, Secondary2DAxis Value: {buttonState.value}");
    }
}
