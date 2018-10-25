using System;
using System.Linq;
public class C
{
    public static void Main()
    {
        while (true)
        {
            var sendData = Console.ReadLine().ToCharArray().Select(x => (byte)x).ToArray();
            View(sendData);
        }
    }

    private static void View(byte[] sendData)
    {
        Encoder encoder = new Encoder();
        encoder.Reset(sendData);

        foreach (var n in sendData)
            Console.Write(n + " ");
        Console.WriteLine();

        while (encoder.IsHaveNext())
            Console.Write(encoder.GetData() + " ");

        Console.WriteLine();
    }
}


public class Encoder
{
   public  const byte HEAD_BYTE = 0x7E;
   public  const byte ESCAPE_BYTE = 0x7D;
   public  const byte ESCAPE_MASK = 0x20;
    byte[] data = new byte[0];
    uint crcNum = 0;
    CRC32 crc = new CRC32();

    enum State
    {
        end,
        header,
        crc,
        footer,
        encode,
        escape,
    }
    State state = State.end;
    public void Reset(byte[] data)
    {
        this.data = data;
        crcNum = crc.Compute(data);

        Console.WriteLine("crc32=");
        Console.WriteLine(crcNum);

        state = State.header;
        pos = 0;
        crcpos = 0;
    }

    int crcpos = 0;
    int pos = 0;
    public bool IsHaveNext()
    {
        if (state == State.end)
            return false;
        else
            return true;
    }

    public byte GetData()
    {
        if (state == State.header)
        {
            state = State.encode;
            return HEAD_BYTE;
        }
        else if (state == State.end)
            throw new Exception();
        else if(state==State.footer)
        {
            state = State.end;
            return HEAD_BYTE;
        }
        else if(state==State.encode)
        {
            if (data.Length == pos)
            {
                state = State.crc;

                return GetData();
            }
            if (data[pos]==ESCAPE_BYTE || data[pos] == HEAD_BYTE)
            {
                state = State.escape;
                return ESCAPE_BYTE;
            }
            else
            {
                var tmp = data[pos];
                pos++;
                if (data.Length == pos)
                    state = State.crc;
                return tmp;
            }
        }
        else if(state==State.escape)
        {
            byte tmp =(byte)( data[pos] ^ ESCAPE_MASK);
            pos++;
            if (data.Length == pos)
                state = State.crc;
            else
                state = State.encode;
            return tmp;
        }
        else if(state==State.crc)
        {
            byte tmp=(byte)((this.crcNum>>((3-crcpos)*8))&0xff);
            crcpos++;
            if (crcpos >= 4)
                state = State.footer;
            return tmp;
        }
        else 
            throw new IndexOutOfRangeException();
    }
}

public class CRC32
{
    private uint[] Table = new uint[256];
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
