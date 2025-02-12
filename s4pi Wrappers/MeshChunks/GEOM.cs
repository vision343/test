/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Peter Jones                                                            *
 *  Keyi Zhang                                                             *
 *  CmarNYC                                                                *
 *  Buzzler                                                                *  
 *                                                                         *
 *  This file is part of the Sims 4 Package Interface (s4pi)               *
 *                                                                         *
 *  s4pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s4pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s4pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System.IO;
using s4pi.Interfaces;
using s4pi.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using s4pi.GenericRCOLResource;
using System.Linq;
using System.Collections;

namespace meshExpImp.ModelBlocks
{
    public class GEOM : ARCOLBlock
    {
        static bool checking = s4pi.Settings.Settings.Checking;

        #region Attributes
        uint tag = (uint)FOURCC("GEOM");
        uint version = 0x0000000E;
        ShaderType shader;
        MTNF mtnf = null;
        uint mergeGroup;
        uint sortOrder;
        VertexFormatList vertexFormats;
        VertexDataList vertexData;
        uint numSubMeshes;
        FaceList faces;
        int skinIndex;
        UVStitchList uvStitchList;
        SeamStitchList seamStitchList;
        SlotrayIntersectionList slotrayIntersectionList;
        UIntList boneHashes;

        TGIBlockList tgiBlockList;
        #endregion

        #region Constructors
        public GEOM(int apiVersion, EventHandler handler) : base(apiVersion, handler, null) { }
        public GEOM(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
        public GEOM(int apiVersion, EventHandler handler, GEOM basis)
            : this(apiVersion, handler,
            basis.version, basis.shader, basis.mtnf, basis.mergeGroup, basis.sortOrder,
            basis.vertexFormats, basis.vertexData,
            basis.faces, basis.skinIndex, basis.uvStitchList, basis.seamStitchList, basis.slotrayIntersectionList, basis.boneHashes,
            basis.tgiBlockList) { }
        public GEOM(int apiVersion, EventHandler handler,
            uint version, ShaderType shader, MTNF mtnf, uint mergeGroup, uint sortOrder,
            IEnumerable<VertexFormat> vertexFormats, IEnumerable<VertexDataElement> vertexData,
            IEnumerable<Face> facePoints, int skinIndex,
            UVStitchList uvStitchList, SeamStitchList seamStitchList, SlotrayIntersectionList slotrayIntersectionList, IEnumerable<uint> boneHashes,
            IEnumerable<TGIBlock> tgiBlockList)
            : base(apiVersion, handler, null)
        {
            this.version = version;
            this.shader = shader;
            if (shader != 0 && mtnf == null)
                throw new ArgumentException("Must supply MTNF when applying a Shader.");
            this.mtnf = shader == 0 ? null : new MTNF(requestedApiVersion, handler, mtnf) { RCOLTag = "GEOM", };
            this.mergeGroup = mergeGroup;
            this.sortOrder = sortOrder;
            this.vertexFormats = vertexFormats == null ? null : new VertexFormatList(handler, version, vertexFormats);
            this.vertexData = vertexData == null ? null : new VertexDataList(handler, version, vertexData, this.vertexFormats);
            this.faces = facePoints == null ? null : new FaceList(handler, facePoints);
            this.skinIndex = skinIndex;
            this.uvStitchList = uvStitchList == null ? null : new UVStitchList(handler, uvStitchList);
            this.seamStitchList = seamStitchList == null ? null : new SeamStitchList(handler, seamStitchList);
            this.slotrayIntersectionList = slotrayIntersectionList == null ? null : new SlotrayIntersectionList(handler, this.version, slotrayIntersectionList);
            this.boneHashes = boneHashes == null ? null : new UIntList(handler, boneHashes);
            this.tgiBlockList = tgiBlockList == null ? null : new TGIBlockList(handler, tgiBlockList);

            if (mtnf != null)
                mtnf.ParentTGIBlocks = this.tgiBlockList;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "GEOM"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x015A1849; } }

        // public override AHandlerElement Clone(EventHandler handler) { return new GEOM(requestedApiVersion, handler, this); }
        #endregion

        #region DataIO
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC("GEOM"))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: 'GEOM'; at 0x{1:X8}", FOURCC(tag), s.Position));
            version = r.ReadUInt32();
            if (checking) if (version != 0x00000005 && version != 0x0000000C && version != 0x0000000D && version != 0x0000000E)
                    throw new InvalidDataException(String.Format("Invalid Version read: '{0}'; expected: '0x00000005', '0x0000000C', '0x0000000D' or '0x0000000E'; at 0x{1:X8}", version, s.Position));

            long tgiPosn = r.ReadUInt32() + s.Position;
            long tgiSize = r.ReadUInt32();

            shader = (ShaderType)r.ReadUInt32();
            if (shader != 0)
            {
                uint size = r.ReadUInt32();
                long posn = s.Position;
                mtnf = new MTNF(requestedApiVersion, handler, s, "GEOM");
                if (checking) if (s.Position != posn + size)
                        throw new InvalidDataException(String.Format("MTNF chunk size invalid; expected 0x{0:X8} bytes, read 0x{1:X8} bytes; at 0x{2:X8}",
                            size, s.Position - posn, s.Position));
            }
            else mtnf = null;

            mergeGroup = r.ReadUInt32();
            sortOrder = r.ReadUInt32();

            int numVertices = r.ReadInt32();//now write that down...
            vertexFormats = new VertexFormatList(handler, version, s);
            vertexData = new VertexDataList(handler, version, s, numVertices, vertexFormats);//...as you'll be needing it

            numSubMeshes = r.ReadUInt32();
            if (checking) if (numSubMeshes != 1)
                    throw new InvalidDataException(String.Format("Expected number of sub meshes to be 1, read {0}, at 0x{1:X8}", numSubMeshes, s.Position));

            byte facePointSize = r.ReadByte();
           // if (checking) if (facePointSize != 2)
           //         throw new InvalidDataException(String.Format("Expected face point size to be 2, read {0}, at 0x{1:X8}", facePointSize, s.Position));

            faces = new FaceList(handler, s);
            if (version == 0x00000005)
            {
                skinIndex = r.ReadInt32();
            }
            else if (version >= 0x0000000C)
            {
                uvStitchList = new UVStitchList(handler, s);
                if (version >= 0x0000000D)
                {
                    seamStitchList = new SeamStitchList(handler, s);
                }
                slotrayIntersectionList = new SlotrayIntersectionList(handler, version, s);
            }
            boneHashes = new UIntList(handler, s);

            tgiBlockList = new TGIBlockList(OnRCOLChanged, s, tgiPosn, tgiSize);
            if (mtnf != null)
                mtnf.ParentTGIBlocks = tgiBlockList;
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            long pos = ms.Position;
            w.Write((uint)0); // tgiOffset
            w.Write((uint)0); // tgiSize

            w.Write((uint)shader);
            if (shader != 0)
            {
                if (mtnf == null) mtnf = new MTNF(requestedApiVersion, handler, "GEOM") { };
                byte[] mtnfData = mtnf.AsBytes;
                w.Write(mtnfData.Length);
                w.Write(mtnfData);
            }

            w.Write(mergeGroup);
            w.Write(sortOrder);

