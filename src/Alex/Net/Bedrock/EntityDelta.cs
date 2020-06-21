using System;
using Alex.API.Utils;
using log4net;
using MiNET.Net;

namespace Alex.Net.Bedrock
{
  public class EntityDelta : McpeMoveEntityDelta
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(McpeMoveEntityDelta));

    private PlayerLocation Current { get; set; } = new PlayerLocation();
    
    private int _dX;
    private int _dY;
    private int _dZ;
    private int _dPitch;
    private int _dYaw;
    private int _dHeadYaw;

    public bool HasX = false;
    public bool HasY = false;
    public bool HasZ = false;

    public bool HasPitch = false;
    public bool HasYaw = false;
    public bool HasHeadYaw = false;
    
    public EntityDelta()
    {
      this.Id = (byte) 111;
      this.IsMcpe = true;
    }

    protected override void DecodePacket()
    {
      //base.DecodePacket();
      this.Id = this.ReadByte();
      this.runtimeEntityId = this.ReadUnsignedVarLong();
      this.flags = this.ReadUshort(false);
      this.AfterDecode();
    }

    private void AfterDecode()
    {
      Current = new PlayerLocation();
      if (((int) this.flags & 1) != 0)
      {
        this._dX = this.ReadSignedVarInt();
        HasX = true;
      }

      if (((int) this.flags & 2) != 0)
      {
        this._dY = this.ReadSignedVarInt();
        HasY = true;
      }

      if (((int) this.flags & 4) != 0)
      {
        this._dZ = this.ReadSignedVarInt();
        HasZ = true;
      }

      float num = 45f / 32f;
      if (((int) this.flags & 8) != 0)
      {
        Current.Pitch = (float)this.ReadByte() * num;
        HasPitch = true;
      }

      if (((int) this.flags & 16) != 0)
      {
        Current.Yaw = (float) this.ReadByte() * num;
        HasYaw = true;
      }

      if (((int) this.flags & 32) != 0)
      {
        Current.HeadYaw = (float) this.ReadByte() * num;
        HasHeadYaw = true;
      }

      if (((int) this.flags & 64) == 0)
        return;
      
      this.isOnGround = true;
    }

    protected override void ResetPacket()
    {
      base.ResetPacket();
      this.runtimeEntityId = 0L;
      this.flags = (ushort) 0;
    }
    
    public static int ToIntDelta(float current, float prev)
    {
      return BitConverter.SingleToInt32Bits(current) - BitConverter.SingleToInt32Bits(prev);
    }

    public static float FromIntDelta(float prev, int delta)
    {
      return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(prev) + delta);
    }

    public PlayerLocation GetCurrentPosition(PlayerLocation previousPosition)
    {
      if (((int) this.flags & 1) != 0)
        Current.X = FromIntDelta(previousPosition.X, this._dX);
      
      if (((int) this.flags & 2) != 0)
        Current.Y = FromIntDelta(previousPosition.Y, this._dY);
      
      if (((int) this.flags & 4) != 0)
        Current.Z = FromIntDelta(previousPosition.Z, this._dZ);
      
      return Current;
    }
  }
}