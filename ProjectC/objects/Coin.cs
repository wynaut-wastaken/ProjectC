using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ProjectC.sound;
using ProjectC.view;
using ProjectC.world;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace ProjectC.objects
{
    class Coin : GameObject
    {
        const int frameSize = 7;
        public float frameIndex = 0;
        const int frameCount = 6;
        public int coinType = 0;
        public static float FallSpeed = 0.01f;
        public Vector2 Velocity;

        public override void draw(SpriteBatch _spriteBatch)
        {
            frameIndex += 0.2f;
            frameIndex %= frameCount;
            _spriteBatch.Draw(Sprites.CoinsSmall, WorldPosition + origin, new Rectangle((int)frameIndex * frameSize, coinType * frameSize, frameSize, frameSize), Color.White);
        }

        public Coin(Vector2 pos, int type)
        {
            var rand = new Random();
            Velocity.X = rand.NextFloat(-5,5) / 4;
            Velocity.Y = rand.NextFloat(1, 4) / -8;
            var len = Velocity.Length();
            Velocity.Normalize();
            Velocity *= len;
            Dimension.LoadGameObject(this);
            position = pos;
            coinType = type;
        }
        public bool Collides(Vector2 pos)
        {
           var hitTile = Dimension.Current.TileAtWorldPos(pos, Chunk.ChunkTData, true);
            var hasHit = TileHelper.HasHitbox(hitTile);
            return hasHit;
        }

        public override void step()
        {
            Velocity.Y += FallSpeed;
            if(Collides(position + Vector2.UnitY))
            {
                Velocity.Y = 0;
                position.Y = (float)Math.Floor(position.Y);
                Velocity.X /= 1.2f;
            }
            Velocity.X /= 1.1f;
            if (Collides(position + Velocity))
            {
                Velocity = Vector2.Zero;
            }
            position += Velocity;
            if(Vector2.Distance(Player.LocalClient.position,position) <= 3)
            {
                position = Vector2.Lerp(position, Player.LocalClient.position, 0.1f);
            }
            if(Vector2.Distance(Player.LocalClient.position,position) <= 0.3f)
            {
                SoundManager.PlaySound(SoundManager.SfxCoinPickup);
                destroy();
            }
        }
    }
}
