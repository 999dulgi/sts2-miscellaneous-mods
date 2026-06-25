using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("ModLoaded")]
public class ModInit
{
	public static void ModLoaded()
	{
		new Harmony("makeblurblur").PatchAll();
	}
}

public static class BlurShader
{
	public static readonly ShaderMaterial Material = new()
	{
		Shader = new Shader()
		{
			Code = """
			shader_type canvas_item;

			uniform sampler2D screen_texture : hint_screen_texture, filter_linear;
			uniform float blur_amount: hint_range(0.0, 10.0) = 1.5;

			void fragment() {
				vec4 color = vec4(0.0);
				float total = 0.0;
				vec2 ps = SCREEN_PIXEL_SIZE * blur_amount;
				for (int x = -3; x <= 3; x++) {
					for (int y = -3; y <= 3; y++) {
						float fx = float(x);
						float fy = float(y);
						float w = exp(-(fx * fx + fy * fy) / 8.0);
						color += texture(screen_texture, SCREEN_UV + vec2(fx, fy) * ps) * w;
						total += w;
					}
				}
				COLOR = color / total;
			}
			"""
		}
	};
}

[HarmonyPatch(
	"MegaCrit.Sts2.Core.Nodes.Cards.Holders.NCardHolder",
	"SetCard")]
public static class NCardHolder_SetCard_Patch
{
	public static void Postfix(object __instance, CanvasItem node)
	{
		var model = node.GetType().GetProperty("Model")?.GetValue(node);
		if (model?.GetType().Name != "Blur") return;

		var cardSize = new Vector2(340f, 462f);

		var backBuffer = new BackBufferCopy()
		{
			CopyMode = BackBufferCopy.CopyModeEnum.Viewport,
		};

		var overlay = new ColorRect()
		{
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Material = BlurShader.Material,
			Position = -cardSize / 2f,
			Size = cardSize,
		};

		node.AddChild(backBuffer);
		node.AddChild(overlay);
	}
}
