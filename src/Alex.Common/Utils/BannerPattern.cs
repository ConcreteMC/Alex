using System.Runtime.Serialization;
using MiNET.Blocks;
using MiNET.Entities.Hostile;
using MojangAPI;

namespace Alex.Common.Utils;

public enum BannerPattern
{
	[EnumMember(Value = "b")] Base,
	[EnumMember(Value = "bs")] BottomStripe,
	[EnumMember(Value = "ts")] TopStripe,
	[EnumMember(Value = "ls")] LeftStripe,
	[EnumMember(Value = "rs")] RightStripe,
	[EnumMember(Value = "cs")] CenterStripeVertical,
	[EnumMember(Value = "ms")] MiddleStripeHorizontal,
	[EnumMember(Value = "drs")] DownRightStripe,
	[EnumMember(Value = "dls")] DownLeftStripe,
	[EnumMember(Value = "ss")] SmallVerticalStripes,
	[EnumMember(Value = "cr")] DiagonalCross,
	[EnumMember(Value = "sc")] SquareCross,
	[EnumMember(Value = "ld")] LeftDiagonal,
	[EnumMember(Value = "rud")] RightUpsideDownDiagonal,
	[EnumMember(Value = "lud")] LeftUpsideDownDiagonal,
	[EnumMember(Value = "rd")] RightDiagonal,
	[EnumMember(Value = "vh")] VerticalHalfLeft,
	[EnumMember(Value = "vhr")] VerticalHalfRight,
	[EnumMember(Value = "hh")] HorizontalHalfTop,
	[EnumMember(Value = "hhb")] HorizontalHalfBottom,
	[EnumMember(Value = "bl")] BottomLeftCorner,
	[EnumMember(Value = "br")] BottomRightCorner,
	[EnumMember(Value = "tl")] TopLeftCorner,
	[EnumMember(Value = "tr")] TopRightCorner,
	[EnumMember(Value = "bt")] BottomTriangle,
	[EnumMember(Value = "tt")] TopTriangle,
	[EnumMember(Value = "bts")] BottomTriangleSawtooth,
	[EnumMember(Value = "tts")] TopTriangleSawtooth,
	[EnumMember(Value = "mc")] MiddleCircle,
	[EnumMember(Value = "mr")] MiddleRhombus,
	[EnumMember(Value = "bo")] Border,
	[EnumMember(Value = "cbo")] CurlyBorder,
	[EnumMember(Value = "bri")] Brick,
	[EnumMember(Value = "gra")] Gradient,
	[EnumMember(Value = "gru")] GradientUpsideDown,
	[EnumMember(Value = "cre")] Creeper,
	[EnumMember(Value = "sku")] Skull,
	[EnumMember(Value = "flo")] Flower,
	[EnumMember(Value = "moj")] Mojang,
	[EnumMember(Value = "glb")] Globe,
	[EnumMember(Value = "pig")] Piglin,
}