using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using UnityEngine;
using static Rewired.Data.Mapping.HardwareJoystickMap;

namespace OHareBasic
{


    [BepInPlugin("mtm101.rulerp.baldiplus.ohare", "O'Hare Over Baldi", "1.0.0.0")]
    [BepInIncompatibility("sakyce.baldiplus.teachersadditions")]
    public class OHarePlugin : BaseUnityPlugin
    {
        public static OHarePlugin Instance;

        public static Sprite airSpriteSmall;
        public static Sprite airSpriteLarge;
        public static Texture2D OHarePoster;
        public static Sprite BadHare;

        public static Sprite ohareSprite1;
        public static Sprite ohareSprite2;
        public static Sprite ohareSprite3;
        public static Sprite ohareSprite4;
        public static Sprite ohareSprite5;
        public static SoundObject ohare_ISay;
        public static SoundObject ohare_Agony;
        public static SoundObject ohare_letItDie;
        public static SoundObject ohare_letItDieFunky;
        public static SoundObject ohare_whoWithMe;
        public static SoundObject ohare_clap;
        public static SoundObject airWind;
        public static SoundObject priDirt;

        public static string MidiID;

        public static List<SoundObject> characterSinging = new List<SoundObject>();

        public static SubtitleTimedKey[] SingKeys = new SubtitleTimedKey[]
        {
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow1",
                time = 0f,
                encrypted = false
            },
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow2",
                time = 3f,
                encrypted = false
            },
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow3",
                time = 6f,
                encrypted = false
            },
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow4",
                time = 8.5f,
                encrypted = false
            },
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow5",
                time = 10.7f,
                encrypted = false
            },
            new SubtitleTimedKey()
            {
                key = "Vfx_LetItGrow6",
                time = 13.5f,
                encrypted = false
            }
        };

        public static SceneObject endObject;

        void AddSing(string chara)
        {
            SoundObject obj = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "let_it_grow_" + chara + ".wav"), "Vfx_LetItGrow1", SoundType.Voice, Color.white);
            if (chara != "cloudy")
            {
                obj.additionalKeys = SingKeys;
            }
            else
            {
                obj.soundKey = "Vfx_Cmlo_Blowing"; //lol
            }
            characterSinging.Add(obj);
        }


        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.ohare");
            ohareSprite1 = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ohare1.png"), Vector2.one / 2, 27);
            ohareSprite2 = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ohare2.png"), Vector2.one / 2, 27);
            ohareSprite3 = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ohare3.png"), Vector2.one / 2, 27);
            ohareSprite4 = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ohare4.png"), Vector2.one / 2, 27);
            ohareSprite5 = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ohare5.png"), Vector2.one / 2, 27);
            OHarePoster = AssetLoader.TextureFromMod(this, "pri_ohare.png");
            BadHare = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "oharedead.png"));
            ohare_Agony = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "agony.wav"), "OHare_Agony", SoundType.Voice, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_ISay = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "i_say.wav"), "OHare_ISay", SoundType.Voice, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_letItDie = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "let_it_die.wav"), "OHare_LetItDie", SoundType.Voice, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_letItDieFunky = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "let_it_die_funky.wav"), "OHare_LetItDie", SoundType.Voice, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_whoWithMe = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "whos_with_me.wav"), "OHare_WhoWithMe", SoundType.Voice, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_clap = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "clap.wav"), "OHare_Clap", SoundType.Effect, new Color(0f, 153f / 255f, 236 / 255f));
            ohare_clap.subDuration = 0.35f;
            airWind = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "windblow.wav"), "Sfx_OhareWind", SoundType.Effect, new Color(1f,1f,1f));

            priDirt = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "pri_nodirtbag.wav"), "Vfx_PRI_NoDirtbag", SoundType.Voice, Color.white);

            airSpriteSmall = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "AirIcon_Small.png"), Vector2.one / 2, 25);
            airSpriteLarge = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "AirIcon_Large.png"), Vector2.one / 2, 50);

            AddSing("principal");
            AddSing("bald");
            AddSing("play");
            AddSing("bully");
            AddSing("sweep");
            AddSing("cloudy");


            harmony.PatchAllConditionals();
            Debug.Log("OHARE MODE ACTIVATE");
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class NameAwakePatch
    {
        static void Prefix()
        {
            ItemObject apple = Resources.FindObjectsOfTypeAll<ItemObject>().Where(x => x.itemType == Items.Apple).First();
            apple.itemSpriteLarge = OHarePlugin.airSpriteLarge;
            apple.itemSpriteSmall = OHarePlugin.airSpriteSmall;
            Resources.FindObjectsOfTypeAll<PosterObject>().Where(x => x.name == "BaldiPoster").First().baseTexture = OHarePlugin.OHarePoster;
            OHarePlugin.MidiID = AssetLoader.MidiFromFile(Path.Combine(AssetLoader.GetModPath(OHarePlugin.Instance), "loraxchords.mid"), "letitgrow");
            OHarePlugin.endObject = Resources.FindObjectsOfTypeAll<SceneObject>().Where(x => x.levelTitle == "YAY").First();
        }
    }

    [ConditionalPatchNever]
    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadLevel")]
    class EternallyStuck
    {
        static void Prefix(ref SceneObject sceneObject)
        {
            sceneObject = OHarePlugin.endObject;
        }
    }
}