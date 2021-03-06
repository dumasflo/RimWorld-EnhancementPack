﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
	public static class MouseoverOnTopRight
	{
		//public void MouseoverReadoutOnGUI()
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instList = instructions.ToList();
			for (int i = 0; i < instList.Count; i++)
			{
				CodeInstruction inst = instList[i];

				//Topright winter shadow
				if (inst.opcode == OpCodes.Call && inst.operand == AccessTools.Method(typeof(GenUI), "DrawTextWinterShadow"))
					inst.operand = AccessTools.Method(typeof(MouseoverOnTopRight), nameof(DrawTextWinterShadowTR));

				//Transform Widgets.Label rect
				if (inst.opcode == OpCodes.Call && inst.operand == AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string) }))
					inst.operand = AccessTools.Method(typeof(MouseoverOnTopRight), nameof(LabelTransform));
			
				yield return inst;

				if (inst.opcode == OpCodes.Callvirt && inst.operand == AccessTools.Property(typeof(MainTabsRoot), "OpenTab").GetGetMethod())
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MouseoverOnTopRight), nameof(FilterForOpenTab)));// 0 != null is false
				}
			}
		}
		

		public static void DrawTextWinterShadowTR(Rect badRect)
		{
			if (Settings.Get().mouseoverInfoTopRight)
				GenUI.DrawTextWinterShadow(new Rect(UI.screenWidth-256f, 256f, 256f, -256f));
			else
				GenUI.DrawTextWinterShadow(badRect);
		}

		public static void LabelTransform(Rect rect, string label)
		{
			if (Settings.Get().mouseoverInfoTopRight)
			{
				//rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				rect.x = UI.screenWidth - rect.x; //flip x
				rect.y = UI.screenHeight - rect.y - 50f; //flip y, adjust for maintabs margin: BotLeft.y = 65f, BotLeft.x = 15f
				rect.x -= Text.CalcSize(label).x;//adjust for text width
			}
			Widgets.Label(rect, label);
		}

		public static MainButtonDef FilterForOpenTab(MainButtonDef def)
		{
			return Settings.Get().mouseoverInfoTopRight ? null : def;
		}

	}
}
