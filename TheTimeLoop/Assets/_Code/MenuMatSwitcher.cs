using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMatSwitcher : MonoBehaviour {

    public List<MeshRenderer> meshrens;
    public SkinnedMeshRenderer skinned_meshren;

    public Material in_game_mat;
    public Material in_menu_mat;

    public void SwitchToMenuMat() {
        if (meshrens != null && meshrens.Count > 0) {
            for (int i = 0; i < meshrens.Count; i++) {
                meshrens[i].material = in_menu_mat;
            }
        }
        if (skinned_meshren != null){
            skinned_meshren.material = in_menu_mat;
        }
    }

    public void SwitchToInGameMat(){
        if (meshrens != null && meshrens.Count > 0){
            for (int i = 0; i < meshrens.Count; i++) {
                meshrens[i].material = in_game_mat;
            }
        }

        if (skinned_meshren != null){
            skinned_meshren.material = in_game_mat;
        }
    }
}
