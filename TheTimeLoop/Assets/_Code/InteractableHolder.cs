using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableHolder : MonoBehaviour
{
   public XRGrabInteractable default_interactable;

   [Space]
   public XRSocketInteractor socket;

   public MeshRenderer torch_meshren;
   public MeshRenderer flashlight_meshren;

   public void DropInteractable(){
        socket.enabled = false;
        socket.enabled = true;
   }

   public void SelectDefault() {

        if (default_interactable != null){

            IXRSelectInteractable select_interactable = default_interactable as IXRSelectInteractable;

            socket.StartManualInteraction(select_interactable);
        }

        flashlight_meshren.enabled = false;
        torch_meshren.enabled = false;
   }


   public bool IsHoldingSomething(){
        return socket.hasSelection;
   }


   public void OnHoverEnter(HoverEnterEventArgs args){


      
        if (socket.hasSelection){
            return;
        }
      
        GameObject go = args.interactableObject.transform.gameObject;

        HoldableInfo info = go.GetComponent<HoldableInfo>();

        if (info == null || info.holdable_type == Holdable.None) {
            return;
        }

        if (info.holdable_type == Holdable.ClockWeight) {
            return;
        }

        if (info.holdable_type == Holdable.Flashlight){

            flashlight_meshren.enabled = true;
            return;
        }

        if (info.holdable_type == Holdable.Torch){
            torch_meshren.enabled = true;
            return;
        }
   }

   public void OnHoverExit(HoverExitEventArgs args){
        
        if (socket.hasSelection){
            return;
        }
        

        flashlight_meshren.enabled = false;
        torch_meshren.enabled = false;

   }


   public void OnSelectedSomething(){
        flashlight_meshren.enabled = false;
        torch_meshren.enabled = false;
   }


}
