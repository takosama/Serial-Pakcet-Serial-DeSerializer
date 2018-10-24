using System;
public class C
{
   	public static void Main()
    {
		byte []sendData={114,51,191,91,81};
      
		Encoder encoder=new Encoder();
		encoder.Reset(sendData);
		
   	}
}


public class Encoder
{
	const byte HEAD_BYTE = 0x7E;
	const byte ESCAPE_BYTE = 0x7D;
	const byte ESCAPE_MASK = 0x20;
	byte[] data=new byte[0];
	uint crcNum=0;
	CRC32 crc=new CRC32();
	
	enum State
	{
		end,
		header,
		footer,
		encode,
		escape,
	}
	State state=State.end;
	public void Reset(byte[] data)
	{
		this.data=data;
		crcNum=crc.Compute(data);
		
		Console.WriteLine("crc32=");
		Console.WriteLine(crcNum);
		
		state=State.header;
		pos=0;
	}
	
	int pos=0;
	public byte GetNext()
	{
		if(state==State.header)
		{
			state=State.encode;
			return HEAD_BYTE;
		}
		else
		throw new IndexOutOfRangeException();
	}
}

public class CRC32
{
    private uint[] Table=new uint[256];
	public CRC32()
    {
          for (uint i = 0; i < 256; i++)
        {
            var x = i;
            for (var j = 0; j < 8; j++)
            {
                x = (uint)((x & 1) == 0 ? x >> 1 : -306674912 ^ x >> 1);
            }
            Table[i] = x;
        }
    }

    public uint Compute(byte[] buf)
    {
        uint num = uint.MaxValue;
        for (var i = 0; i < buf.Length; i++)
        {
            num = Table[(num ^ buf[i]) & 255] ^ num >> 8;
        }

        return (uint)(num ^ -1);
    }
}