            if (vertexData == null) w.Write(0);
            else w.Write(vertexData.Count);
            if (vertexFormats == null) vertexFormats = new VertexFormatList(handler, version);
            vertexFormats.UnParse(ms);
            if (vertexData == null) vertexData = new VertexDataList(handler, version, vertexFormats);
            vertexData.UnParse(ms);
            w.Write((uint)1);
            w.Write((byte)2);
            if (faces == null) faces = new FaceList(handler);
            faces.UnParse(ms);
            if (version == 0x00000005)
            {
                w.Write(skinIndex);
            }
            else if (version >= 0x0000000C)
            {
                if (uvStitchList == null) uvStitchList = new UVStitchList(handler);
                uvStitchList.UnParse(ms);
                if (version >= 0x0000000D)
                {
                    if (seamStitchList == null) seamStitchList = new SeamStitchList(handler);
                    seamStitchList.UnParse(ms);
                }
                if (slotrayIntersectionList == null) slotrayIntersectionList = new SlotrayIntersectionList(handler, this.version);
                slotrayIntersectionList.UnParse(ms);
            }
            if (boneHashes == null) boneHashes = new UIntList(handler);
            boneHashes.UnParse(ms);

            if (tgiBlockList == null)
            {
                tgiBlockList = new TGIBlockList(OnRCOLChanged);
                if (mtnf != null)
                    mtnf.ParentTGIBlocks = tgiBlockList;
            }
            tgiBlockList.UnParse(ms, pos);

