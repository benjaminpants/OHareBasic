using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OHareBasic
{
    public class OHare_StupidState : Baldi_Chase
    {
        public MovementModifier mm;
        public ActivityModifier am;
        public NpcState prev;
        float time = 99f;
        public OHare_StupidState(NPC npc, Baldi baldi, NpcState previousState) : base(npc, baldi)
        {
            prev = previousState;
        }

        public override void Initialize()
        {
            base.Initialize();
            mm = new MovementModifier((Directions.RandomDirection.ToVector3() * 12f) + (Directions.RandomDirection.ToVector3() * 12f), 0.1f);
            am = baldi.GetComponent<Entity>().ExternalActivity;
            am.moveMods.Add(mm);
            baldi.AudMan.PlaySingle(OHarePlugin.airWind);
            time = baldi.appleTime;
        }

        public override void OnStateTriggerStay(Collider other)
        {
        }

        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime * baldi.ec.NpcTimeScale;
            if (time > 0f) return;
            baldi.AudMan.FlushQueue(true);
            am.moveMods.Remove(mm);
            baldi.behaviorStateMachine.ChangeState(prev);
        }
    }
}
