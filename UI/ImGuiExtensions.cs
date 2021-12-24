using System;
using System.Numerics;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface;
using Auralyte.Game;
using Auralyte.Configuration;

namespace Auralyte.UI {
    public static class ImGuiEx {

        /// <summary>
        /// Draws a typeahead input. The typeahead input is a text input with a text hint overlaying it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <param name="values"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static bool TypeaheadInput(string id, ref string input, List<string> values, uint bufferSize) {
            // Trim the end of the input to make finishing the input easier.
            input = input.TrimEnd();

            // Get the initial hint.
            string hint = GetHint(input, values);

            // Record the current position and width so we can overlay the hint after.
            Vector2 beforeInput = ImGui.GetCursorPos();
            float width = ImGui.CalcItemWidth();
            bool inputChanged = false;
            if(ImGui.InputText(id, ref input, bufferSize, ImGuiInputTextFlags.AllowTabInput)) {
                inputChanged = true;

                // Trim the end of the input to make finishing the input easier (again).
                bool takeHint = false;

                // If the user has hit tab, take note.
                if(input.EndsWith("\t")) {
                    takeHint = true;
                }

                // Get the latest hint.
                input = input.TrimEnd();
                hint = GetHint(input, values);

                if(takeHint && hint.Length >= input.Length) {
                    input = hint;
                    ImGui.SetWindowFocus(null);
                }
            }

            // Overlay the hint.
            ImGui.SetCursorPos(beforeInput + new Vector2(4, 3));
            hint = CorrectHint(hint, input);
            ImGui.Text(hint);
            ImGui.SetCursorPos(beforeInput);
            ImGui.Dummy(new(width, ImGui.GetTextLineHeight() + 6));

            return inputChanged;
        }

        /// <summary>
        /// Gets the matching hint from an array of values using for the provided input.
        /// </summary>
        /// <param name="input">Input to find the hint for.</param>
        /// <param name="values">List of values to hint towards.</param>
        /// <returns></returns>
        private static string GetHint(string input, List<string> values) {
            // If the string is empty, return an empty string.
            if(input.Trim().Length == 0) {
                return "";
            }

            // Find the first match in the list of values and return it or an empty string if no match was found.
            string value = values.Find((value) => { return value.ToLower().StartsWith(input.ToLower()); });

            if(value == null) {
                value = "";
            }

            return value;
        }

        /// <summary>
        /// Corrects the hint for the provided input, matching the casing where appropriate.<br/>
        /// This method is required to get around ImGui inputs being very possessive of their data.
        /// </summary>
        /// <param name="hint">Hint to correct.</param>
        /// <param name="input">Input to correct towards.</param>
        /// <returns></returns>
        private static string CorrectHint(string hint, string input) {
            if(hint == input || hint.Length < input.Length) {
                return hint;
            }

            return input + hint.Substring(input.Length);
        }