            return ms;
        }

        private byte ReadByte(Stream s) { return new BinaryReader(s).ReadByte(); }
        private void WriteByte(Stream s, byte element) { new BinaryWriter(s).Write(element); }
        #endregion

        #region Sub-Types

        #region VertexFormat
        public enum UsageType : uint
        {
            Position = 0x01,
            Normal = 0x02,
            UV = 0x03,
            BoneAssignment = 0x04,
            Weights = 0x05,
            TangentNormal = 0x06,
            Color = 0x07,
            BiNormal = 0x08,
            WeightMap = 0x09,
            VertexID = 0x0A,
        }
        static uint[] expectedDataType05 = new uint[] {
            /*Unknown*/ 0,
            /*Position*/ 1,
            /*Normal*/ 1,
            /*UV*/ 1,
            /*BoneAssignment*/ 2,
            /*Weights*/ 1, 
            /*TangentNormal*/ 1,
            /*Color*/ 3,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        static byte[] expectedElementSize05 = new byte[] {
            /*Unknown*/ 0,
            /*Position*/ 12,
            /*Normal*/ 12,
            /*UV*/ 8,
            /*BoneAssignment*/ 4,
            /*Weights*/ 16, 
            /*TangentNormal*/ 12,
            /*Color*/ 4,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        static uint[] expectedDataType0C = new uint[] {
            /*Unknown*/ 0,
            /*Position*/ 1,
            /*Normal*/ 1,
            /*UV*/ 1,
            /*BoneAssignment*/ 2,
            /*BoneWeight*/ 2, 
            /*TangentNormal*/ 1,
            /*Color*/ 3,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        static byte[] expectedElementSize0C = new byte[] {
            /*Unknown*/ 0,
            /*Position*/ 12,
            /*Normal*/ 12,
            /*UV*/ 8,
            /*BoneAssignment*/ 4,
            /*BoneWeight*/ 4, 
            /*TangentNormal*/ 12,
            /*Color*/ 4,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        public class VertexFormat : AHandlerElement, IEquatable<VertexFormat>
        {
            const int recommendedApiVersion = 1;

            uint version;
            UsageType usage;
            internal uint dataType;
            internal byte elementSize;

            public VertexFormat(int apiVersion, EventHandler handler, uint version) : base(apiVersion, handler) { this.version = version; }
            public VertexFormat(int apiVersion, EventHandler handler, uint version, Stream s) : base(apiVersion, handler) { this.version = version; Parse(s); }
            public VertexFormat(int apiVersion, EventHandler handler, VertexFormat basis)
                : this(apiVersion, handler, basis.version, basis.usage, basis.dataType, basis.elementSize) { }
            public VertexFormat(int apiVersion, EventHandler handler, uint version, UsageType usage, uint dataType, byte elementSize)
                : base(apiVersion, handler)
            {
                this.version = version;
                this.usage = usage;
                this.dataType = dataType;
                this.elementSize = elementSize;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                usage = (UsageType)r.ReadUInt32();
                dataType = r.ReadUInt32();
                elementSize = r.ReadByte();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)usage);
                w.Write(dataType);
                w.Write(elementSize);
            }

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new VertexFormat(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<VertexFormat>
            public bool Equals(VertexFormat other)
            {
                return this.usage.Equals(other.usage);
            }

            public override bool Equals(object obj) { return obj is VertexFormat && Equals(obj as VertexFormat); }

            public override int GetHashCode() { return usage.GetHashCode(); }
            #endregion

            [ElementPriority(1)]
            public UsageType Usage { get { return usage; } set { if (!usage.Equals(value)) { usage = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
        }
        public class VertexFormatList : DependentList<VertexFormat>
        {
            uint version;

            #region Constructors
            public VertexFormatList(EventHandler handler, uint version) : base(handler) { this.version = version; }
            public VertexFormatList(EventHandler handler, uint version, Stream s) : base(null) { this.version = version; elementHandler = handler; Parse(s); this.handler = handler; }
            public VertexFormatList(EventHandler handler, uint version, IEnumerable<VertexFormat> le)
                : base(null)
            {
                elementHandler = handler;
                this.version = version;
                foreach (VertexFormat ve in le)
                    this.Add(new VertexFormat(0, elementHandler, version, ve.Usage, ve.dataType, ve.elementSize));
                this.handler = handler;
            }
            #endregion

            protected override VertexFormat CreateElement(Stream s) { return new VertexFormat(0, elementHandler, this.version, s); }
            protected override void WriteElement(Stream s, VertexFormat element) { element.UnParse(s); }

            //public override void Add() { this.Add(new VertexFormat(0, elementHandler)); }
        }
        #endregion

        #region VertexElement
        public abstract class VertexElement : AHandlerElement, IEquatable<VertexElement>
        {
            const int recommendedApiVersion = 1;

            protected VertexElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            protected VertexElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }

            protected abstract void Parse(Stream s);
            internal abstract void UnParse(Stream s);

            #region AHandlerElement
            //public abstract AHandlerElement Clone(EventHandler handler);
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public abstract bool Equals(VertexElement other);

            public virtual string Value { get { return string.Join("; ", ValueBuilder.Split('\n')); } }
        }
        public class PositionElement : VertexElement
        {
            protected float x, y, z;

            public PositionElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public PositionElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public PositionElement(int apiVersion, EventHandler handler, PositionElement basis) : this(apiVersion, handler, basis.x, basis.y, basis.z) { }
            public PositionElement(int apiVersion, EventHandler handler, float x, float y, float z) : base(apiVersion, handler) { this.x = x; this.y = y; this.z = z; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); x = r.ReadSingle(); y = r.ReadSingle(); z = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(x); w.Write(y); w.Write(z); }

            // public override AHandlerElement Clone(EventHandler handler) { return new PositionElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { PositionElement o = other as PositionElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is PositionElement && this.Equals(obj as PositionElement); }
            public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode(); }

            [ElementPriority(1)]
            public float X { get { return x; } set { if (!x.Equals(value)) { x = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float Y { get { return y; } set { if (!y.Equals(value)) { y = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float Z { get { return z; } set { if (!z.Equals(value)) { z = value; OnElementChanged(); } } }
        }
        public class NormalElement : PositionElement
        {
            public NormalElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public NormalElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public NormalElement(int apiVersion, EventHandler handler, NormalElement basis) : this(apiVersion, handler, basis.x, basis.y, basis.z) { }
            public NormalElement(int apiVersion, EventHandler handler, float x, float y, float z) : base(apiVersion, handler) { this.x = x; this.y = y; this.z = z; }

            // public override AHandlerElement Clone(EventHandler handler) { return new NormalElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { NormalElement o = other as NormalElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is NormalElement && this.Equals(obj as NormalElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class UVElement : VertexElement
        {
            protected float u, v;

            public UVElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public UVElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public UVElement(int apiVersion, EventHandler handler, UVElement basis) : this(apiVersion, handler, basis.u, basis.v) { }
            public UVElement(int apiVersion, EventHandler handler, float u, float v) : base(apiVersion, handler) { this.u = u; this.v = v; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); u = r.ReadSingle(); v = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(u); w.Write(v); }

            // public override AHandlerElement Clone(EventHandler handler) { return new UVElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { UVElement o = other as UVElement; return o != null && u.Equals(o.u) && v.Equals(o.v); }
            public override bool Equals(object obj) { return obj is UVElement && this.Equals(obj as UVElement); }
            public override int GetHashCode() { return u.GetHashCode() ^ v.GetHashCode(); }

            [ElementPriority(1)]
            public float U { get { return u; } set { if (!u.Equals(value)) { u = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float V { get { return v; } set { if (!v.Equals(value)) { v = value; OnElementChanged(); } } }
        }
        public class BoneAssignmentElement : VertexElement
        {
            protected byte b1, b2, b3, b4;

            public BoneAssignmentElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public BoneAssignmentElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public BoneAssignmentElement(int apiVersion, EventHandler handler, BoneAssignmentElement basis) : this(apiVersion, handler, basis.b1, basis.b2, basis.b3, basis.b4) { }
            public BoneAssignmentElement(int apiVersion, EventHandler handler, byte b1, byte b2, byte b3, byte b4) : base(apiVersion, handler) { this.b1 = b1; this.b2 = b2; this.b3 = b3; this.b4 = b4; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); b1 = r.ReadByte(); b2 = r.ReadByte(); b3 = r.ReadByte(); b4 = r.ReadByte(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(b1); w.Write(b2); w.Write(b3); w.Write(b4); }

            // public override AHandlerElement Clone(EventHandler handler) { return new BoneAssignmentElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { BoneAssignmentElement o = other as BoneAssignmentElement; return o != null && b1.Equals(o.b1) && b2.Equals(o.b2) && b3.Equals(o.b3) && b4.Equals(o.b4); }
            public override bool Equals(object obj) { return obj is BoneAssignmentElement && this.Equals(obj as BoneAssignmentElement); }
            public override int GetHashCode() { return b1.GetHashCode() ^ b2.GetHashCode() ^ b3.GetHashCode() ^ b4.GetHashCode(); }

            [ElementPriority(1)]
            public byte B1 { get { return b1; } set { if (!b1.Equals(value)) { b1 = value; OnElementChanged(); } } }
            public byte B2 { get { return b2; } set { if (!b2.Equals(value)) { b2 = value; OnElementChanged(); } } }
            public byte B3 { get { return b3; } set { if (!b3.Equals(value)) { b3 = value; OnElementChanged(); } } }
            public byte B4 { get { return b4; } set { if (!b4.Equals(value)) { b4 = value; OnElementChanged(); } } }
        }
        public class WeightsElement : VertexElement
        {
            protected float w1, w2, w3, w4;

            public WeightsElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public WeightsElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public WeightsElement(int apiVersion, EventHandler handler, WeightsElement basis) : this(apiVersion, handler, basis.w1, basis.w2, basis.w3, basis.w4) { }
            public WeightsElement(int apiVersion, EventHandler handler, float w1, float w2, float w3, float w4) : base(apiVersion, handler) { this.w1 = w1; this.w2 = w2; this.w3 = w3; this.w4 = w4; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); w1 = r.ReadSingle(); w2 = r.ReadSingle(); w3 = r.ReadSingle(); w4 = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(w1); w.Write(w2); w.Write(w3); w.Write(w4); }

            // public override AHandlerElement Clone(EventHandler handler) { return new WeightsElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { WeightsElement o = other as WeightsElement; return o != null && w1.Equals(o.w1) && w2.Equals(o.w2) && w3.Equals(o.w3) && w3.Equals(o.w4); }
            public override bool Equals(object obj) { return obj is WeightsElement && this.Equals(obj as WeightsElement); }
            public override int GetHashCode() { return w1.GetHashCode() ^ w2.GetHashCode() ^ w3.GetHashCode() ^ w4.GetHashCode(); }

            [ElementPriority(1)]
            public float W1 { get { return w1; } set { if (!w1.Equals(value)) { w1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float W2 { get { return w2; } set { if (!w2.Equals(value)) { w2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float W3 { get { return w3; } set { if (!w3.Equals(value)) { w3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public float W4 { get { return w4; } set { if (!w4.Equals(value)) { w4 = value; OnElementChanged(); } } }
        }
        public class WeightBytesElement : VertexElement
        {
            protected byte w1, w2, w3, w4;

            public WeightBytesElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public WeightBytesElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public WeightBytesElement(int apiVersion, EventHandler handler, WeightBytesElement basis) : this(apiVersion, handler, basis.w1, basis.w2, basis.w3, basis.w4) { }
            public WeightBytesElement(int apiVersion, EventHandler handler, byte w1, byte w2, byte w3, byte w4) : base(apiVersion, handler) { this.w1 = w1; this.w2 = w2; this.w3 = w3; this.w4 = w4; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); w1 = r.ReadByte(); w2 = r.ReadByte(); w3 = r.ReadByte(); w4 = r.ReadByte(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(w1); w.Write(w2); w.Write(w3); w.Write(w4); }

            // public override AHandlerElement Clone(EventHandler handler) { return new WeightBytesElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { WeightBytesElement o = other as WeightBytesElement; return o != null && w1.Equals(o.w1) && w2.Equals(o.w2) && w3.Equals(o.w3) && w3.Equals(o.w4); }
            public override bool Equals(object obj) { return obj is WeightBytesElement && this.Equals(obj as WeightBytesElement); }
            public override int GetHashCode() { return w1.GetHashCode() ^ w2.GetHashCode() ^ w3.GetHashCode() ^ w4.GetHashCode(); }

            [ElementPriority(1)]
            public byte W1 { get { return w1; } set { if (!w1.Equals(value)) { w1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public byte W2 { get { return w2; } set { if (!w2.Equals(value)) { w2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public byte W3 { get { return w3; } set { if (!w3.Equals(value)) { w3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte W4 { get { return w4; } set { if (!w4.Equals(value)) { w4 = value; OnElementChanged(); } } }
        }
        public class TangentNormalElement : PositionElement
        {
            public TangentNormalElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public TangentNormalElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public TangentNormalElement(int apiVersion, EventHandler handler, TangentNormalElement basis) : this(apiVersion, handler, basis.x, basis.y, basis.z) { }
            public TangentNormalElement(int apiVersion, EventHandler handler, float x, float y, float z) : base(apiVersion, handler) { this.x = x; this.y = y; this.z = z; }

            // public override AHandlerElement Clone(EventHandler handler) { return new TangentNormalElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { TangentNormalElement o = other as TangentNormalElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is TangentNormalElement && this.Equals(obj as TangentNormalElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class ColorElement : VertexElement
        {
            int argb;

            public ColorElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public ColorElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public ColorElement(int apiVersion, EventHandler handler, ColorElement basis) : this(apiVersion, handler, basis.argb) { }
            public ColorElement(int apiVersion, EventHandler handler, int argb) : base(apiVersion, handler) { this.argb = argb; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); argb = r.ReadInt32(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(argb); }

            // public override AHandlerElement Clone(EventHandler handler) { return new ColorElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { ColorElement o = other as ColorElement; return o != null && argb.Equals(o.argb); }
            public override bool Equals(object obj) { return obj is PositionElement && this.Equals(obj as PositionElement); }
            public override int GetHashCode() { return argb.GetHashCode(); }

            public override string Value { get { return Color.FromArgb(argb).ToString(); } }

            [ElementPriority(1)]
            public byte Alpha { get { return Color.FromArgb(argb).A; } set { if (!Color.FromArgb(argb).A.Equals(value)) { argb = Color.FromArgb(value, Color.FromArgb(argb).R, Color.FromArgb(argb).G, Color.FromArgb(argb).B).ToArgb(); OnElementChanged(); } } }
            [ElementPriority(2)]
            public byte Red { get { return Color.FromArgb(argb).R; } set { if (!Color.FromArgb(argb).R.Equals(value)) { argb = Color.FromArgb(Color.FromArgb(argb).A, value, Color.FromArgb(argb).G, Color.FromArgb(argb).B).ToArgb(); OnElementChanged(); } } }
            [ElementPriority(3)]
            public byte Green { get { return Color.FromArgb(argb).G; } set { if (!Color.FromArgb(argb).G.Equals(value)) { argb = Color.FromArgb(Color.FromArgb(argb).A, Color.FromArgb(argb).R, value, Color.FromArgb(argb).B).ToArgb(); OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Blue { get { return Color.FromArgb(argb).B; } set { if (!Color.FromArgb(argb).B.Equals(value)) { argb = Color.FromArgb(Color.FromArgb(argb).A, Color.FromArgb(argb).R, Color.FromArgb(argb).G, value).ToArgb(); OnElementChanged(); } } }
        }
        public class VertexIDElement : BoneAssignmentElement
        {
            public VertexIDElement(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public VertexIDElement(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
            public VertexIDElement(int apiVersion, EventHandler handler, VertexIDElement basis) : this(apiVersion, handler, basis.b1, basis.b2, basis.b3, basis.b4) { }
            public VertexIDElement(int apiVersion, EventHandler handler, byte b1, byte b2, byte b3, byte b4) : base(apiVersion, handler) { this.b1 = b1; this.b2 = b2; this.b3 = b3; this.b4 = b4; }

            // public override AHandlerElement Clone(EventHandler handler) { return new VertexIDElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { VertexIDElement o = other as VertexIDElement; return o != null && b1.Equals(o.b1) && b2.Equals(o.b2) && b3.Equals(o.b3) && b4.Equals(o.b4); }
            public override bool Equals(object obj) { return obj is VertexIDElement && this.Equals(obj as VertexIDElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class ElementList : DependentList<VertexElement>
        {
            private uint version;

            public DependentList<VertexFormat> ParentVertexFormats { get; private set; }

            #region Constructors
            public ElementList(EventHandler handler, uint version) : base(handler) { }
            public ElementList(EventHandler handler, uint version, Stream s, DependentList<VertexFormat> parentVertexFormats)
                : base(null)
            {
                this.version = version;
                this.ParentVertexFormats = parentVertexFormats;
                elementHandler = handler;
                foreach (var fmt in parentVertexFormats)
                {
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: this.Add(new PositionElement(0, handler, s)); break;
                        case UsageType.Normal: this.Add(new NormalElement(0, handler, s)); break;
                        case UsageType.UV: this.Add(new UVElement(0, handler, s)); break;
                        case UsageType.BoneAssignment: this.Add(new BoneAssignmentElement(0, handler, s)); break;
                        case UsageType.Weights:
                            switch (fmt.dataType)
                            {
                                case 1: this.Add(new WeightsElement(0, handler, s)); break;
                                case 2: this.Add(new WeightBytesElement(0, handler, s)); break;
                            }
                            break;
                        case UsageType.TangentNormal: this.Add(new TangentNormalElement(0, handler, s)); break;
                        case UsageType.Color: this.Add(new ColorElement(0, handler, s)); break;
                        case UsageType.VertexID: this.Add(new VertexIDElement(0, handler, s)); break;
                    }
                }
                this.handler = handler;
            }
            public ElementList(EventHandler handler, uint version, IEnumerable<VertexElement> ilt, DependentList<VertexFormat> parentVertexFormats)
                : base(null)
            {
                this.version = version;
                this.ParentVertexFormats = parentVertexFormats;
                elementHandler = handler;
                //foreach (var fmt in parentVertexFormats)
                for (int i = 0; i < parentVertexFormats.Count; i++)
                {
                    VertexElement vtx = (i < ilt.Count()) ? ilt.ElementAt(i) : null;
                    VertexFormat fmt = parentVertexFormats[i];
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: this.Add(vtx is PositionElement ? vtx : new PositionElement(0, handler)); break;

                        //case UsageType.Position: this.Add(ilt.FirstOrDefault(t => t is PositionElement) ?? new PositionElement(0, handler)); break;
                        case UsageType.Normal: this.Add(vtx is NormalElement ? vtx : new NormalElement(0, handler)); break;
                        case UsageType.UV: this.Add(vtx is UVElement ? vtx : new UVElement(0, handler)); break;
                        case UsageType.BoneAssignment: this.Add(vtx is BoneAssignmentElement ? vtx : new BoneAssignmentElement(0, handler)); break;

                        //case UsageType.Normal: this.Add(ilt.FirstOrDefault(t => t is NormalElement) ?? new NormalElement(0, handler)); break;
                        //case UsageType.UV: this.Add(ilt.FirstOrDefault(t => t is UVElement) ?? new UVElement(0, handler)); break;
                        //case UsageType.BoneAssignment: this.Add(ilt.FirstOrDefault(t => t is BoneAssignmentElement) ?? new BoneAssignmentElement(0, handler)); break;
                        //case UsageType.Weights: this.Add(vtx is WeightsElement ? vtx : new WeightsElement(0, handler)); break;

                        case UsageType.Weights:
                            switch (fmt.dataType)
                            {
                                case 1: this.Add(vtx is WeightsElement ? vtx : new WeightsElement(0, handler)); break;
                                case 2: this.Add(vtx is WeightBytesElement ? vtx : new WeightBytesElement(0, handler)); break;
                            }
                            break;

                        //case UsageType.TangentNormal: this.Add(ilt.FirstOrDefault(t => t is TangentNormalElement) ?? new TangentNormalElement(0, handler)); break;
                        //case UsageType.Color: this.Add(ilt.FirstOrDefault(t => t is ColorElement) ?? new ColorElement(0, handler)); break;
                        //case UsageType.VertexID: this.Add(ilt.FirstOrDefault(t => t is VertexIDElement) ?? new VertexIDElement(0, handler)); break;
                        case UsageType.TangentNormal: this.Add(vtx is TangentNormalElement ? vtx : new TangentNormalElement(0, handler)); break;
                        case UsageType.Color: this.Add(vtx is ColorElement ? vtx : new ColorElement(0, handler)); break;
                        case UsageType.VertexID: this.Add(vtx is VertexIDElement ? vtx : new VertexIDElement(0, handler)); break;

                    }
                }
                this.handler = handler;
            }
            #endregion

            protected override VertexElement CreateElement(Stream s) { throw new NotImplementedException(); }

            public override void UnParse(Stream s)
            {
                for (int i = 0; i < ParentVertexFormats.Count; i++)
                {
                    VertexElement vtx = (i < this.Count) ? this[i] : null;
                    VertexFormat fmt = ParentVertexFormats[i];
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: (vtx is PositionElement ? vtx : new PositionElement(0, null)).UnParse(s); break;
                        case UsageType.Normal: (vtx is NormalElement ? vtx : new NormalElement(0, null)).UnParse(s); break;
                        case UsageType.UV: (vtx is UVElement ? vtx : new UVElement(0, null)).UnParse(s); break;
                        case UsageType.BoneAssignment: (vtx is BoneAssignmentElement ? vtx : new BoneAssignmentElement(0, null)).UnParse(s); break;
                        case UsageType.Weights:
                            switch (fmt.dataType)
                            {
                                case 1: (vtx is WeightsElement ? vtx : new WeightsElement(0, null)).UnParse(s); break;
                                case 2: (vtx is WeightBytesElement ? vtx : new WeightBytesElement(0, null)).UnParse(s); break;
                            }
                            break;
                        case UsageType.TangentNormal: (vtx is TangentNormalElement ? vtx : new TangentNormalElement(0, null)).UnParse(s); break;
                        case UsageType.Color: (vtx is ColorElement ? vtx : new ColorElement(0, null)).UnParse(s); break;
                        case UsageType.VertexID: (vtx is VertexIDElement ? vtx : new VertexIDElement(0, null)).UnParse(s); break;
                    }
                }
                /*
                foreach (var fmt in ParentVertexFormats)
                {
                    VertexElement vtx = null;
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: vtx = this.Find(e => e is PositionElement); break;
                        case UsageType.Normal: vtx = this.Find(e => e is NormalElement); break;
                        case UsageType.UV: vtx = this.Find(e => e is UVElement); break;
                        case UsageType.BoneAssignment: vtx = this.Find(e => e is BoneAssignmentElement); break;
                        case UsageType.Weights:
                            switch (this.version)
                            {
                                case 0x00000005: vtx = this.Find(e => e is WeightsElement); break;
                                case 0x0000000C: vtx = this.Find(e => e is WeightBytesElement); break;
                            }
                            break;
                        case UsageType.TangentNormal: vtx = this.Find(e => e is TangentNormalElement); break;
                        case UsageType.Color: vtx = this.Find(e => e is ColorElement); break;
                        case UsageType.VertexID: vtx = this.Find(e => e is VertexIDElement); break;
                    }
                    if (vtx == null)
                        throw new InvalidOperationException();
                    vtx.UnParse(s);
                }
                 * */

            }

            protected override void WriteElement(Stream s, VertexElement element) { throw new NotImplementedException(); }

            public override void Add() { throw new NotImplementedException(); }

          /*  public VertexElement this[UsageType usage]
            {
                get
                {
                    if (!ParentVertexFormats.Exists(x => x.Usage.Equals(usage)))
                        throw new IndexOutOfRangeException();
                    switch (usage)
                    {
                        case UsageType.Position: return this.Find(x => x is PositionElement);
                        case UsageType.Normal: return this.Find(x => x is NormalElement);
                        case UsageType.UV: return this.Find(x => x is UVElement);
                        case UsageType.BoneAssignment: return this.Find(x => x is BoneAssignmentElement);
                        case UsageType.Weights:
                            switch (this)
                            {
                                case 0x00000005: return this.Find(x => x is WeightsElement); 
                                case 0x0000000C:
                                case 0x0000000D: return this.Find(x => x is WeightBytesElement);
                            }
                            break;
                        case UsageType.TangentNormal: return this.Find(x => x is TangentNormalElement);
                        case UsageType.Color: return this.Find(x => x is ColorElement);
                        case UsageType.VertexID: return this.Find(x => x is VertexIDElement);
                    }
                    throw new ArgumentException();
                }
                set
                {
                    VertexElement vtx = this[usage];
                    if (vtx != null && vtx.Equals(value)) return;

                    int index = this.IndexOf(vtx);
                    if (value.GetType().Equals(vtx.GetType()))
                        this[index] = vtx.Clone(handler) as VertexElement;
                    else
                        throw new ArgumentException();
                }
            } */
        }
        public class VertexDataElement : AHandlerElement, IEquatable<VertexDataElement>
        {
            const int recommendedApiVersion = 1;

            private uint version;
            private ElementList elementList;

            public DependentList<VertexFormat> ParentVertexFormats { get; set; }
            public override List<string> ContentFields { get { var res = GetContentFields(requestedApiVersion, this.GetType()); res.Remove("ParentVertexFormats"); return res; } }

            public VertexDataElement(int apiVersion, EventHandler handler, uint version, DependentList<VertexFormat> parentVertexFormats) : base(apiVersion, handler) { this.version = version; this.ParentVertexFormats = parentVertexFormats; }
            public VertexDataElement(int apiVersion, EventHandler handler, uint version, Stream s, DependentList<VertexFormat> parentVertexFormats) : base(apiVersion, handler) { this.version = version; this.ParentVertexFormats = parentVertexFormats; Parse(s); }
            public VertexDataElement(int apiVersion, EventHandler handler, uint version, VertexDataElement basis) : this(apiVersion, handler, basis.version, basis.elementList, basis.ParentVertexFormats) { }
            public VertexDataElement(int apiVersion, EventHandler handler, uint version, DependentList<VertexElement> elementList, DependentList<VertexFormat> parentVertexFormats)
                : base(apiVersion, handler)
            {
                this.version = version;
                this.ParentVertexFormats = parentVertexFormats;//reference!
                this.elementList = new ElementList(handler, version, elementList, ParentVertexFormats);
            }

            private void Parse(Stream s) { elementList = new ElementList(handler, version, s, ParentVertexFormats); }
            internal void UnParse(Stream s) { elementList.UnParse(s); }

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new VertexDataElement(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            #endregion

            public bool Equals(VertexDataElement other) { return elementList.Equals(other.elementList); }
            public override bool Equals(object obj) { return obj is VertexDataElement && this.Equals(obj as VertexDataElement); }
            public override int GetHashCode() { return elementList.GetHashCode(); }

            public ElementList Vertex
            {
                get { return elementList; }
                set { if (!elementList.Equals(value)) { elementList = new ElementList(handler, version, value, ParentVertexFormats); OnElementChanged(); } }
            }

            /*
            private string zValue
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var fmt in ParentVertexFormats)
                    {
                        sb.AppendLine(fmt.Usage.ToString() + ": " + elementList[fmt.Usage].Value);
                    }
                    return sb.ToString();
                }
            }
             * */
            public string Value
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ParentVertexFormats.Count; i++)
                    {
                        VertexElement vtx = (i < elementList.Count) ? elementList[i] : null;
                        VertexFormat fmt = ParentVertexFormats[i];
                        switch (fmt.Usage)
                        {
                            case UsageType.Position: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is PositionElement ? vtx : new PositionElement(0, null)).Value); break;
                            case UsageType.Normal: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is NormalElement ? vtx : new NormalElement(0, null)).Value); break;
                            case UsageType.UV: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is UVElement ? vtx : new UVElement(0, null)).Value); break;
                            case UsageType.BoneAssignment: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is BoneAssignmentElement ? vtx : new BoneAssignmentElement(0, null)).Value); break;
                            case UsageType.Weights:
                                switch (fmt.dataType)
                                {
                                    case 1: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is WeightsElement ? vtx : new WeightsElement(0, null)).Value); break;
                                    case 2: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is WeightBytesElement ? vtx : new WeightBytesElement(0, null)).Value); break;
                                }
                                break;

                            case UsageType.TangentNormal: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is TangentNormalElement ? vtx : new TangentNormalElement(0, null)).Value); break;
                            case UsageType.Color: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is ColorElement ? vtx : new ColorElement(0, null)).Value); break;
                            case UsageType.VertexID: sb.AppendLine(fmt.Usage.ToString() + ": " + (vtx is VertexIDElement ? vtx : new VertexIDElement(0, null)).Value); break;
                        }
                    }
                    return sb.ToString();
                }
            }
        }

        public class VertexDataList : DependentList<VertexDataElement>
        {
            int origCount;
            uint version;
            DependentList<VertexFormat> parentVertexFormats;

            #region Constructors
            public VertexDataList(EventHandler handler, uint version, DependentList<VertexFormat> parentVertexFormats) : base(handler) { this.version = version; this.parentVertexFormats = parentVertexFormats; }
            public VertexDataList(EventHandler handler, uint version, Stream s, int origCount, DependentList<VertexFormat> parentVertexFormats) : base(null) { this.origCount = origCount; this.version = version; this.parentVertexFormats = parentVertexFormats; elementHandler = handler; Parse(s); this.handler = handler; }
            public VertexDataList(EventHandler handler, uint version, IEnumerable<VertexDataElement> ilt, DependentList<VertexFormat> parentVertexFormats)
                : base(null)
            {
                this.version = version;
                this.parentVertexFormats = parentVertexFormats;
                elementHandler = handler;
                foreach (var t in ilt)
                    this.Add(t);
                this.handler = handler;
            }
            #endregion

            protected override int ReadCount(Stream s) { return origCount; }
            protected override VertexDataElement CreateElement(Stream s) { return new VertexDataElement(0, elementHandler, version, s, parentVertexFormats); }

            protected override void WriteCount(Stream s, int count) { }
            protected override void WriteElement(Stream s, VertexDataElement element) { element.UnParse(s); }

            public override void Add() { this.Add(new VertexDataElement(0, elementHandler, version, parentVertexFormats)); }
            public override void Add(VertexDataElement item) { item.ParentVertexFormats = parentVertexFormats; base.Add(item); }
        }
        #endregion

        public class Face : AHandlerElement, IEquatable<Face>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            ushort facePoint0;
            ushort facePoint1;
            ushort facePoint2;
            #endregion

            public Face(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public Face(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public Face(int apiVersion, EventHandler handler, Face basis)
                : this(apiVersion, handler, basis.facePoint0, basis.facePoint1, basis.facePoint2) { }
            public Face(int apiVersion, EventHandler handler, ushort vertexDataIndex0, ushort vertexDataIndex1, ushort vertexDataIndex2)
                : base(apiVersion, handler)
            {
                this.facePoint0 = vertexDataIndex0;
                this.facePoint1 = vertexDataIndex1;
                this.facePoint2 = vertexDataIndex2;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                facePoint0 = r.ReadUInt16();
                facePoint1 = r.ReadUInt16();
                facePoint2 = r.ReadUInt16();
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(facePoint0);
                w.Write(facePoint1);
                w.Write(facePoint2);
            }

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new Face(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Face>
            public bool Equals(Face other)
            {
                return this.facePoint0.Equals(other.facePoint0)
                    && this.facePoint1.Equals(other.facePoint1)
                    && this.facePoint2.Equals(other.facePoint2);
            }

            public override bool Equals(object obj) { return obj is VertexFormat && Equals(obj as VertexFormat); }

            public override int GetHashCode() { return facePoint0.GetHashCode() ^ facePoint1.GetHashCode() ^ facePoint2.GetHashCode(); }
            #endregion

            [ElementPriority(1)]
            public ushort VertexDataIndex0 { get { return facePoint0; } set { if (facePoint0 != value) { facePoint0 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort VertexDataIndex1 { get { return facePoint1; } set { if (facePoint1 != value) { facePoint1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ushort VertexDataIndex2 { get { return facePoint2; } set { if (facePoint2 != value) { facePoint2 = value; OnElementChanged(); } } }

            public string Value { get { return string.Join("; ", ValueBuilder.Split('\n')); } }
        }
        public class FaceList : DependentList<Face>
        {
            #region Constructors
            public FaceList(EventHandler handler) : base(handler) { }
            public FaceList(EventHandler handler, Stream s) : base(handler, s) { }
            public FaceList(EventHandler handler, IEnumerable<Face> le) : base(handler, le) { }
            #endregion

            protected override int ReadCount(Stream s) { return base.ReadCount(s) / 3; }
            protected override Face CreateElement(Stream s) { return new Face(0, elementHandler, s); }
            protected override void WriteCount(Stream s, int count) { base.WriteCount(s, (int)(count * 3)); }
            protected override void WriteElement(Stream s, Face element) { element.UnParse(s); }

            //public override void Add() { this.Add(new Face(0, elementHandler)); }
        }

        public class UVStitch : AHandlerElement, IEquatable<UVStitch>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            uint index;
            Vector2List uv;
            #endregion

            public UVStitch(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public UVStitch(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public UVStitch(int apiVersion, EventHandler handler, UVStitch basis) : this(apiVersion, handler, basis.index, basis.uv) { }
            public UVStitch(int apiVersion, EventHandler handler, uint index, IEnumerable<Vector2> uv)
                : base(apiVersion, handler)
            {
                this.index = index;
                this.uv = uv == null ? null : new Vector2List(handler, uv);
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                index = r.ReadUInt32();
                uv = new Vector2List(handler, s);
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(index);
                if (uv == null) uv = new Vector2List(handler);
                uv.UnParse(s);
            }

            #region AHandlerElement
            // public override UnknownThing Clone(EventHandler handler) { return new UnknownThing(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<UVStitch>
            public bool Equals(UVStitch other)
            {
                if (this.index != other.index)
                    return false;

                if (this.uv == null && other.uv != null)
                    return false;

                if (this.uv != null)
                {
                    if (other.uv == null || this.uv.Count != other.uv.Count)
                        return false;

                    for (int i = this.uv.Count - 1; i >= 0; i--)
                    {
                        if (!this.uv[i].Equals(other.uv[i]))
                            return false;
                    }
                    return true;
                }

                return true;
            }

            public override bool Equals(object obj) { return obj is UVStitch && Equals(obj as UVStitch); }

            public override int GetHashCode() { return index.GetHashCode() ^ uv.GetHashCode(); }
            #endregion
            [ElementPriority(1)]
            public uint Index { get { return index; } set { if (index != value) { index = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public Vector2List UVList { get { return uv; } set { if (!uv.Equals(value)) { uv = value == null ? null : new Vector2List(handler, value); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
        }
        public class UVStitchList : DependentList<UVStitch>
        {
            #region Constructors
            public UVStitchList(EventHandler handler) : base(handler) { }
            public UVStitchList(EventHandler handler, Stream s) : base(handler, s) { }
            public UVStitchList(EventHandler handler, IEnumerable<UVStitch> le) : base(handler, le) { }
            #endregion

            protected override UVStitch CreateElement(Stream s) { return new UVStitch(0, elementHandler, s); }
            protected override void WriteElement(Stream s, UVStitch element) { element.UnParse(s); }
        }

        public class SeamStitch : AHandlerElement, IEquatable<SeamStitch>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            uint index;
            ushort vertID;
            #endregion

            public SeamStitch(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public SeamStitch(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public SeamStitch(int apiVersion, EventHandler handler, SeamStitch basis) : this(apiVersion, handler, basis.index, basis.vertID) { }
            public SeamStitch(int apiVersion, EventHandler handler, uint index, ushort vertID)
                : base(apiVersion, handler)
            {
                this.index = index;
                this.vertID = vertID;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                index = r.ReadUInt32();
                vertID = r.ReadUInt16();
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(index);
                w.Write(vertID);
            }

            #region AHandlerElement
            // public override UnknownThing Clone(EventHandler handler) { return new UnknownThing(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<SeamStitch>
            public bool Equals(SeamStitch other)
            {
                return (this.index == other.index) && (this.vertID == other.vertID);
            }

            public override bool Equals(object obj) { return obj is SeamStitch && Equals(obj as SeamStitch); }

            public override int GetHashCode() { return index.GetHashCode() ^ vertID.GetHashCode(); }
            #endregion
            [ElementPriority(1)]
            public uint Index { get { return index; } set { if (index != value) { index = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort VertexID { get { return vertID; } set { if (!vertID.Equals(value)) { vertID = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
        }
        public class SeamStitchList : DependentList<SeamStitch>
        {
            #region Constructors
            public SeamStitchList(EventHandler handler) : base(handler) { }
            public SeamStitchList(EventHandler handler, Stream s) : base(handler, s) { }
            public SeamStitchList(EventHandler handler, IEnumerable<SeamStitch> le) : base(handler, le) { }
            #endregion

            protected override SeamStitch CreateElement(Stream s) { return new SeamStitch(0, elementHandler, s); }
            protected override void WriteElement(Stream s, SeamStitch element) { element.UnParse(s); }
        }

        public class SlotrayIntersection : AHandlerElement, IEquatable<SlotrayIntersection>
        {
            const int recommendedApiVersion = 1;
            // const int unk5size = 53;//bytes;
            // Sizes found: 53 (common), 245

            #region Attributes
            uint slotIndex;
            ushort[] indices;
            float[] coordinates;
            float distance;
            Vector3 offsetFromIntersectionOS;
            Vector3 slotAveragePosOS;
            Quaternion transformToLS;
            byte pivotBoneIdx;
            uint pivotBoneHash;
            uint parentVersion;
            #endregion

            public SlotrayIntersection(int apiVersion, EventHandler handler, uint version) : base(apiVersion, handler) { this.parentVersion = version; }
            public SlotrayIntersection(int apiVersion, EventHandler handler, uint version, Stream s) : base(apiVersion, handler) { this.parentVersion = version; Parse(s); }
            public SlotrayIntersection(int apiVersion, EventHandler handler, SlotrayIntersection basis)
                : this(apiVersion, handler, basis.slotIndex,
                basis.indices, basis.coordinates,
                basis.distance, basis.offsetFromIntersectionOS,
                basis.slotAveragePosOS, 
                basis.transformToLS, basis.pivotBoneIdx, basis.pivotBoneHash, basis.parentVersion) { }
            public SlotrayIntersection(int apiVersion, EventHandler handler, uint slotIndex,
                ushort[] indices, float[] coordinates,
                float distance, Vector3 offsetFromIntersectionOS,
                Vector3 slotAveragePosOS,
                Quaternion transformToLS, byte pivotBoneIdx, uint pivotBoneHash, uint parentVersion)
                : base(apiVersion, handler)
            {
                this.slotIndex = slotIndex;
                this.indices = new ushort[3];
                for (int i = 0; i < 3; i++)
                {
                    this.indices[i] = indices[i];
                }
                this.coordinates = new float[2];
                for (int i = 0; i < 2; i++)
                {
                    this.coordinates[i] = coordinates[i];
                }
                this.distance = distance;
                this.offsetFromIntersectionOS = new Vector3(apiVersion, this.handler, offsetFromIntersectionOS);
                this.slotAveragePosOS = new Vector3(apiVersion, this.handler, slotAveragePosOS);
                this.transformToLS = new Quaternion(apiVersion, this.handler, transformToLS);
                this.pivotBoneIdx = pivotBoneIdx;
                this.pivotBoneHash = pivotBoneHash;
                this.parentVersion = parentVersion;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                slotIndex = r.ReadUInt32();
                indices = new ushort[3];
                for (int i = 0; i < 3; i++)
                {
                    indices[i] = r.ReadUInt16();
                }
                coordinates = new float[2];
                for (int i = 0; i < 2; i++)
                {
                    coordinates[i] = r.ReadSingle();
                }
                distance = r.ReadSingle();
                offsetFromIntersectionOS = new Vector3(this.RecommendedApiVersion, this.handler, s);
                slotAveragePosOS = new Vector3(this.RecommendedApiVersion, this.handler, s);
                transformToLS = new Quaternion(this.RecommendedApiVersion, this.handler, s);
                if (this.parentVersion >= 0x0E)
                {
                    this.pivotBoneHash = r.ReadUInt32();
                }
                else
                {
                    pivotBoneIdx = r.ReadByte();
                }
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(slotIndex);
                if (indices == null) indices = new ushort[3];
                for (int i = 0; i < 3; i++)
                {
                    w.Write(indices[i]);
                }
                if (coordinates == null) coordinates = new float[2];
                for (int i = 0; i < 2; i++)
                {
                    w.Write(coordinates[i]);
                }
                w.Write(distance);
                offsetFromIntersectionOS.UnParse(s);
                slotAveragePosOS.UnParse(s);
                transformToLS.UnParse(s);
                if (this.parentVersion >= 0x0E)
                {
                    w.Write(this.pivotBoneHash);
                }
                else
                {
                    w.Write(pivotBoneIdx);
                }
            }

            #region AHandlerElement
            // public override UnknownThing Clone(EventHandler handler) { return new UnknownThing(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            #endregion

            #region IEquatable<SlotrayIntersection>
            public bool Equals(SlotrayIntersection other)
            {
                return this.slotIndex.Equals(other.slotIndex)
                    && this.indices.Equals(other.indices)
                    && this.coordinates.Equals(other.coordinates)
                    && this.distance.Equals(other.distance)
                    && this.offsetFromIntersectionOS.Equals(other.offsetFromIntersectionOS)
                    && this.slotAveragePosOS.Equals(other.slotAveragePosOS)
                    && this.transformToLS.Equals(other.transformToLS)
                    && this.pivotBoneIdx.Equals(other.pivotBoneIdx)
                    && this.pivotBoneHash.Equals(other.pivotBoneHash);
            }

            public override bool Equals(object obj) { return obj is SlotrayIntersection && Equals(obj as SlotrayIntersection); }

            public override int GetHashCode() { return slotIndex.GetHashCode() ^ indices.GetHashCode() ^ coordinates.GetHashCode() ^ distance.GetHashCode() ^
                offsetFromIntersectionOS.GetHashCode() ^ slotAveragePosOS.GetHashCode() ^ transformToLS.GetHashCode() ^ pivotBoneIdx.GetHashCode() ^ pivotBoneHash.GetHashCode(); }
            #endregion
            [ElementPriority(1)]
            public uint SlotIndex { get { return slotIndex; } set { if (slotIndex != value) { slotIndex = value; OnElementChanged(); } } }
            [ElementPriority(1)]
            public uint SlotHash { get { return slotIndex; } set { if (slotIndex != value) { slotIndex = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort[] FacePointIndices { get { return indices; } set { if (indices != value) { indices = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public float[] IntersectionCoordinates { get { return coordinates; } set { if (coordinates != value) { coordinates = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public float Distance { get { return distance; } set { if (distance != value) { distance = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public Vector3 OffsetFromIntersectionInObjectSpace { get { return offsetFromIntersectionOS; } set { if (offsetFromIntersectionOS != value) { offsetFromIntersectionOS = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public Vector3 SlotAveragePositionInObjectSpace { get { return slotAveragePosOS; } set { if (slotAveragePosOS != value) { slotAveragePosOS = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public Quaternion TransformToLocalSpace { get { return transformToLS; } set { if (transformToLS != value) { transformToLS = value; OnElementChanged(); } } }
            [ElementPriority(18)]
            public byte PivotBoneIndex { get { return pivotBoneIdx; } set { if (pivotBoneIdx != value) { pivotBoneIdx = value; OnElementChanged(); } } }
            [ElementPriority(18)]
            public uint PivotBoneHash { get { return pivotBoneHash; } set { if (pivotBoneHash != value) { pivotBoneHash = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }

            public override List<string> ContentFields
            {
                get
                {
                    var res = GetContentFields(requestedApiVersion, this.GetType());
                    if (this.parentVersion >= 0x0E)
                    {
                        res.Remove("SlotIndex");
                        res.Remove("PivotBoneIndex");
                    }
                    else
                    {
                        res.Remove("SlotHash");
                        res.Remove("PivotBoneHash");
                    }
                    return res;
                }
            } 
        }
        public class SlotrayIntersectionList : DependentList<SlotrayIntersection>
        {
            uint parentVersion;
            #region Constructors
            public SlotrayIntersectionList(EventHandler handler, uint version) : base(handler) { this.parentVersion = version; }
            public SlotrayIntersectionList(EventHandler handler, uint version, Stream s) : base(null) { this.parentVersion = version; elementHandler = handler; Parse(s); this.handler = handler; }
            public SlotrayIntersectionList(EventHandler handler, uint version, IEnumerable<SlotrayIntersection> le) 
            : base(null)
            {
                elementHandler = handler;
                this.parentVersion = version;
                foreach (SlotrayIntersection sr in le)
                    this.Add(new SlotrayIntersection(0, elementHandler, sr));
                this.handler = handler;
            }

            #endregion

            protected override SlotrayIntersection CreateElement(Stream s) { return new SlotrayIntersection(0, elementHandler, parentVersion, s); }
            protected override void WriteElement(Stream s, SlotrayIntersection element) { element.UnParse(s); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public ShaderType Shader { get { return shader; } set { if (shader != value) { shader = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public MTNF Mtnf
        {
            get { return mtnf; }
            set
            {
                if ((shader == 0 && value != null) || (shader != 0 && value == null)) throw new ArgumentException();
                if (!mtnf.Equals(value))
                {
                    mtnf = new MTNF(requestedApiVersion, OnRCOLChanged, value) { ParentTGIBlocks = tgiBlockList, RCOLTag = "GEOM", };
                    OnRCOLChanged(this, EventArgs.Empty);
                }
            }
        }
        [ElementPriority(14)]
        public uint MergeGroup { get { return mergeGroup; } set { if (mergeGroup != value) { mergeGroup = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public uint SortOrder { get { return sortOrder; } set { if (sortOrder != value) { sortOrder = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public VertexFormatList VertexFormats
        {
            get { return vertexFormats; }
            set { if (!vertexFormats.Equals(value)) { vertexFormats = value == null ? null : new VertexFormatList(OnRCOLChanged, version, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(17)]
        public VertexDataList VertexData
        {
            get { return vertexData; }
            set { if (!vertexData.Equals(value)) { vertexData = value == null ? null : new VertexDataList(OnRCOLChanged, version, value, this.vertexFormats); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(18)]
        public FaceList Faces
        {
            get { return faces; }
            set { if (!faces.Equals(value)) { faces = value == null ? null : new FaceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(19), TGIBlockListContentField("TGIBlocks")]
        public int SkinIndex { get { return skinIndex; } set { if (skinIndex != value) { skinIndex = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(19)]
        public UVStitchList UVStitchData { get { return uvStitchList; } set { if (!uvStitchList.Equals(value)) { uvStitchList = value == null ? null : new UVStitchList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(20)]
        public SeamStitchList SeamStitchData { get { return seamStitchList; } set { if (!seamStitchList.Equals(value)) { seamStitchList = value == null ? null : new SeamStitchList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(20)]
        public SlotrayIntersectionList SlotrayIntersectionData { get { return slotrayIntersectionList; } set { if (!slotrayIntersectionList.Equals(value)) { slotrayIntersectionList = value == null ? null : new SlotrayIntersectionList(OnRCOLChanged, this.version, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(21)]
        public UIntList BoneHashes
        {
            get { return boneHashes; }
            set { if (!boneHashes.Equals(value)) { boneHashes = value == null ? null : new UIntList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(22)]
        public TGIBlockList TGIBlocks
        {
            get { return tgiBlockList; }
            set
            {
                if (!tgiBlockList.Equals(value))
                {
                    tgiBlockList = value == null ? null : new TGIBlockList(OnRCOLChanged, value);
                    if (mtnf != null)
                        mtnf.ParentTGIBlocks = tgiBlockList;
                    OnRCOLChanged(this, EventArgs.Empty);
                }
            }
        }

        public string Value { get { return ValueBuilder; } }

        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (shader == 0)
                    res.Remove("Mtnf");
                if (version < 0x0000000C)
                {
                    res.Remove("UVStitchData");
                    res.Remove("SlotrayIntersectionData");
                }
                else
                {
                    res.Remove("SkinIndex");
                }
                if (version < 0x0000000D) res.Remove("SeamStitchData");
                return res;
            }
        }
        #endregion
    }
}