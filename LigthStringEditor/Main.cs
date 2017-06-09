using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LigthStringEditor
{
    

    /// <summary>
    /// This is a Very Brute tool, garanted work only with Dies Irae [Steam Version]
    /// </summary>
    public class LightDat {
        private bool LastLengthCheck = false;
        private StructReader Script;
        public LightDat(byte[] Script) => this.Script = new StructReader(new MemoryStream(Script), false, Encoding.Unicode);

        Encoding Encoding = Encoding.Unicode;
        long StringTablePos = 0;
        long OffsetTablePos = 0;
        public string[] Import() {
            try {
                Script.Seek(0, SeekOrigin.Begin);
                if (StringTablePos == 0)
                    StringTablePos = FindStringTablePos();
                else
                    Script.Seek(StringTablePos, SeekOrigin.Begin);

                if (OffsetTablePos == 0)
                    OffsetTablePos = FindOffsetTable();
                else
                    Script.Seek(OffsetTablePos, SeekOrigin.Begin);

                uint Count = Script.ReadUInt32();
                StrEntry[] Entries = new StrEntry[Count];
                for (uint i = 0; i < Entries.LongLength; i++) {
                    Script.ReadStruct(ref Entries[i]);

                }

                string[] Strings = new string[Entries.LongLength];
                for (uint i = 0; i < Strings.LongLength; i++) {
                    Script.Seek(Entries[i].Offset + StringTablePos + 4, SeekOrigin.Begin);
                    List<byte> Buffer = new List<byte>();
                    while (Entries[i].Length-- > 0)
                        Buffer.Add(Script.ReadByte());
                    Strings[i] = Encoding.GetString(Buffer.ToArray());
                }

                return Strings;
            }
            catch (Exception ex) {
                if (LastLengthCheck)
                    throw ex;
                LastLengthCheck = true;
                OffsetTablePos = 0;
                StringTablePos = 0;
                return Import();
            }
        }


        public byte[] Export(string[] Strings) {
            MemoryStream Output = new MemoryStream();
            Script.Seek(0, SeekOrigin.Begin);
            CopyStream(Script.BaseStream, Output, OffsetTablePos);

            MemoryStream StrBuffer = new MemoryStream();
            StructWriter StrWorker = new StructWriter(StrBuffer, false, Encoding.Unicode);

            MemoryStream OffBuffer = new MemoryStream();
            StructWriter OffWorker = new StructWriter(OffBuffer, false, Encoding.Unicode);

            OffWorker.Write((uint)Strings.LongLength);

            for (uint i = 0; i < Strings.LongLength; i++) {
                StrEntry Entry = new StrEntry() {
                    Offset = (uint)StrBuffer.Length,
                    Length = (uint)Strings[i].Length * 2
                };
                OffWorker.WriteStruct(ref Entry);

                StrWorker.Write(Strings[i], StringStyle.UCString);                
            }

            OffWorker.Write((uint)StrBuffer.Length);

            StrBuffer.Position = 0;
            OffBuffer.Position = 0;

            CopyStream(OffBuffer, Output, OffBuffer.Length);
            CopyStream(StrBuffer, Output, StrBuffer.Length);

            StrWorker.Close();
            OffWorker.Close();

            return Output.ToArray();
        }

        private void CopyStream(Stream Input, Stream Output, long Len) {
            long Readed = 0;
            while (Readed < Len) {
                byte[] Buffer = new byte[Readed + 1024 > Len ? Len - Readed : 1024];
                int r = Input.Read(Buffer, 0, Buffer.Length);
                Output.Write(Buffer, 0, Buffer.Length);
                Readed += r;
                if (r == 0)
                    throw new Exception("Failed to Read the Stream");
            }
        }

        private struct StrEntry {
            internal uint Offset;
            internal uint Length;            
        }

        private long FindOffsetTable() {
            while (Script.PeekInt32() != 0)
                Script.Seek(-8, SeekOrigin.Current);
            Script.Seek(-4, SeekOrigin.Current);
            return Script.BaseStream.Position;
        }
        uint LastStringLen = 0;
        private long FindStringTablePos() {
            if (LastLengthCheck) {
                LastStringLen = 2;
                do {
                    LastStringLen += 2;
                    Script.Seek(LastStringLen * -1, SeekOrigin.End);
                } while (Script.PeekInt16() != 0);
                LastStringLen -= 4;

                do {
                    Script.Seek(-5, SeekOrigin.Current);//-1 bytes per loop, because the condition skip 4 every time.
                } while (Script.ReadInt32() != LastStringLen || Script.PeekInt32() != StrTblLen);

            } else {
                Script.Seek(-4, SeekOrigin.End);
                while (Script.PeekInt32() != StrTblLen) {
                    Script.Seek(-2, SeekOrigin.Current);
                }
            }
            return Script.BaseStream.Position;
        }

        private long StrTblLen => Script.BaseStream.Length - Script.BaseStream.Position - 4;
    }
}
