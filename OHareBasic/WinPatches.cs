using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetManager;
using UnityEngine;
using UnityEngine.UI;

namespace OHareBasic
{

    public class VictoryCharacter : MonoBehaviour
    {

        public SpriteRenderer spriteR;
        public SoundObject toPlayWhenSing;
        public AudioManager audMan;
        public AudioSource glitchSource;
        public float startingY;
        const float startingDist = 15f;
        private bool glitching;

        public void CrashSound()
        {
            ShutUp();
            glitchSource.clip = Singleton<LocalizationManager>.Instance.GetLocalizedAudioClip(toPlayWhenSing);
            glitchSource.Play();
            float tim = UnityEngine.Random.Range(0f, glitchSource.clip.length);
            glitchSource.time = tim;
            glitching = true;
            StartCoroutine(CrashSound(tim));
        }

        public void ShutUp()
        {
            glitching = false;
            audMan.FlushQueue(true);
            audMan.audioDevice.Stop();
        }

        private IEnumerator CrashSound(float sampleTime)
        {
            while (glitching)
            {
                glitchSource.time = sampleTime;
                yield return null;
            }
            glitchSource.Stop();
            yield break;
        }

        public void Awake()
        {
            startingY = transform.position.y;
        }

        public void Sink()
        {
            transform.position += Vector3.down * startingDist;
        }

        public void Sing()
        {
            audMan.PlaySingle(toPlayWhenSing);
        }

        public void SetSprite(Sprite spr)
        {
            spriteR.sprite = spr;
        }

        public void Rise()
        {
            MoveTo(new Vector3(transform.position.x, startingY, transform.position.z), 1f);
        }

        public void MoveTo(Vector3 targetPosition, float time)
        {
            StartCoroutine(Move(targetPosition, time));
        }

        public void MoveRelative(Vector3 offsetPosition, float time)
        {
            MoveTo(transform.position + offsetPosition, time);
        }

        IEnumerator Move(Vector3 targetPosition, float time)
        {
            if (targetPosition == transform.position) yield break;
            float tween = 0f;
            Vector3 startingPosition = transform.position;
            while (tween <= 1f)
            {
                tween += Time.deltaTime / time;
                transform.position = Vector3.Lerp(startingPosition, targetPosition, tween);
                yield return null;
            }
            //just to make sure we are exactly there
            transform.position = targetPosition;
            yield break;
        }
    }


    [HarmonyPatch(typeof(PlaceholderWinManager))]
    [HarmonyPatch("BeginPlay")]
    class WinBeginPlayPatch
    {
        static FieldInfo baldiSprite = AccessTools.Field(typeof(BaldiDance), "baldiSprite");
        static FieldInfo audMan = AccessTools.Field(typeof(BaldiDance), "audMan");
        static FieldInfo subtitleColor = AccessTools.Field(typeof(AudioManager), "subtitleColor");
        static FieldInfo overrideSubtitleColor = AccessTools.Field(typeof(AudioManager), "overrideSubtitleColor");
        static FieldInfo crashAudioSource = AccessTools.Field(typeof(BaldiDance), "crashAudioSource");

        static VictoryCharacter CreateDummyCharacter(BaldiDance reference, Sprite toSet, Color subColor, Vector3 posOffset, float downOffset = 0f, SoundObject toSing = null)
        {
            BaldiDance bd = GameObject.Instantiate<BaldiDance>(reference);
            GameObject gb = bd.gameObject;
            VictoryCharacter vc = gb.AddComponent<VictoryCharacter>();
            vc.spriteR = (SpriteRenderer)baldiSprite.GetValue(bd);
            vc.audMan = (AudioManager)audMan.GetValue(bd);
            subtitleColor.SetValue(vc.audMan, subColor);
            overrideSubtitleColor.SetValue(vc.audMan, true);
            vc.glitchSource = (AudioSource)crashAudioSource.GetValue(bd);
            GameObject.Destroy(bd);
            vc.SetSprite(toSet);
            vc.transform.position += posOffset;
            vc.transform.position += Vector3.down * downOffset;
            vc.startingY = vc.transform.position.y;
            vc.toPlayWhenSing = toSing;
            vc.Sink();
            gb.name = toSet.name;
            return vc;
        }

