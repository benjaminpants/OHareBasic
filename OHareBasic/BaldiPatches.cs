using System;
using System.Collections;
using System.ComponentModel;
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
        static FieldInfo rulerBroken = AccessTools.Field(typeof(Baldi), "breakRuler");

        static void Prefix(Baldi __instance, ref Animator ___animator, ref AudioManager ___audMan, ref SoundObject ___slap)
        {
            SpriteRenderer spr = __instance.spriteRenderer[0];
            spr.sprite = OHarePlugin.ohareSprite3;
            ___animator.enabled = false;
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
            __instance.Slap();
            yield return new WaitForSeconds(0.625f);
            __instance.Slap();
            yield return new WaitForSeconds(0.6f);
            __instance.Slap();
            yield return new WaitForSeconds(0.6f);
            __instance.Slap();
            yield return new WaitForSeconds(0.6f);
            __instance.Slap();
            yield return new WaitForSeconds(0.6f);
            __instance.Slap();
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
    [HarmonyPatch("Praise")]
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
    [HarmonyPatch("TakeApple")]
    class OnTriggerEnterPatch
    {

        static bool Prefix(Baldi __instance)
        {
            __instance.behaviorStateMachine.ChangeState(new OHare_StupidState(__instance, __instance, __instance.behaviorStateMachine.currentState));
            return false;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Slap")]
    class SlapPatch
    {
        static void Prefix(Baldi __instance)
        {
            SpriteRenderer spr = __instance.spriteRenderer[0];
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

    [HarmonyPatch(typeof(Baldi_Chase))]
    [HarmonyPatch("Update")]
    class AngryPatch
    {
        static void Prefix(ref float ___delayTimer)
        {
            ___delayTimer = float.PositiveInfinity;
        }
    }
}
