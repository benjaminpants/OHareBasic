using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace OHareBasic
{
    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Start")]
    class StartPatch
    {
        static FieldInfo subtitleColor = AccessTools.Field(typeof(AudioManager), "subtitleColor");
        static FieldInfo rulerBroken = AccessTools.Field(typeof(Baldi), "rulerBroken");

        static void Prefix(Baldi __instance, ref Animator ___animator, ref float ___delay, ref AudioManager ___audMan, ref SoundObject ___slap)
        {
            SpriteRenderer spr = __instance.gameObject.transform.Find("SpriteBase").Find("Sprite").GetComponent<SpriteRenderer>();
            spr.sprite = OHarePlugin.ohareSprite3;
            ___animator.enabled = false;
            ___delay = float.PositiveInfinity;
            subtitleColor.SetValue(___audMan, new Color(0f, 153f / 255f, 236 / 255f));
            ___audMan.PlaySingle(OHarePlugin.ohare_ISay);
            ___slap = OHarePlugin.ohare_clap;
            __instance.StartCoroutine(StartTheHell(__instance, spr));
        }
        static IEnumerator StartTheHell(Baldi __instance, SpriteRenderer spr)
        {
            bool rb = (bool)rulerBroken.GetValue(__instance);
            if (!rb)
            {
                yield return new WaitForSeconds(1.35f);
            }
            __instance.AudMan.PlaySingle(rb ? OHarePlugin.ohare_letItDieFunky : OHarePlugin.ohare_letItDie);
            yield return new WaitForSeconds(0.925f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.625f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.6f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.6f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.6f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.6f);
            __instance.ManualSlap();
            yield return new WaitForSeconds(0.40f);
            // give them a break from the O'Hell
            if (!rb)
            {
                __instance.AudMan.PlaySingle(OHarePlugin.ohare_whoWithMe);
                spr.sprite = OHarePlugin.ohareSprite4;
                yield return new WaitForSeconds(1.0f);
                spr.sprite = OHarePlugin.ohareSprite3;
            }
            __instance.StartCoroutine(StartTheHell(__instance, spr));
            yield break;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Pause")]
    class PausePatch
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("BreakRuler")]
    class BreakRulerPatch
    {
        static void Postfix(ref bool ___breakRuler)
        {
            ___breakRuler = false; //turn off the sound
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("OnTriggerEnter")]
    class OnTriggerEnterPatch
    {

        static FieldInfo fi = AccessTools.Field(typeof(Baldi), "eatingApple");

        static bool Prefix(Baldi __instance, Collider other, ref bool ___eatingApple, ref bool ___paused, ref bool ___rulerBroken)
        {
            if (___eatingApple)
            {
                return false;
            }
            if (other.tag == "Player")
            {
                PlayerManager component = other.GetComponent<PlayerManager>();
                ItemManager itm = component.itm;
                if (!component.invincible && !___paused)
                {
                    if (itm.Has(Items.Apple) && !___eatingApple)
                    {
                        itm.Remove(Items.Apple);
                        MovementModifier mm = new MovementModifier((__instance.Navigator.Velocity.normalized * component.plm.runSpeed), 0.1f);
                        ActivityModifier am = __instance.GetComponent<ActivityModifier>();
                        am.moveMods.Add(mm);
                        __instance.AudMan.PlaySingle(OHarePlugin.airWind);
                        __instance.StartCoroutine(Air(__instance, am, mm));
                        ___eatingApple = true;
                        return false;
                    }
                }
            }
            return !___rulerBroken;
        }

        static IEnumerator Air(Baldi instance, ActivityModifier am, MovementModifier mm)
        {
            float time = instance.appleTime;
            while (time > 0f)
            {
                time -= Time.deltaTime * instance.ec.NpcTimeScale;
                yield return null;
            }
            instance.AudMan.FlushQueue(true);
            fi.SetValue(instance, false); //because we cant have refs in ienumerators
            am.moveMods.Remove(mm);
            yield break;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Slapped")]
    class SlapPatch
    {
        static void Prefix(Baldi __instance)
        {
            SpriteRenderer spr = __instance.gameObject.transform.Find("SpriteBase").Find("Sprite").GetComponent<SpriteRenderer>();
            spr.sprite = OHarePlugin.ohareSprite2;
            __instance.StopCoroutine(ResetAnimation(spr));
            __instance.StartCoroutine(ResetAnimation(spr));
        }

        static IEnumerator ResetAnimation(SpriteRenderer spr)
        {
            yield return new WaitForSeconds(0.2f);
            spr.sprite = OHarePlugin.ohareSprite5;
            yield return new WaitForSeconds(0.1f);
            spr.sprite = OHarePlugin.ohareSprite1;
            yield break;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("GetAngry")]
    class AngryPatch
    {
        static void Postfix(ref float ___delay)
        {
            ___delay = float.PositiveInfinity;
        }
    }
}
