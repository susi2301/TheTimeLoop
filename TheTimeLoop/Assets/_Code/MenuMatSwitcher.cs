using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMatSwitcher : MonoBehaviour {

    public List<MeshRenderer> meshrens;
    public SkinnedMeshRenderer skinned_meshren;
    
    // ASUMING BY DEFAULT IT IS USING IN GAME MATERIAL!
    public bool is_using_menu_mat = false;

    public Material in_game_mat;
    public Material in_menu_mat;

    public void SwitchToMenuMat() {
        if (is_using_menu_mat) {
            return;
        }

        is_using_menu_mat = true;
        

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

        if (!is_using_menu_mat){
            return;
        }

        is_using_menu_mat = false;

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
