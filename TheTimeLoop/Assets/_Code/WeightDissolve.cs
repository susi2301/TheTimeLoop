using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class WeightDissolve : MonoBehaviour
{
   public MeshRenderer weight_meshren;
   public VisualEffect dissolve_particles;
   public HoverMaterialAnimator mat_animator;
   public float duration;

   private bool is_dissolving = false;
   private int dissolve_lerp_ID = Shader.PropertyToID("dissolve_duration_lerp");
   
   private void Awake() {
      mat_animator.fade_in_duration = duration;
      mat_animator.on_fade_in_value_updated = OnFadeValueUpdated;
      mat_animator.event_on_faded_in.AddListener(OnDissolveFinished);
      dissolve_particles.Stop();
   }

   public void Reset() {
      is_dissolving = false;
      mat_animator.Reset();
      dissolve_particles.Stop();
      weight_meshren.enabled = true;
      mat_animator.fade_in_duration = duration;
   }

   public void StartDissolve() {
      is_dissolving = true;
      dissolve_particles.Play();
      dissolve_particles.SetFloat(dissolve_lerp_ID, 0.0f);
      mat_animator.FadeIn();
   }

   private void OnFadeValueUpdated(float value) {
      if (!is_dissolving) {
         return;
      }
      dissolve_particles.SetFloat(dissolve_lerp_ID, value);
   }
   
   private void OnDissolveFinished() {
      is_dissolving = false;
      dissolve_particles.Stop();
      weight_meshren.enabled = false;
   }

   // EDITOR TEST 
   [Space]
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
