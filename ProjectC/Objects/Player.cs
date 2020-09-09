﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectC.Engine.Objects;
using ProjectC.Engine.View;
using ProjectC.Engine.World;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ProjectC.Objects
{
    public class Player : GameObject
    {
        public Player()
        {
            origin = new Vector2(8,8);
        }
        
        private int _oldScroll = 0;
        
        public override void step()
        {
            sprite = Sprites.Square;
            
            var w = Keyboard.GetState().IsKeyDown(Keys.W) ? 1 : 0;
            var a = Keyboard.GetState().IsKeyDown(Keys.A) ? 1 : 0;
            var s = Keyboard.GetState().IsKeyDown(Keys.S) ? 1 : 0;
            var d = Keyboard.GetState().IsKeyDown(Keys.D) ? 1 : 0;
            position += new Vector2(d - a, s - w);
            var scroll = _oldScroll - Mouse.GetState().ScrollWheelValue;
            _oldScroll = Mouse.GetState().ScrollWheelValue;
            if (scroll > 0)
            {
                Camera.zoom *= 0.9f;
            }
            if (scroll < 0)
            {
                Camera.zoom /= 0.9f;
            }

            var click = Mouse.GetState().LeftButton.Equals(ButtonState.Pressed);
            if (click)
            {
                var pos = Mouse.GetState().Position.ToVector2();
                pos = Vector2.Transform(pos,Matrix.Invert(Camera.CameraMatrix));
                var clampedpos = pos;
                clampedpos -= position;
                var len = Math.Clamp(clampedpos.Length(),8,96);
                clampedpos.Normalize();
                clampedpos *= len;
                new Tile(EnumTiles.Fresh, ChunkedWorld.LoadChunk(new ChunkIdentifier((int)((pos.X / 8) / 256), (int)((pos.Y / 8) / 256))), Tile.SnapToGrid(position + clampedpos));
            }
            
            Camera.zoom = Math.Clamp(Camera.zoom, 0.5f, 4f);
        }
    }
}