        static bool Prefix(PlaceholderWinManager __instance,ref MovementModifier ___moveMod, int ___balloonCount, Balloon[] ___balloonPre, ref BaldiDance ___dancingBaldi, ref GameObject ___endingError, ref GameObject ___blackScreen)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Singleton<CoreGameManager>.Instance.GetPlayer(0).Am.moveMods.Add(___moveMod);
            Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.enabled = false;
            ___dancingBaldi.gameObject.SetActive(true);
            SpriteRenderer sprit = (SpriteRenderer)baldiSprite.GetValue(___dancingBaldi);
            AudioManager aud = (AudioManager)audMan.GetValue(___dancingBaldi);
            subtitleColor.SetValue(aud, new Color(0f, 153f / 255f, 236 / 255f));
            for (int i = 0; i < ___balloonCount; i++)
            {
                UnityEngine.Object.Instantiate<Balloon>(___balloonPre[UnityEngine.Random.Range(0, ___balloonPre.Length)], __instance.Ec.transform).Initialize(__instance.Ec.rooms[0]);
            }
            List<Sprite> sprites = Resources.FindObjectsOfTypeAll<Sprite>().ToList();
            List<VictoryCharacter> characters = new List<VictoryCharacter>
            {
                // Principal
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "Principal"), new Color(0, 0.1176f, 0.4824f), Vector3.left * 7f, 0f, OHarePlugin.characterSinging[0]),
                // Baldi
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "DanceSheet_0"), new Color(0f,1f,0f), Vector3.right * 7f, 0f, OHarePlugin.characterSinging[1]),
                // Playtime
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "Playtime_0"), new Color(1f,0f,0f), Vector3.right * 14f, 1.2f, OHarePlugin.characterSinging[2]),
                // Bully
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "bully_final"), new Color(1f, 0.6357f, 0f), Vector3.left * 14f, -0.8f, OHarePlugin.characterSinging[3]),
                // Sweep
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "Gotta Sweep Sprite"), new Color(0f, 0.6226f, 0.0614f), (Vector3.left * 13f) + (Vector3.back * 12f), -0.8f, OHarePlugin.characterSinging[4]),
                // Cloudy Copter
                CreateDummyCharacter(___dancingBaldi, sprites.Find(x => x.name == "CloudyCopter"), Color.white, (Vector3.right * 13f) + (Vector3.back * 12f), -0.8f, OHarePlugin.characterSinging[5])
            };



            __instance.StartCoroutine(ComeOn(___dancingBaldi, sprit, aud, characters, ___endingError, ___blackScreen));

            return false;
        }

        static IEnumerator FlipBaB(SpriteRenderer spr, int times, float length)
        {
            for (int i = 0; i < times; i++)
            {
                if (spr.sprite == OHarePlugin.ohareSprite3)
                {
                    spr.sprite = OHarePlugin.ohareSprite4;
                }
                else
                {
                    spr.sprite = OHarePlugin.ohareSprite3;
                }
                yield return new WaitForSeconds(length / (float)times);
            }
            spr.sprite = OHarePlugin.ohareSprite3;
            yield break;
        }


        static IEnumerator ComeOn(BaldiDance b, SpriteRenderer spr, AudioManager audMan, List<VictoryCharacter> characters, GameObject endingError, GameObject blackScreen)
        {
            spr.sprite = OHarePlugin.ohareSprite1;
            yield return new WaitForSeconds(1f);
            audMan.PlaySingle(OHarePlugin.ohare_ISay);
            yield return new WaitForSeconds(3f);
            b.StartCoroutine(FlipBaB(spr, 3, 2f));
            audMan.PlaySingle(OHarePlugin.ohare_whoWithMe);
            yield return new WaitForSeconds(2f);
            VictoryCharacter principal = characters.First();
            principal.Rise();
            yield return new WaitForSeconds(1.1f);
            spr.sprite = OHarePlugin.ohareSprite4;
            principal.audMan.PlaySingle(OHarePlugin.priDirt);
            yield return new WaitForSeconds(2f);
            spr.sprite = OHarePlugin.ohareSprite3;
            b.StartCoroutine(FlipBaB(spr, 6, 3f)); //might as well force this to do this because i dont feel like adding another reference.
            foreach (VictoryCharacter c in characters)
            {
                c.Rise();
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(2.5f);
            Singleton<MusicManager>.Instance.PlayMidi(OHarePlugin.MidiID, false);
            for (int i = 0; i < characters.Count; i++)
            {
                VictoryCharacter c = characters[i];
                if (i != 0) //dont move principal we need him
                {
                    c.MoveTo(new Vector3(b.transform.position.x, c.transform.position.y, b.transform.position.z), 60f); //wont last a full minute so you wont realize they are all heading towards him
                }
                c.Sing();
            }
            yield return new WaitForSeconds(8f);
            principal.MoveRelative(Vector3.right * 4f, 0.3f);
            yield return new WaitForSeconds(0.31f);
            Shader.SetGlobalInt("_ColorGlitching", 1);
            Shader.SetGlobalFloat("_ColorGlitchVal", UnityEngine.Random.Range(0f, 4096f));
            Shader.SetGlobalFloat("_ColorGlitchPercent", 1f);
            Shader.SetGlobalInt("_SpriteColorGlitching", 1);
            Shader.SetGlobalFloat("_SpriteColorGlitchVal", UnityEngine.Random.Range(0f, 1f));
            Shader.SetGlobalFloat("_SpriteColorGlitchPercent", 1f);
            float time = 2f + UnityEngine.Random.Range(0f,1f);
            float swapTime = 0.5f;
            b.glitching = true;
            while (time > 0f)
            {
                time -= Time.deltaTime;
                swapTime -= Time.deltaTime;
                if (swapTime <= 0f)
                {
                    if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
                    {
                        Shader.SetGlobalFloat("_ColorGlitchVal", UnityEngine.Random.Range(0f, 1f));
                        Shader.SetGlobalFloat("_SpriteColorGlitchVal", UnityEngine.Random.Range(0f, 1f));
                    }
                    swapTime = 0.5f;
                }
                audMan.PlaySingle(OHarePlugin.ohare_Agony);
                yield return null;
            }
            if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
            {
                Shader.SetGlobalInt("_SpriteColorGlitching", 0);
            }
            characters.Add(CreateDummyCharacter(b, OHarePlugin.ohareSprite1, new Color(0f, 153f / 255f, 236 / 255f), Vector3.zero, 0f, OHarePlugin.ohare_Agony)); //because im lazy
            characters.Do(x => x.CrashSound());
            endingError.transform.Find("BG").gameObject.GetComponent<Image>().sprite = OHarePlugin.BadHare;
            endingError.SetActive(true);
            Singleton<MusicManager>.Instance.StopMidi();
            while (!Input.anyKeyDown && !Singleton<InputManager>.Instance.GetDigitalInput("MouseSubmit", true) && !Singleton<InputManager>.Instance.GetDigitalInput("Pause", true) && !Singleton<InputManager>.Instance.AnyButton(true))
            {
                yield return null;
            }
            endingError.SetActive(false);
            blackScreen.SetActive(true);
            b.glitching = false;
            characters.Do(x => x.ShutUp());
            Singleton<CoreGameManager>.Instance.Quit();
            yield break;
        }
    }
}
