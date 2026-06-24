using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class WeightDissolve : MonoBehaviour
{
   //public MeshRenderer weight_meshren;
   public VisualEffect dissolve_particles;
   public ShaderFloatAnimator mat_animator;
   public float dissolve_duration;
   public float hover_fade_duration;
   
   public Material socket_hover_mat;
   private Material original_material;
   
   private bool is_dissolving = false;
   private bool is_dissolved = false;
   private int dissolve_lerp_ID = Shader.PropertyToID("dissolve_duration_lerp");
   
   private void Awake() {
      mat_animator.fade_in_duration = dissolve_duration;
      mat_animator.on_fade_in_value_updated = OnFadeValueUpdated;
      mat_animator.event_on_fully_faded_in.AddListener(OnDissolveFinished);
      dissolve_particles.Stop();
      original_material = mat_animator.mesh_renderer.material;
   }

   public void Reset() {
      is_dissolving = false;
      is_dissolved = false;
      mat_animator.mesh_renderer.enabled = true;
      mat_animator.mesh_renderer.material = original_material;
      mat_animator.SetNewPropertyName("_DissolveAmount");
      mat_animator.Reset();
      mat_animator.fade_in_duration = dissolve_duration;
      mat_animator.fade_out_duration = dissolve_duration;
      dissolve_particles.Stop();
   }

   public void StartDissolve() {
      is_dissolving = true;
      dissolve_particles.Play();
      dissolve_particles.SetFloat(dissolve_lerp_ID, 0.0f);
      mat_animator.FadeIn();
   }

   private void OnFadeValueUpdated(float value_linear, float value_easing) {
      if (!is_dissolving || is_dissolved) {
         return;
      }
      dissolve_particles.SetFloat(dissolve_lerp_ID, value_linear);
   }
   
   private void OnDissolveFinished() {
      if (is_dissolving) {
         is_dissolving = false;
         is_dissolved = true;
         dissolve_particles.Stop();
         // 
         mat_animator.mesh_renderer.material = socket_hover_mat;
         mat_animator.fade_in_duration = hover_fade_duration;
         mat_animator.fade_out_duration = hover_fade_duration;
         
         mat_animator.SetNewPropertyName("_GlobalAlpha");
         mat_animator.Reset();
      }
   }

   public void FadeInHover() {
      if (is_dissolved) {
         mat_animator.FadeIn();
      }
   }

   public void FadeOutHover() {
      if (is_dissolved) {
         mat_animator.FadeOut();
      }
   }
   
   // EDITOR TEST 
   [Space]
   [Header("Editor Test Stuff")]
   public bool editor_btn_reset = false;
   public bool editor_btn_dissolve = false;
   private void OnValidate() {
      if (editor_btn_reset) {
         Reset();
      }

      editor_btn_reset = false;

      if (editor_btn_dissolve) {
         StartDissolve();
      }

      editor_btn_dissolve = false;
   }
}
