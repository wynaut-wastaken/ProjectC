using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Text;

namespace ProjectC.sound
{
    class SoundManager
    {
        public static SoundEffect SfxCoinPickup;
        public static SoundEffectInstance CoinPickupInst;
        public static Stack<SoundEffectInstance> CoinSfxStack = new Stack<SoundEffectInstance>();

        public static void LoadSfx()
        {
            SfxCoinPickup = MainGame.Instance.Content.Load<SoundEffect>("coin_pickup");
        }

        public static void PlaySound(SoundEffect sound)
        {
            CoinPickupInst = SfxCoinPickup.CreateInstance();
            if (sound == SfxCoinPickup)
            {
                if (CoinSfxStack.Count > 5)
                {
                    CoinPickupInst.Volume = Math.Max(0.3f - (CoinSfxStack.Count * 0.001f), 0);
                    if (CoinPickupInst.Volume > 0.02f)
                    {
                        CoinSfxStack.Push(CoinPickupInst);
                    }
                    return;
                }
                else
                {
                    CoinPickupInst.Volume = 0.3f;
                    CoinSfxStack.Push(CoinPickupInst);
                    return;
                }
            }
            else sound.Play();
        }

        public static void PopCoins()
        {
            if (CoinSfxStack.Count <= 0) return;
            if (CoinPickupInst.State == SoundState.Playing) return;
            CoinPickupInst = CoinSfxStack.Pop();
            if (CoinPickupInst.State == SoundState.Playing)
            {
                CoinSfxStack.Push(CoinPickupInst);
                return;
            }
            CoinPickupInst.Play();
        }
    }
}
