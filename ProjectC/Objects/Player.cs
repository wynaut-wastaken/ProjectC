﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectC.world;
using ProjectC.view;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using SharpDX.DirectWrite;

namespace ProjectC.objects
{
    public class Player : Entity
    {
        public bool FacingRight = true;
        public static Player LocalClient;

        public Vector2 CamOffset = Vector2.One * 0.5f;
        public float CollisionIncrement = 128;
        private float inc { get => CollisionIncrement; }

        public float WalkSpeed
        {
            get => _walkSpeed / TileHelper.TileSize;
            set => _walkSpeed = value;
        }
        public float FallSpeed
        {
            get => _fallSpeed / TileHelper.TileSize;
            set => _fallSpeed = value;
        }
        public float MaxYSpeed = 8;
        public float JumpHeight = 0.6f;

        private float _fallSpeed = 0.2f;
        private float _walkSpeed = 1.5f;
        private bool _jumpedYet = false;
        private bool _onground = false;

        public Dimension CurrentDimension = Dimension.Current;
        public Chunk ChunkIn = Dimension.VoidChunk;
        public Point ChunkPos = Point.Zero;
        public Vector2 Velocity = Vector2.Zero;
        public int[,] Hotbar = new int[10, Chunk.TileDepth];
        public int HotbarSlot;

        public Rectangle Hitbox;

        public Player()
        {
            Dimension.LoadGameObject(this);
            position = new Vector2(512.5f * Chunk.ChunkWidth,1.5f * Chunk.ChunkHeight);
            LocalClient = this;
            origin = new Vector2(8,24);
            for(var i = 0; i <= Hotbar.GetUpperBound(0); i++)
            {
                Hotbar[i, Chunk.ChunkTData] = i;
                Hotbar[i, Chunk.ChunkTColor] = (int)Color.White.PackedValue;
            }
            Hitbox = new Rectangle(position.ToPoint(), new Point(1, 3));
        }
        
        private int _oldScroll;

        public static float Lerp(float value1, float value2, float amount)
        {
            var d = (value2 - value1) * amount;
            return value1 + d;
        }

        public bool Collides(Vector2 pos)
        {
            var hitTile = Dimension.Current.TileAtWorldPos(pos - Vector2.UnitY * 2, Chunk.ChunkTData, true);
            var hasHit = TileHelper.HasHitbox(hitTile);
            if (!hasHit)
            {
                hitTile = Dimension.Current.TileAtWorldPos(pos - Vector2.UnitY, Chunk.ChunkTData, true);
                hasHit = TileHelper.HasHitbox(hitTile);
            }
            if (!hasHit)
            {
                hitTile = Dimension.Current.TileAtWorldPos(pos, Chunk.ChunkTData, true);
                hasHit = TileHelper.HasHitbox(hitTile);
            }
            return hasHit;
        }

        public void Collide()
        {
            if (Collides(position)) return;

            var prevPos = position;

            if (Collides(position + Velocity.X * Vector2.UnitX))
            {
                if(!Collides(position + Velocity.X * Vector2.UnitX - Vector2.UnitY) && _onground) {
                    position.Y -= 1;
                }
                else
                {
                    Velocity.X = 0;
                }
            }
            position.X += Velocity.X;
            if (Collides(position + Velocity.Y * Vector2.UnitY))
            {
                while(!Collides(position + Math.Sign(Velocity.Y)*Vector2.UnitY/inc))
                {
                    position.Y += Math.Sign(Velocity.Y) / inc;
                }
                Velocity.Y = 0;
            }
            position.Y += Velocity.Y;
            if(!Collides(position+Vector2.UnitY) && Collides(position + Vector2.UnitY*2) && _onground && !_jumpedYet)
            {
                position += Vector2.UnitY;
            }
        }

