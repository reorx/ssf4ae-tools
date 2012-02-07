﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using RainbowLib.BCM;

namespace RainbowLib
{

    public class BCMFile
    {
        ObservableCollection<Charge> _Charges = new ObservableCollection<Charge>();

        public ObservableCollection<Charge> Charges
        {
            get { return _Charges; }
        }
        ObservableCollection<InputMotion> _InputMotions = new ObservableCollection<InputMotion>();
        public ObservableCollection<InputMotion> InputMotions
        {
            get { return _InputMotions; }
        }
        ObservableCollection<Move> _Moves = new ObservableCollection<Move>();
        public ObservableCollection<Move> Moves
        {
            get { return _Moves; }
        }
        ObservableCollection<CancelList> _CancelLists = new ObservableCollection<CancelList>();
        public ObservableCollection<CancelList> CancelLists
        {
            get { return _CancelLists; }
        }
        public static void ToFilename(string filename, BCMFile bcm)
        {
            using (var ms = new MemoryStream())
            using (var inFile = new BinaryWriter(ms))
            {
                //Header
                inFile.Write(1296253475);
                inFile.Write(2686974);
                inFile.Write(65537);
                inFile.Write(0);
                inFile.Write((ushort)bcm.Charges.Count());
                inFile.Write((ushort)(bcm.InputMotions.Count() - 1));
                inFile.Write((ushort)bcm.Moves.Count());
                inFile.Write((ushort)bcm.CancelLists.Count());
                //TODO WRITE OFFSETS
                int offsets = (int)inFile.BaseStream.Position;
                inFile.Write(0);
                inFile.Write(0);

                inFile.Write(0);
                inFile.Write(0);

                inFile.Write(0);
                inFile.Write(0);

                inFile.Write(0);
                inFile.Write(0);
                int ChargeOffset = (int)inFile.BaseStream.Position;
                if (bcm.Charges.Count == 0)
                    ChargeOffset = 0;
                foreach (Charge c in bcm.Charges)
                {
                    inFile.Write((ushort)c.Input);
                    inFile.Write(c.Unknown1);
                    inFile.Write((ushort)c.MoveFlags);
                    inFile.Write(c.Frames);
                    inFile.Write(c.Unknown3);
                    inFile.Write(c.StorageIndex);
                }
                int ChargeNamesOffset = (int)inFile.BaseStream.Position;
                if (bcm.Charges.Count == 0)
                    ChargeNamesOffset = 0;
                foreach (Charge c in bcm.Charges)
                    inFile.Write(0);
                int InputOffset = (int)inFile.BaseStream.Position;
                foreach (InputMotion i in bcm.InputMotions)
                {
                    if (i == InputMotion.NONE)
                        continue;
                    inFile.Write(i.Entries.Count());
                    foreach (InputMotionEntry entry in i.Entries)
                    {
                        inFile.Write((ushort)entry.Type);
                        inFile.Write((ushort)entry.Buffer);
                        //if (entry.Type == InputType.CHARGE)
                        //    inFile.Write((ushort)bcm.Charges.IndexOf(entry.Charge));
                        inFile.Write((ushort)entry.Input);
                        inFile.Write((ushort)entry.MoveFlags);
                        inFile.Write((ushort)entry.Flags);
                        inFile.Write((ushort)entry.Requirement);
                    }
                    for (int j = i.Entries.Count; j < 16; j++)
                    {
                        inFile.Write(0);
                        inFile.Write(0);
                        inFile.Write(0);
                    }
                }
                int InputNamesOffset = (int)inFile.BaseStream.Position;
                foreach (InputMotion c in bcm.InputMotions)
                {
                    if (c == InputMotion.NONE)
                        continue;
                    inFile.Write(0);
                }
                int MoveOffset = (int)inFile.BaseStream.Position;
                foreach (Move m in bcm.Moves)
                {
                    inFile.Write((ushort)m.Input);
                    inFile.Write((ushort)m.MoveFlags);
                    inFile.Write((ushort)m.PositionRestriction);
                    inFile.Write((ushort)m.Restriction);
                    inFile.Write((uint)m.StateRestriction);
                    inFile.Write((ulong)m.UltraRestriction);
                    inFile.Write(m.PositionRestrictionDistance);
                    inFile.Write((short)m.EXRequirement);
                    inFile.Write((short)m.EXCost);
                    inFile.Write((short)m.UltraRequirement);
                    inFile.Write((short)m.UltraCost);
                    inFile.Write(bcm.InputMotions.IndexOf(m.InputMotion) - 1);
                    inFile.Write(m.Script.Index);

                    /* AI data */
                    inFile.Write((uint)m.Attributes);
                    inFile.Write(m.CpuMinRange);
                    inFile.Write(m.CpuMaxRange);
                    inFile.Write(m.Unk2);
                    inFile.Write(m.Unk3);
                    inFile.Write(m.CpuPassiveMove);
                    inFile.Write(m.CpuCounterMove);
                    inFile.Write(m.CpuVsStand);
                    inFile.Write(m.CpuVsCrouch);
                    inFile.Write(m.CpuVsAir);
                    inFile.Write(m.CpuVsDown);
                    inFile.Write(m.CpuVsStunned);
                    inFile.Write(m.CpuProbeMove);
                    inFile.Write(m.CpuVsVeryClose);
                    inFile.Write(m.CpuVsClose);
                    inFile.Write(m.CpuVsMidRange);
                    inFile.Write(m.CpuVsFar);
                    inFile.Write(m.CpuVsVeryFar);

                }
                int MovesNameOffset = (int)inFile.BaseStream.Position;
                foreach (Move m in bcm.Moves)
                    inFile.Write(0);
                int CancelListOffset = (int)inFile.BaseStream.Position;
                int off = bcm.CancelLists.Count*12;
                foreach (CancelList cl in bcm.CancelLists)
                {
                    inFile.Write(cl.Moves.Count);
                    inFile.Write(off);
                    off = off - 8 + cl.Moves.Count*2;
                }
                int CancelListNameOffset = (int)inFile.BaseStream.Position;
                foreach (CancelList cl in bcm.CancelLists)
                    inFile.Write(0);
                foreach (CancelList cl in bcm.CancelLists)
                {
                    foreach (Move m in cl.Moves)
                    {
                        inFile.Write((short)bcm.Moves.IndexOf(m));
                    }
                }
                //Names Time!
                List<string> strings = new List<string>();
                foreach (Charge tmp in bcm.Charges)
                    strings.Add(tmp.Name);
                Util.writeStringTable(inFile, ChargeNamesOffset, strings);

                strings.Clear();
                foreach (InputMotion tmp in bcm.InputMotions)
                {
                    if (tmp == InputMotion.NONE)
                        continue;
                    strings.Add(tmp.Name);
                }
                Util.writeStringTable(inFile, InputNamesOffset, strings);

                strings.Clear();
                foreach (Move tmp in bcm.Moves)
                    strings.Add(tmp.Name);
                Util.writeStringTable(inFile, MovesNameOffset, strings);

                strings.Clear();
                foreach (CancelList tmp in bcm.CancelLists)
                    strings.Add(tmp.Name);
                Util.writeStringTable(inFile, CancelListNameOffset, strings);

                inFile.Seek((int)offsets, SeekOrigin.Begin);
                inFile.Write(ChargeOffset);
                inFile.Write(ChargeNamesOffset);

                inFile.Write(InputOffset);
                inFile.Write(InputNamesOffset);

                inFile.Write(MoveOffset);
                inFile.Write(MovesNameOffset);

                inFile.Write(CancelListOffset);
                inFile.Write(CancelListNameOffset);

                using (var fs = File.Create(filename))
                {
                    ms.WriteTo(fs);
                }
            }
        }

