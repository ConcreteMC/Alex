namespace Alex.Net.Bedrock.Types
{
	public class NetSkin
	{
		/// <summary>
		/// SkinID is a unique ID produced for the skin, for example 'c18e65aa-7b21-4637-9b63-8ad63622ef01_Alex' for the default Alex skin.
		/// </summary>
		public string SkinId            { get; set; }
		
		/// <summary>
		/// SkinResourcePatch is a JSON encoded object holding some fields that point to the geometry that the skin has.
		/// The JSON object that this holds specifies the way that the geometry of animations and the default skin of the player are combined.
		/// </summary>
		public byte[] SkinResourcePatch { get; set; }
		
		public uint   SkinImageWidth    { get; set; }
		
		public uint   SkinImageHeight   { get; set; }
		
		/// <summary>
		/// 	SkinData is a byte slice of SkinImageWidth * SkinImageHeight bytes. It is an RGBA ordered byte representation of the skin pixels.
		/// </summary>
		public byte[] SkinData          { get; set; }
		
		/// <summary>
		/// 	Animations is a list of all animations that the skin has
		/// </summary>
		public SkinAnimation[] Animations { get; set; }
		
		public uint   CapeImageWidth  { get; set; }
		public uint   CapeImageHeight { get; set; }
		public byte[] CapeData        { get; set; }
		
		/// <summary>
		/// 	SkinGeometry is a JSON encoded structure of the geometry data of a skin, containing properties
		/// 	such as bones, uv, pivot etc.
		/// </summary>
		public byte[] SkinGeometry { get; set; }
		
		/// <summary>
		/// 	
		/// </summary>
		public byte[] AnimationData { get; set; }
		
		/// <summary>
		/// PremiumSkin specifies if this is a skin that was purchased from the marketplace.
		/// </summary>
		public bool PremiumSkin { get; set; }
		
		/// <summary>
		/// 	PersonaSkin specifies if this is a skin that was created using the in-game skin creator.
		/// </summary>
		public bool PersonaSkin { get; set; }
		
		/// <summary>
		/// Secifies if the skin had a Persona cape (in-game skin creator cape) equipped on a classic skin.
		/// </summary>
		public bool PersonaCapeOnClassicSkin { get; set; }
		
		/// <summary>
		/// 	CapeID is a unique identifier that identifies the cape. It usually holds a UUID in it.
		/// </summary>
		public string CapeId { get; set; }
		
		/// <summary>
		/// 	FullSkinID is an ID that represents the skin in full. The actual functionality is unknown: The client does not seem to send a value for this.
		/// </summary>
		public string FullSkinId { get; set; }
		
		/// <summary>
		/// 	SkinColour is a hex representation (including #) of the base colour of the skin. An example of the colour sent here is '#b37b62'.
		/// </summary>
		public string SkinColour { get; set; }
		
		/// <summary>
		/// 	ArmSize is the size of the arms of the player's model. This is either 'wide' (generally for male skins) or 'slim' (generally for female skins).
		/// </summary>
		public string ArmSize { get; set; }
		
		/// <summary>
		/// 	A list of all persona pieces that the skin is composed of.
		/// </summary>
		public PersonaPiece[] PersonaPieces { get; set; }
		
		/// <summary>
		/// 	PieceTintColours is a list of specific tint colours for (some of) the persona pieces found in the list above.
		/// </summary>
		public PersonaPieceTintColour PieceTintColours { get; set; }
		
		/// <summary>
		/// 	Trusted specifies if the skin is 'trusted'. No code should rely on this field, as any proxy or client can easily change it.
		/// </summary>
		public bool Trusted { get; set; }
	}

	public enum SkinAnimationType
	{
		SkinAnimationHead = 1,
		SkinAnimationBody32x32 = 2,
		SkinAnimationBody128x128 = 3,
	}
	
	/// <summary>
	/// SkinAnimation represents an animation that may be added to a skin. The client plays the animation itself,
	/// without the server having to do so.
	/// The rate at which these animations play appears to be decided by the client.
	/// </summary>
	public class SkinAnimation
	{
		public uint ImageWidth  { get; set; }
		public uint ImageHeight { get; set; }
		
		/// <summary>
		/// ImageData is a byte slice of ImageWidth * ImageHeight bytes. It is an RGBA ordered byte representation
		/// of the animation image pixels. The ImageData contains FrameCount images in it, which each represent one
		/// stage of the animation. The actual part of the skin that this field holds depends on the AnimationType,
		/// where SkinAnimationHead holds only the head and its hat, whereas the other animations hold the entire
		/// body of the skin.
		/// </summary>
		public byte[] ImageData     { get; set; }
		
		/// <summary>
		/// 	AnimationType is the type of the animation, which is one of the types found above. 
		/// </summary>
		public SkinAnimationType AnimationType { get; set; }
		
		/// <summary>
		/// 	FrameCount is the amount of frames that the skin animation holds. The number of frames here is the amount of images that may be found in the ImageData field.
		/// </summary>
		public float  FrameCount    { get; set; }
	}

	public class PersonaPiece
	{
		/// <summary>
		///		PieceId is a UUID that identifies the piece itself, which is unique for each separate piece.
		/// </summary>
		public string PieceId { get; set; }
		
		/// <summary>
		///	PieceType holds the type of the piece. Several types I was able to find immediately are listed below.
		/// - persona_skeleton
		/// - persona_body
		/// - persona_skin
		/// - persona_bottom
		/// - persona_feet
		/// - persona_top
		/// - persona_mouth
		/// - persona_hair
		/// - persona_eyes
		/// - persona_facial_hair
		/// </summary>
		public string PieceType { get; set; }
		
		/// <summary>
		/// 	PackID is a UUID that identifies the pack that the persona piece belongs to.
		/// </summary>
		public string PackId { get; set; }
		
		/// <summary>
		/// 	Default specifies if the piece is one of the default pieces. This is true when the piece is one of those that a Steve or Alex skin have.
		/// </summary>
		public bool Default { get; set; }
		
		/// <summary>
		/// ProductID is a UUID that identifies the piece when it comes to purchases. It is empty for pieces that
		/// have the 'Default' field set to true. 
		/// </summary>
		public string ProductId { get; set; }
	}

	public class PersonaPieceTintColour
	{
		/// <summary>
		///	PieceType is the type of the persona skin piece that this tint colour concerns. The piece type must
		/// always be present in the persona pieces list, but not each piece type has a tint colour sent.
		/// Pieces that do have a tint colour that I was able to find immediately are listed below.
		/// - persona_mouth
		/// - persona_eyes
		/// - persona_hair
		/// </summary>
		public string PieceType { get; set; }
		
		/// <summary>
		/// Colours is a list four colours written in hex notation (note, that unlike the SkinColour field in
		/// the ClientData struct, this is actually ARGB, not just RGB).
		/// The colours refer to different parts of the skin piece. The 'persona_eyes' may have the following
		/// colours: ["#ffa12722","#ff2f1f0f","#ff3aafd9","#0"]
		/// The first hex colour represents the tint colour of the iris, the second hex colour represents the
		/// eyebrows and the third represents the sclera. The fourth is #0 because there are only 3 parts of the
		/// persona_eyes skin piece.
		/// </summary>
		public string[] Colours { get; set; }
	}
}