        public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None) {
            if(ImGui.IsItemHovered(flags))
                ImGui.SetTooltip(s);
        }

        // Why is this not a basic feature of ImGui...
        private static readonly Stack<float> fontScaleStack = new();
        private static float curScale = 1;
        public static void PushFontScale(float scale) {
            fontScaleStack.Push(curScale);
            curScale = scale;
            ImGui.SetWindowFontScale(curScale);
        }

        public static void PopFontScale() {
            curScale = fontScaleStack.Pop();
            ImGui.SetWindowFontScale(curScale);
        }

        public static void PushFontSize(float size) => PushFontScale(size / ImGui.GetFont().FontSize);

        public static void PopFontSize() => PopFontScale();

        public static float GetFontScale() => curScale;

        public static void ClampWindowPosToViewport() {
            var viewport = ImGui.GetWindowViewport();
            if(ImGui.IsWindowAppearing() || viewport.ID != ImGuiHelpers.MainViewport.ID)
                return;

            var pos = viewport.Pos;
            ClampWindowPos(pos, pos + viewport.Size);
        }

        public static void ClampWindowPos(Vector2 max) => ClampWindowPos(Vector2.Zero, max);

        public static void ClampWindowPos(Vector2 min, Vector2 max) {
            var pos = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            var x = Math.Min(Math.Max(pos.X, min.X), max.X - size.X);
            var y = Math.Min(Math.Max(pos.Y, min.Y), max.Y - size.Y);
            ImGui.SetWindowPos(new Vector2(x, y));
        }

        public static bool IsWindowInMainViewport() => ImGui.GetWindowViewport().ID == ImGuiHelpers.MainViewport.ID;

        public static bool ShouldDrawInViewport() => IsWindowInMainViewport() || Client.IsGameFocused;

        public static void ShouldDrawInViewport(out bool b) => b = ShouldDrawInViewport();

        public static bool SetBoolOnGameFocus(ref bool b) {
            if(!b)
                b = Client.IsGameFocused;
            return b;
        }

        public static string TryGetClipboardText() {
            try { return ImGui.GetClipboardText(); } catch { return string.Empty; }
        }

        private static bool sliderEnabled = false;
        private static bool sliderVertical = false;
        private static float sliderInterval = 0;
        private static int lastHitInterval = 0;
        private static Action<bool, bool, bool> sliderAction;
        public static void SetupSlider(bool vertical, float interval, Action<bool, bool, bool> action) {
            sliderEnabled = true;
            sliderVertical = vertical;
            sliderInterval = interval;
            lastHitInterval = 0;
            sliderAction = action;
        }

        public static void DoSlider() {
            if(!sliderEnabled)
                return;

            // You can blame ImGui for this
            var popupOpen = !ImGui.IsPopupOpen("_SLIDER") && ImGui.IsPopupOpen(null, ImGuiPopupFlags.AnyPopup);
            if(!popupOpen) {
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGui.SetNextWindowPos(new Vector2(-100));
                ImGui.OpenPopup("_SLIDER", ImGuiPopupFlags.NoOpenOverItems);
                if(!ImGui.BeginPopup("_SLIDER"))
                    return;
            }

            var drag = sliderVertical ? ImGui.GetMouseDragDelta().Y : ImGui.GetMouseDragDelta().X;
            var dragInterval = (int)(drag / sliderInterval);
            var hit = false;
            var increment = false;
            if(dragInterval > lastHitInterval) {
                hit = true;
                increment = true;
            } else if(dragInterval < lastHitInterval)
                hit = true;

            var closing = !ImGui.IsMouseDown(ImGuiMouseButton.Left);

            if(lastHitInterval != dragInterval) {
                while(lastHitInterval != dragInterval) {
                    lastHitInterval += increment ? 1 : -1;
                    sliderAction(hit, increment, closing && lastHitInterval == dragInterval);
                }
            } else
                sliderAction(false, false, closing);

            if(closing)
                sliderEnabled = false;

            if(!popupOpen)
                ImGui.EndPopup();
        }

        // ?????????
        public static void PushClipRectFullScreen() => ImGui.GetWindowDrawList().PushClipRectFullScreen();

        public static void TextCopyable(string text) {
            ImGui.TextUnformatted(text);

            if(!ImGui.IsItemHovered())
                return;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if(ImGui.IsItemClicked())
                ImGui.SetClipboardText(text);
        }

        public static Vector2 RotateVector(Vector2 v, float a) {
            var aCos = (float)Math.Cos(a);
            var aSin = (float)Math.Sin(a);
            return RotateVector(v, aCos, aSin);
        }

        public static Vector2 RotateVector(Vector2 v, float aCos, float aSin) => new(v.X * aCos - v.Y * aSin, v.X * aSin + v.Y * aCos);

        public static void IncrementAnimationTimer() {
            animationTimer += 1;
        }

        public class AuraSettings {
            public float size = 30;
            public float zoom = 1;
            public Vector2 offset = Vector2.Zero;
            public double rotation = 0;
            public int iconId = -1;
            public bool flipped = false;
            public bool hovered = false;
            public float activeTime = -1;
            public bool frame = false;
            public float timer = -1;
            public float timerMax = -1;
            public int timerType = -1;
            public bool timerReverse = false;
            public int charges = -1;
            public int glowType = -1;
            public int spinnerType = -1;
            public int greyscaleType = -1;
            public int darkenType = -1;
        }

        private static readonly uint ICON_COLOUR = 0xFFFFFFFF;
        private static readonly uint ICON_COLOUR_DARK = 0xFF999999;
        private static readonly uint ICON_COLOUR_DARKER = 0xFF666666;
        private static readonly uint ICON_COLOUR_DARKEST = 0xFF333333;

        public static void DrawAura(this ImDrawListPtr drawList, Vector2 position, AuraSettings settings) {
            var texture = settings.greyscaleType == (int)Property.GreyscaleType.Off ? TextureDictionary.textureDictionaryHR[settings.iconId] : TextureDictionary.textureDictionaryGSHR[settings.iconId];

            if(texture == null) {
                return;
            }
            
            // Position
            var area = new VectorArea(
                position,
                settings.size
            );

            drawList.DrawIcon(texture, area, settings);
            drawList.DrawAuraDecals(area, settings);
        }

        public static void DrawIcon(this ImDrawListPtr drawList, ImGuiScene.TextureWrap texture, VectorArea area, AuraSettings settings) {
            // Texture Crop
            float zoom = 0.5f / settings.zoom;
            var textureTopLeft = new Vector2(0.5f - zoom + settings.offset.X, 0.5f - zoom + settings.offset.Y);
            var textureBottomRight = new Vector2(0.5f + zoom + settings.offset.X, 0.5f + zoom + settings.offset.Y);
            var rCos = (float)Math.Cos(settings.rotation);
            var rSin = (float)-Math.Sin(settings.rotation);
            var uvHalfSize = (textureBottomRight - textureTopLeft) / 2;
            var uvCenter = textureTopLeft + uvHalfSize;
            textureTopLeft = uvCenter + RotateVector(-uvHalfSize, rCos, rSin);
            textureBottomRight = uvCenter + RotateVector(uvHalfSize, rCos, rSin);

            uint colour = ICON_COLOUR;

            switch((Property.DarkenType)settings.darkenType) {
                case Property.DarkenType.Dark:
                    colour = ICON_COLOUR_DARK;
                    break;
                case Property.DarkenType.Darker:
                    colour = ICON_COLOUR_DARKER;
                    break;
                case Property.DarkenType.Darkest:
                    colour = ICON_COLOUR_DARKEST;
                    break;
            }

            drawList.DrawImage(texture, area, new VectorArea(textureTopLeft, textureBottomRight.X - textureTopLeft.X), colour);
        }

        public static void DrawImage(this ImDrawListPtr drawList, ImGuiScene.TextureWrap texture, VectorArea position, VectorArea texturePosition) {
            drawList.DrawImage(texture, position, texturePosition, 0xFFFFFFFF);
        }

        public static void DrawImage(this ImDrawListPtr drawList, ImGuiScene.TextureWrap texture, VectorArea position, VectorArea texturePosition, uint colour) {
            drawList.AddImageQuad(texture.ImGuiHandle, position.p1, position.p2, position.p3, position.p4, texturePosition.p1, texturePosition.p2, texturePosition.p3, texturePosition.p4, colour);
        }

        private static readonly VectorArea iconFrameArea = new(new Vector2(1f / 426f, 141f / 426f), 46 / 426f);
        private static readonly VectorArea activeFrameArea = new(new Vector2(97f / 426f, 141f / 426f), 46 / 426f);
        private static readonly float activeFrameGap = 2f / 426f;
        private static readonly float activeFrameSize = 46f / 426f;
        private static int animationTimer = 0;

        public static void DrawAuraDecals(this ImDrawListPtr drawList, VectorArea area, AuraSettings settings) {
            // Calculate the area of the frame.
            var frameSize = area.width * 0.075f;
            var frameArea = new VectorArea(area.position -= new Vector2(frameSize, frameSize), area.width + frameSize * 2);
            bool greyscale = settings.greyscaleType == (int)Property.GreyscaleType.On;

            // Frame
            drawList.DrawAuraFrame(frameArea, greyscale);

            // Cooldown Spinner
            if(settings.timerMax > 0) {
                var cooldownStyle = CooldownStyle.None;
                switch(settings.timerType) {
                    case (int)Property.TimerSubType.Cooldown:
                        cooldownStyle = CooldownStyle.Cooldown;
                        break;
                    case (int)Property.TimerSubType.ChargeCooldown:
                        cooldownStyle = CooldownStyle.Charge;
                        break;
                }
                drawList.DrawAuraCooldownSpinner(frameArea, settings.timer / settings.timerMax, cooldownStyle, greyscale);
            }

            // Glow
            if(settings.glowType == (int)Property.GlowType.On) { 
                drawList.DrawAuraFrameGlow(frameArea, greyscale);
            }

            // Active Spinner
            if(settings.spinnerType != (int)Property.SpinnerType.Off) {
                drawList.DrawAuraActiveSpinner(frameArea, settings.spinnerType == (int)Property.SpinnerType.Reverse, greyscale);
            }

            // Cooldown Text
            if(settings.timerMax > 0) {
                drawList.DrawAuraTimer(frameArea, settings.timer, settings.timerMax);
            }

            // Charge Text
            if(settings.charges >= 0 && settings.timerType == (int)Property.TimerSubType.ChargeCooldown) {
                VectorArea chargeArea = new(frameArea.p3 - new Vector2(16, 16), 18);
                drawList.DrawAuraCharges(chargeArea, settings.charges, greyscale);
            }
        }

        public static void DrawAuraFrame(this ImDrawListPtr drawList, VectorArea position, bool greyscale) {
            // Get a reference to the frame texture.
            var frameSheet = !greyscale ? TextureDictionary.textureDictionaryHR[TextureDictionary.FrameIconID] : TextureDictionary.textureDictionaryGSHR[TextureDictionary.FrameIconID];
            if(frameSheet == null || frameSheet.ImGuiHandle == IntPtr.Zero) {
                return;
            }

            drawList.DrawImage(frameSheet, position, iconFrameArea, 0xFFFFFFFF);
        }

        private const byte maxCooldownPhase = 80;
        private static readonly Vector2 iconCooldownSize = new(44);
        private static readonly Vector2 iconCooldownSection = new(44, 48);
        private static readonly Vector2 iconCooldownSheetSize1 = new(432, 432);
        private static readonly Vector2 iconCooldownSheetSize2 = new(792, 792);
        private static readonly Vector2 iconCooldownUV0Mult1 = iconCooldownSection / iconCooldownSheetSize1;
        private static readonly Vector2 iconCooldownUV0Mult2 = iconCooldownSection / iconCooldownSheetSize2;
        private static readonly Vector2 iconCooldownUV1Add1 = iconCooldownSize / iconCooldownSheetSize1;
        private static readonly Vector2 iconCooldownUV1Add2 = iconCooldownSize / iconCooldownSheetSize2;
        private static readonly Vector2 iconCooldownSheetUVOffset1 = new Vector2(18, -1) / iconCooldownSheetSize1; // Due to squaring
        private static readonly Vector2 iconCooldownSheetUVOffset2 = new Vector2(0, 179) / iconCooldownSheetSize2;

        public enum CooldownStyle {
            None,
            Cooldown,
            Charge,
            GCD,
        }

        public static void DrawAuraCooldownSpinner(this ImDrawListPtr drawList, VectorArea area, float progress, CooldownStyle style, bool greyscale) {
            // Get a reference to the frame texture.
            var textureDictionary = !greyscale ? TextureDictionary.textureDictionaryHR : TextureDictionary.textureDictionaryGSHR;
            var cooldownSheet = style switch {
                CooldownStyle.Cooldown => textureDictionary[TextureDictionary.GetSafeIconID(1)],
                CooldownStyle.Charge => textureDictionary[TextureDictionary.GetSafeIconID(2)],
                CooldownStyle.GCD => textureDictionary[TextureDictionary.GetSafeIconID(2)],
                CooldownStyle.None => null,
                _ => null
            };
            if(cooldownSheet == null || cooldownSheet.ImGuiHandle == IntPtr.Zero) {
                return;
            }

            var phase = (byte)Math.Min(Math.Max(Math.Ceiling(maxCooldownPhase * progress), 0), maxCooldownPhase);
            var row = Math.DivRem(phase, 9, out var column);
            var uv0 = new Vector2(column, row);
            Vector2 uv1;
            switch(style) {
                case CooldownStyle.Cooldown:
                    area.position += Vector2.One;
                    area.width -= 1;
                    uv0 = uv0 * iconCooldownUV0Mult1 + iconCooldownSheetUVOffset1;
                    uv1 = uv0 + iconCooldownUV1Add1;
                    break;
                case CooldownStyle.Charge:
                    uv0 = uv0 * iconCooldownUV0Mult2 + iconCooldownSheetUVOffset2;
                    uv1 = uv0 + iconCooldownUV1Add2;
                    break;
                case CooldownStyle.GCD:
                    uv0 = uv0 * iconCooldownUV0Mult2 + iconCooldownSheetUVOffset2 + new Vector2(0.5f, 0);
                    uv1 = uv0 + iconCooldownUV1Add2;
                    break;
                default:
                    uv0 = uv0 * iconCooldownUV0Mult2 + iconCooldownSheetUVOffset2;
                    uv1 = uv0 + iconCooldownUV1Add2;
                    break;
            }

            drawList.DrawImage(cooldownSheet, area, new VectorArea(uv0, uv1.X - uv0.X));
        }

        private static void DrawAuraTimer(this ImDrawListPtr drawList, VectorArea area, float timer, float timerMax) {
            ImGui.PushFont(Auralyte.Font);

            // Make the font occupy a portion of the icon.
            var wantedSize = area.width * 0.65f;
            var str = $"{Math.Ceiling(timerMax - timer)}";

            PushFontSize(wantedSize);

            var textSizeHalf = ImGui.CalcTextSize(str) / (2 * ImGuiHelpers.GlobalScale);

            // Outline
            var textOutlinePos = area.center - textSizeHalf + new Vector2(wantedSize * 0.04f, 0);
            drawList.AddText(Auralyte.Font, wantedSize, textOutlinePos, 0xFF000000, str);
            textOutlinePos = area.center - textSizeHalf + new Vector2(wantedSize * -0.04f, 0);
            drawList.AddText(Auralyte.Font, wantedSize, textOutlinePos, 0xFF000000, str);
            textOutlinePos = area.center - textSizeHalf + new Vector2(0, wantedSize * -0.08f);
            drawList.AddText(Auralyte.Font, wantedSize, textOutlinePos, 0xFF000000, str);

            var textPos = area.center - textSizeHalf - Vector2.UnitY;
            drawList.AddText(Auralyte.Font, wantedSize, textPos, 0xFFFFFFFF, str);

            PopFontSize();

            ImGui.PopFont();
        }

        private static void DrawAuraCharges(this ImDrawListPtr drawList, VectorArea area, int charges, bool greyscale) {
            if(charges < 1 || charges > 8) {
                return;
            }

            // Get a reference to the frame texture.
            var textureDictionary = !greyscale ? TextureDictionary.textureDictionaryHR : TextureDictionary.textureDictionaryGSHR;
            var texture = textureDictionary[60917 + charges];

            if(texture == null) {
                return;
            }

            drawList.DrawImage(texture, area, new(new(0.25f,0.25f),0.5f), 0xFFFFFFFF);
        }

        public static void DrawAuraActiveSpinner(this ImDrawListPtr drawList, VectorArea area, bool greyscale) {
            drawList.DrawAuraActiveSpinner(area, false, greyscale);
        }
        public static void DrawAuraActiveSpinner(this ImDrawListPtr drawList, VectorArea area, bool reverse, bool greyscale) {
            // Get a reference to the frame texture.
            var frameSheet = !greyscale ? TextureDictionary.textureDictionaryHR[TextureDictionary.FrameIconID] : TextureDictionary.textureDictionaryGSHR[TextureDictionary.FrameIconID];
            if(frameSheet == null || frameSheet.ImGuiHandle == IntPtr.Zero) {
                return;
            }

            float animationLength = 60;
            float animationTime = animationTimer % animationLength;

            int animationFrame = (int)Math.Round(animationTime / 60 * 8);

            if(reverse) {
                animationFrame = 8 - animationFrame;
            }

            if(animationFrame == 8) {
                animationFrame = 0;
            }

            int x = animationFrame % 3;
            int y = animationFrame / 3;

            var drawableFrame = activeFrameArea;

            var xOffset = (activeFrameGap + activeFrameSize) * x;
            var yOffset = (activeFrameGap + activeFrameSize) * y;

            drawableFrame += new Vector2(xOffset, yOffset);

            drawList.DrawImage(frameSheet, area, drawableFrame, 0xFFFFFFFF);
        }


        private static readonly VectorArea iconFrameGlowArea = new(new Vector2(245f / 426f, 145f / 426f), 62 / 426f);
        private static readonly float glowAnimationTime = 240;
        public static void DrawAuraFrameGlow(this ImDrawListPtr drawList, VectorArea area, bool greyscale) {
            // Get a reference to the frame texture.
            var frameSheet = !greyscale ? TextureDictionary.textureDictionaryHR[TextureDictionary.FrameIconID] : TextureDictionary.textureDictionaryGSHR[TextureDictionary.FrameIconID];
            if(frameSheet == null || frameSheet.ImGuiHandle == IntPtr.Zero) {
                return;
            }

            // Clone the area to prevent pollution.
            area = area.Clone();

            // Calculate glow size.
            float glowFrame = (animationTimer % glowAnimationTime);
            if(glowFrame > glowAnimationTime / 2) { 
                glowFrame = (glowAnimationTime / 2) - (glowFrame - (glowAnimationTime/2)); 
            }

            glowFrame /= (glowAnimationTime / 2);
            float glowSize = area.width * (0.15f + (0.05f * glowFrame));

            var glowArea = new VectorArea(area.position -= new Vector2(glowSize, glowSize), area.width + glowSize * 2);

            drawList.DrawImage(frameSheet, glowArea, iconFrameGlowArea, 0xFFFFFFFF);
        }
    }

    public class VectorArea {
        public Vector2 position;
        public float width, height;

        public Vector2 p1 => position;
        public Vector2 p2 => new(position.X + width, position.Y);
        public Vector2 p3 => new(position.X + width, position.Y + width);
        public Vector2 p4 => new(position.X, position.Y + width);
        public Vector2 center => (p1 + p3) / 2;

        public VectorArea(Vector2 position, float size) : this(position, size, size) {
        }

        public VectorArea(Vector2 position, float width, float height) {
            this.position = position;
            this.width = width;
            this.height = height;
        }

        public VectorArea Clone() {
            return new VectorArea(new(position.X, position.Y), width);
        }

        public static VectorArea operator +(VectorArea area, Vector2 positionChange) {
            return new VectorArea(area.position + positionChange, area.width);
        }
    }


}