        public static BCMFile FromFilename(string filename)
        {
            using (var fs = File.OpenRead(filename))
            using (var tracker = new TrackingStream(fs))
            using (var inFile = new BinaryReader(tracker))
            {
                AELogger.Log(AELogger.O_SEPARATOR, false);
                var bcm = new BCMFile();
                tracker.SetLabel("Header");
                var s = new string(inFile.ReadChars(4));
                //Console.WriteLine("{0} - {1:X} bytes",s,inFile.BaseStream.Length);
                if (s != "#BCM")
                    throw new Exception("This is not a valid BCM File!");
                tracker.IgnoreBytes(12);
                inFile.BaseStream.Seek(16);

                var ChargeCount = inFile.ReadUInt16();
                var InputCount = inFile.ReadUInt16();
                var MoveCount = inFile.ReadUInt16();
                var CancelListCount = inFile.ReadUInt16();
                for (int i = 0; i < CancelListCount; i++)
                    bcm.CancelLists.Add(new CancelList());
                var ChargeOffset = inFile.ReadUInt32();
                var ChargeNamesOffset = inFile.ReadUInt32();

                var InputOffset = inFile.ReadUInt32();
                var InputNamesOffset = inFile.ReadUInt32();

                var MoveOffset = inFile.ReadUInt32();
                var MoveNamesOffset = inFile.ReadUInt32();

                var CancelListOffset = inFile.ReadUInt32();
                var CancelListNamesOffset = inFile.ReadUInt32();

                #region Read Charges
                AELogger.Log("Header done, reading charges");
                tracker.SetLabel("Charges");
                for (int i = 0; i < ChargeCount; i++)
                {
                    var charge = new Charge();
                    inFile.BaseStream.Seek(ChargeNamesOffset + i*4);
                    inFile.BaseStream.Seek(inFile.ReadUInt32());
                    charge.Name = inFile.ReadCString();
                    inFile.BaseStream.Seek(ChargeOffset + i*16);
                    charge.Input = (Input)inFile.ReadUInt16();
                    /*if (!Input.IsDefined(typeof(Input), charge.Input))
                    {
                        AELogger.Log("undefined Input enum value: "
                            + charge.Input + " in charge named " + charge.Name);
                    }*/
                    if (Char.IsDigit(charge.Input.ToString()[0])
                        && charge.Input.ToString()[0] != '0')
                    {
                        AELogger.Log("undefined Input enum value: "
                            + charge.Input + " in charge named " + charge.Name);
                    }
                    charge.Unknown1 = inFile.ReadUInt16();
                    charge.MoveFlags = (MoveFlags)inFile.ReadUInt16();
                    if (Char.IsDigit(charge.MoveFlags.ToString()[0])
                        && charge.Input.ToString()[0] != '0')
                    {
                        AELogger.Log("undefined Moveflags enum value: "
                            + charge.MoveFlags + " in charge named " + charge.Name);
                    }
                    charge.Frames = inFile.ReadUInt32();
                    charge.Unknown3 = inFile.ReadUInt16();
                    charge.StorageIndex = inFile.ReadUInt32();
                    //Console.WriteLine(charge);
                    bcm.Charges.Add(charge);
                }

                #endregion

                #region Read Inputs
                AELogger.Log("charges done, reading motions");
                tracker.SetLabel("Inputs");
                bcm.InputMotions.Add(InputMotion.NONE);
                for (int i = 0; i < InputCount; i++)
                {
                    var inputMotion = new InputMotion("tmp");
                    inFile.BaseStream.Seek(InputNamesOffset + i*4);
                    inFile.BaseStream.Seek(inFile.ReadUInt32());
                    inputMotion.Name = inFile.ReadCString();
                    //Console.WriteLine(inputMotion.Name);

                    inFile.BaseStream.Seek(InputOffset + i*0xC4);
                    var cnt = inFile.ReadUInt32();
                    for (int j = 0; j < cnt; j++)
                    {
                        var entry = new InputMotionEntry();
                        entry.Type = (InputType)inFile.ReadUInt16();
                        if (!InputType.IsDefined(typeof(InputType), entry.Type))
                        {
                            AELogger.Log("undefined InputType enum value: "
                                + entry.Type + " in motion named " + inputMotion.Name + " index " + j);
                        }
                        entry.Buffer = inFile.ReadUInt16();
                        System.UInt16 a = inFile.ReadUInt16();
                        if (entry.Type == InputType.CHARGE)
                            entry.Charge = bcm.Charges[a];
                        entry.Input = (Input)a;
                        if (Char.IsDigit(entry.Input.ToString()[0])
                            && entry.Input.ToString()[0] != '0')
                        {
                            AELogger.Log("undefined Input enum value: "
                                + entry.Input + " in motion named " + inputMotion.Name + " index " + j);
                        }
                        entry.MoveFlags = (MoveFlags)inFile.ReadUInt16();
                        if (Char.IsDigit(entry.MoveFlags.ToString()[0])
                            && entry.MoveFlags.ToString()[0] != '0')
                        {
                            AELogger.Log("undefined MoveFlags enum value: "
                                + entry.MoveFlags + " in motion named " + inputMotion.Name + " index " + j);
                        }
                        entry.Flags = (InputReqType)inFile.ReadUInt16();
                        if (!InputReqType.IsDefined(typeof(InputReqType), entry.Flags))
                        {
                            AELogger.Log("undefined InputReqType enum value: "
                                + entry.Flags + " in motion named " + inputMotion.Name + " index " + j);
                        }
                        entry.Requirement = inFile.ReadUInt16();
                        inputMotion.Entries.Add(entry);
                        //Console.WriteLine(entry);

                    }
                    tracker.IgnoreBytes(12*(16 - cnt));
                    bcm.InputMotions.Add(inputMotion);
                }

                #endregion

                #region Read Moves
                AELogger.Log("motions done, reading moves");
                tracker.SetLabel("Moves");
                for (int i = 0; i < MoveCount; i++)
                {
                    var move = new Move();
                    inFile.BaseStream.Seek(MoveNamesOffset + i*4);
                    inFile.BaseStream.Seek(inFile.ReadUInt32());
                    move.Name = inFile.ReadCString();
                    //Console.WriteLine(move.Name);

                    inFile.BaseStream.Seek(MoveOffset + i*0x54);

                    move.Input = (Input)inFile.ReadUInt16();
                    if (Char.IsDigit(move.Input.ToString()[0])
                            && move.Input.ToString()[0] != '0')
                    {
                        AELogger.Log("undefined Input enum value: "
                            + move.Input + " in move named " + move.Name);
                    }
                    move.MoveFlags = (MoveFlags)inFile.ReadUInt16();
                    if (Char.IsDigit(move.MoveFlags.ToString()[0])
                            && move.MoveFlags.ToString()[0] != '0')
                    {
                        AELogger.Log("undefined MoveFlags enum value: "
                            + move.MoveFlags + " in move named " + move.Name);
                    }
                    move.PositionRestriction = (PositionRestriction)inFile.ReadUInt16();
                    move.Restriction = (MoveRestriction)inFile.ReadUInt16();
                    move.StateRestriction = (Move.MoveStateRestriction)inFile.ReadUInt32();
                    move.UltraRestriction = (Move.MoveUltraRestriction)inFile.ReadUInt64();
                    move.PositionRestrictionDistance = inFile.ReadSingle();
                    move.EXRequirement = inFile.ReadInt16();
                    move.EXCost = inFile.ReadInt16();
                    move.UltraRequirement = inFile.ReadInt16();
                    move.UltraCost = inFile.ReadInt16();
                    var index = inFile.ReadInt32();
                    if (index != -1 && index < bcm.InputMotions.Count)
                        move.InputMotion = bcm.InputMotions[index + 1];
                    else
                        move.InputMotion = InputMotion.NONE;
                    move.ScriptIndex = inFile.ReadInt32();

                    /* AI data */
                    move.Attributes = (MoveAttributeFlags)inFile.ReadUInt32();
                    move.CpuMinRange = inFile.ReadSingle();
                    move.CpuMaxRange = inFile.ReadSingle();
                    move.Unk2 = inFile.ReadUInt32();
                    move.Unk3 = inFile.ReadUInt16();
                    move.CpuPassiveMove = inFile.ReadUInt16();
                    move.CpuCounterMove = inFile.ReadUInt16();
                    move.CpuVsStand = inFile.ReadUInt16();
                    move.CpuVsCrouch = inFile.ReadUInt16();
                    move.CpuVsAir = inFile.ReadUInt16();
                    move.CpuVsDown = inFile.ReadUInt16();
                    move.CpuVsStunned = inFile.ReadUInt16();
                    move.CpuProbeMove = inFile.ReadUInt16();
                    move.CpuVsVeryClose = inFile.ReadUInt16();
                    move.CpuVsClose = inFile.ReadUInt16();
                    move.CpuVsMidRange = inFile.ReadUInt16();
                    move.CpuVsFar = inFile.ReadUInt16();
                    move.CpuVsVeryFar = inFile.ReadUInt16();

                    bcm.Moves.Add(move);
                }

                #endregion

                #region ReadCancels
                AELogger.Log("moves done, reading cancels");
                tracker.SetLabel("Cancels");
                for (int i = 0; i < CancelListCount; i++)
                {
                    var cl = bcm.CancelLists[i];
                    inFile.BaseStream.Seek(CancelListNamesOffset + i*4);
                    inFile.BaseStream.Seek(inFile.ReadUInt32());
                    cl.Name = inFile.ReadCString();
                    //Console.WriteLine(cl.Name);

                    inFile.BaseStream.Seek(CancelListOffset + i*0x8);
                    var count = inFile.ReadUInt32();
                    var off = inFile.ReadUInt32();
                    inFile.BaseStream.Seek(off - 8, SeekOrigin.Current);
                    for (int j = 0; j < count; j++)
                    {
                        var x = inFile.ReadInt16();
                        //Console.WriteLine(x);
                        if (x != -1)
                            cl.Moves.Add(bcm.Moves[x]);
                        else cl.Moves.Add(Move.NULL);
                    }
                }
                AELogger.Log("cancels done");
                #endregion

                ////Console.WriteLine(tracker.Report());
                AELogger.Log(AELogger.O_SEPARATOR, false);
                return bcm;
            }
        }
        private BCMFile() { }
    }
}