        public void LoadNearbyChunks()
        {
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitY - Vector2.UnitX * 2).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitY - Vector2.UnitX).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitY).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitY + Vector2.UnitX).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitY + Vector2.UnitX * 2).ToPoint(), true);

            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitX * 2).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition - Vector2.UnitX).ToPoint(), true);
            //already loaded middle chunk
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitX).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitX * 2).ToPoint(), true);

            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitY - Vector2.UnitX * 2).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitY - Vector2.UnitX).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitY).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitY + Vector2.UnitX).ToPoint(), true);
            Dimension.Current.LoadChunk((ChunkIn.ChunkspacePosition + Vector2.UnitY + Vector2.UnitX * 2).ToPoint(), true);
        }

        public override void step()
        {
            position = Vector2.Max(position, Vector2.Zero);
            ChunkIn = Dimension.Current.ChunkAtWorldPos(position, true);
            ChunkPos = ChunkIn.WorldToChunk(position);
            LoadNearbyChunks();
            
            sprite = Sprites.PlayerHuman;

            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            var a = keyState.IsKeyDown(Keys.A) ? 1 : 0;
            var d = keyState.IsKeyDown(Keys.D) ? 1 : 0;
            _onground = Collides(position + Vector2.UnitY);
            if(_onground && position.Y != Math.Round(position.Y))
            {
                position.Y -= (position.Y - (float)Math.Floor(position.Y));
            }
            Velocity += new Vector2((d - a) * WalkSpeed, _onground ? 0 : FallSpeed);
            if(keyState.IsKeyDown(Keys.Space) && _onground && !_jumpedYet)
            {
                _onground = false;
                Velocity.Y = -JumpHeight;
                _jumpedYet = true;
            }
            if(keyState.IsKeyUp(Keys.Space))
            {
                _jumpedYet = false;
            }
            Velocity.Y = Math.Clamp(Velocity.Y, -MaxYSpeed, _onground ? 0 : MaxYSpeed);
            Velocity.X = Lerp(Math.Clamp(Velocity.X, -WalkSpeed * 2, WalkSpeed * 2), 0, 0.25f);

            Collide();
            
            var scroll = _oldScroll - mouseState.ScrollWheelValue;
            _oldScroll = mouseState.ScrollWheelValue;
            if (scroll > 0)
            {
                Camera.zoom *= 0.9f;
            }

            if ((d - a) < 0)
            {
                FacingRight = false;
            }  
            if ((d - a) > 0)
            {
                FacingRight = true;
            }  

            if (scroll < 0)
            {
                Camera.zoom /= 0.9f;
            }

            Camera.Position = position * TileHelper.TileSize;
            var click = mouseState.LeftButton == ButtonState.Pressed;

            for (var i = 0; i <= Hotbar.GetUpperBound(0); i++)
            {
                var input = keyState.IsKeyDown(Keys.D0 + i);
                if (input)
                {
                    HotbarSlot = i - 1;
                    if (HotbarSlot < 0) HotbarSlot = Hotbar.GetUpperBound(0);
                }
            }

            if(mouseState.RightButton == ButtonState.Pressed)
            {
               
                new Coin(GetPlacingPosition(mouseState.Position.ToVector2()), new Random().Next(0,2));
            }

            if (click)
            {
                TryPlaceTile(mouseState.Position.ToVector2(), Hotbar[HotbarSlot, Chunk.ChunkTData], Hotbar[HotbarSlot, Chunk.ChunkTSide], Hotbar[HotbarSlot, Chunk.ChunkTColor], Hotbar[HotbarSlot, Chunk.ChunkTMeta]);
            }
            var save = keyState.IsKeyDown(Keys.K);
            if (save)
            {
                CurrentDimension.Save();
            }
            Camera.zoom = Math.Clamp(Camera.zoom, 0.8f, 4f);
        }

        public Vector2 GetPlacingPosition(Vector2 worldPos)
        {
            var playerpos = WorldPosition;
            var clampedpos = Vector2.Transform(worldPos, Matrix.Invert(Camera.CameraMatrix));
            clampedpos -= playerpos;
            var len = Math.Clamp(clampedpos.Length(), 8, 96);
            clampedpos.Normalize();
            clampedpos *= len;
            var npos = playerpos + clampedpos;
            npos /= TileHelper.TileSize;
            npos = Vector2.Round(npos);
            return npos;
        }

        public bool TryPlaceTile(Vector2 worldPos, int type, int side, int color, int meta)
        {
            var npos = GetPlacingPosition(worldPos);
            var placed = false;
            if (npos.X >= 0 && npos.Y >= 0)
            {
                var chunk = CurrentDimension.ChunkAtWorldPos(npos, true);
                var chunkpos = chunk.WorldToChunk(npos);
                placed = TileHelper.TryMakeTile(type, side, color, meta, chunk, chunkpos);
            }
            return placed;
        }

        public override void draw(SpriteBatch _spriteBatch)
        {
            if (sprite != null)
            {
                var facingflip = FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(sprite, position*TileHelper.TileSize, null, Color.White, 0, origin, Vector2.One, facingflip, 0);
            }
        }

        public void DrawGui(SpriteBatch batch)
        {
            for (var i = 0; i < Hotbar.GetUpperBound(0); i++)
            {
                var pos = Camera.Position + new Vector2(-128, 0) + new Vector2(i * 32, 128);
                batch.Draw(Sprites.HotbarPart, pos - new Vector2(12, 12), Color.White);
                batch.Draw(TileHelper.SpriteFrom(Hotbar[i, Chunk.ChunkTData]), pos, new Rectangle(0, 0, TileHelper.TileSize, TileHelper.TileSize), Color.White);
            }
        }
    }
}