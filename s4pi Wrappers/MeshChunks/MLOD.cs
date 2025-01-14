﻿/***************************************************************************
 *  Copyright (C) 2009, 2010 by Peter L Jones                              *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/

using System;
using System.IO;
using s4pi.Interfaces;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using s4pi.Settings;
using s4pi.GenericRCOLResource;

namespace meshExpImp.ModelBlocks
{
    public class MLOD : ARCOLBlock
    {
        private static bool checking = Settings.Checking;

        #region Attributes
        private UInt32 mVersion = 0x00000202;
        private MeshList mMeshes;
        #endregion

        #region Constructors
        public MLOD(int apiVersion, EventHandler handler, MLOD basis) : this(apiVersion, handler, basis.Version, new MeshList(handler,basis, basis.mMeshes)) { }
        public MLOD(int apiVersion, EventHandler handler) : base(apiVersion, handler, null) { }
        public MLOD(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler, s) { }
        public MLOD(int apiVersion, EventHandler handler, uint version, MeshList meshes)
            : base(apiVersion, handler, null)
        {
            mVersion = version;
            mMeshes = meshes;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "MLOD"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x01D10F34; } }

        protected override void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            string tag = FOURCC(br.ReadUInt32());
            if (checking && tag != Tag)
                throw new InvalidDataException(string.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{1:X8}", tag, Tag, s.Position));
            mVersion = br.ReadUInt32();
            mMeshes = new MeshList(handler, this, s);
        }

        public override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((uint)FOURCC(Tag));
            bw.Write(mVersion);
            if (mMeshes == null) mMeshes = new MeshList(handler, this);
            mMeshes.UnParse(s);
            return s;
        }
        #endregion

        #region Sub-types
        public class GeometryState : AHandlerElement, IEquatable<GeometryState>
        {
            private const int kRecommendedApiVersion = 1;

            private UInt32 mName;
            private Int32 mStartIndex;
            private Int32 mMinVertexIndex;
            private Int32 mVertexCount;
            private Int32 mPrimitiveCount;

            public GeometryState(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public GeometryState(int apiVersion, EventHandler handler, GeometryState basis) : this(apiVersion, handler, basis.Name, basis.StartIndex, basis.MinVertexIndex, basis.VertexCount, basis.PrimitiveCount) { }
            public GeometryState(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public GeometryState(int apiVersion, EventHandler handler, uint name, int startIndex, int minVertexIndex, int vertexCount, int primitiveCount)
                : base(apiVersion, handler)
            {
                mName = name;
                mStartIndex = startIndex;
                mMinVertexIndex = minVertexIndex;
                mVertexCount = vertexCount;
                mPrimitiveCount = primitiveCount;
            }

            [ElementPriority(1)]
            public UInt32 Name
            {
                get { return mName; }
                set { if (mName != value) { mName = value; OnElementChanged(); } }
            }
            [ElementPriority(2)]
            public Int32 StartIndex
            {
                get { return mStartIndex; }
                set { if (mStartIndex != value) { mStartIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public Int32 MinVertexIndex
            {
                get { return mMinVertexIndex; }
                set { if (mMinVertexIndex != value) { mMinVertexIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(4)]
            public Int32 VertexCount
            {
                get { return mVertexCount; }
                set { if (mVertexCount != value) { mVertexCount = value; OnElementChanged(); } }
            }
            [ElementPriority(5)]
            public Int32 PrimitiveCount
            {
                get { return mPrimitiveCount; }
                set { if (mPrimitiveCount != value) { mPrimitiveCount = value; OnElementChanged(); } }
            }

            private void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                mName = br.ReadUInt32();
                mStartIndex = br.ReadInt32();
                mMinVertexIndex = br.ReadInt32();
                mVertexCount = br.ReadInt32();
                mPrimitiveCount = br.ReadInt32();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                bw.Write(mName);
                bw.Write(mStartIndex);
                bw.Write(mMinVertexIndex);
                bw.Write(mVertexCount);
                bw.Write(mPrimitiveCount);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(base.requestedApiVersion, GetType()); }
            }

            public override int RecommendedApiVersion
            {
                get { return kRecommendedApiVersion; }
            }

            public bool Equals(GeometryState other)
            {
                return
                    mName.Equals(other.mName)
                    && mStartIndex.Equals(other.mStartIndex)
                    && mMinVertexIndex.Equals(other.mMinVertexIndex)
                    && mVertexCount.Equals(other.mVertexCount)
                    && mPrimitiveCount.Equals(other.mPrimitiveCount)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as GeometryState != null ? this.Equals(obj as GeometryState) : false;
            }
            public override int GetHashCode()
            {
                return
                    mName.GetHashCode()
                    ^ mStartIndex.GetHashCode()
                    ^ mMinVertexIndex.GetHashCode()
                    ^ mVertexCount.GetHashCode()
                    ^ mPrimitiveCount.GetHashCode()
                    ;
            }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Name: 0x{0:X8}\n", mName);
                    sb.AppendFormat("Start Index:\t{0}\n", mStartIndex);
                    sb.AppendFormat("Min Vertex Index:\t{0}\n", mMinVertexIndex);
                    sb.AppendFormat("Vertex Count:\t{0}\n", mVertexCount);
                    sb.AppendFormat("Primitive Count:\t{0}\n", mPrimitiveCount);
                    return sb.ToString();
                    /**/
                }
            }
        }

        public class GeometryStateList : DependentList<GeometryState>
        {
            public GeometryStateList(EventHandler handler) : base(handler) { }
            public GeometryStateList(EventHandler handler, Stream s) : base(handler, s) { }
            public GeometryStateList(EventHandler handler, IEnumerable<GeometryState> basis) : base(handler, basis) { }

            protected override GeometryState CreateElement(Stream s) { return new GeometryState(0, handler, s); }
            protected override void WriteElement(Stream s, GeometryState element) { element.UnParse(s); }

            //public override void Add() { Add(new GeometryState(0, handler)); }
        }

        public class Mesh : AHandlerElement, IEquatable<Mesh>
        {
            private const int kRecommendedApiVersion = 1;
            private MLOD mOwner;

            #region Attributes
            private UInt32 mName;
            private GenericRCOLResource.ChunkReference mMaterialIndex;
            private GenericRCOLResource.ChunkReference mVertexFormatIndex;
            private GenericRCOLResource.ChunkReference mVertexBufferIndex;
            private GenericRCOLResource.ChunkReference mIndexBufferIndex;
            private ModelPrimitiveType mPrimitiveType;
            private MeshFlags mFlags;
            private UInt32 mStreamOffset;
            private Int32 mStartVertex;
            private Int32 mStartIndex;
            private Int32 mMinVertexIndex;
            private Int32 mVertexCount;
            private Int32 mPrimitiveCount;
            private BoundingBox mBounds;
            private GenericRCOLResource.ChunkReference mSkinControllerIndex;
            private UIntList mJointReferences;
            private GenericRCOLResource.ChunkReference mMeshMaterialIndex;
            private GeometryStateList mGeometryStates;
            private UInt32 mParentBoneName;
            private Vector4 mMirrorPlane; 
            private UInt32 mSortOrderHint;
            private BoundingBox[] mBoundingBoxBones;
            #endregion

            #region Constructors
            public Mesh(int apiVersion, EventHandler handler, MLOD owner)
                : base(apiVersion, handler)
            {
                mOwner = owner;

                mMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mVertexFormatIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mVertexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mIndexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mBounds = new BoundingBox(requestedApiVersion, handler);
                mSkinControllerIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mJointReferences = new UIntList(handler);
                mGeometryStates = new GeometryStateList(handler);
                mMeshMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
                mMirrorPlane = new Vector4(requestedApiVersion, handler);//mOwner.Version > 0x00000201
                mBoundingBoxBones = new BoundingBox[0];
            }
            public Mesh(int apiVersion, EventHandler handler, Mesh basis)
                : this(apiVersion, handler, basis.mOwner,
                basis.mName,
                basis.mMaterialIndex, basis.mVertexFormatIndex, basis.mVertexBufferIndex, basis.mIndexBufferIndex,
                basis.mPrimitiveType, basis.mFlags,
                basis.mStreamOffset, basis.mStartVertex, basis.mStartIndex, basis.mMinVertexIndex, basis.mVertexCount, basis.mPrimitiveCount,
                basis.mBounds, basis.mSkinControllerIndex,
                basis.mJointReferences, basis.mGeometryStates, basis.mMeshMaterialIndex,
                basis.mParentBoneName, basis.mMirrorPlane,
                basis.mSortOrderHint, basis.mBoundingBoxBones
                ) { }
            public Mesh(int apiVersion, EventHandler handler, MLOD owner,
                uint name,
                GenericRCOLResource.ChunkReference materialIndex, GenericRCOLResource.ChunkReference vertexFormatIndex,
                GenericRCOLResource.ChunkReference vertexBufferIndex, GenericRCOLResource.ChunkReference indexBufferIndex,
                ModelPrimitiveType primitiveType, MeshFlags flags,
                uint streamOffset, int startVertex, int startIndex, int minVertexIndex, int vertexCount, int primitiveCount,
                BoundingBox bounds, GenericRCOLResource.ChunkReference skinControllerIndex,
                UIntList jointReferences, GeometryStateList geometryStates, GenericRCOLResource.ChunkReference scaleOffsetIndex,
                uint parentName, Vector4 mirrorPlane, uint sortOrderHint, BoundingBox[] boundingBoxBones
                )
                : base(apiVersion, handler)
            {
                mOwner = owner;

                mName = name;
                mMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, materialIndex);
                mVertexFormatIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, vertexFormatIndex);
                mVertexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, vertexBufferIndex);
                mIndexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, indexBufferIndex);
                mPrimitiveType = primitiveType;
                mFlags = flags;
                mStreamOffset = streamOffset;
                mStartVertex = startVertex;
                mStartIndex = startIndex;
                mMinVertexIndex = minVertexIndex;
                mVertexCount = vertexCount;
                mPrimitiveCount = primitiveCount;
                mBounds = new BoundingBox(requestedApiVersion, handler, bounds);
                mSkinControllerIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, skinControllerIndex);
                mJointReferences = jointReferences == null ? null : new UIntList(handler, jointReferences);
                mGeometryStates = geometryStates == null ? null : new GeometryStateList(handler, geometryStates);
                mMeshMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, scaleOffsetIndex);
                if (mOwner.Version > 0x00000201)
                {
                    mParentBoneName = parentName;
                    mMirrorPlane = new Vector4(requestedApiVersion, handler, mirrorPlane);
                }
                if (mOwner.Version > 0x00000203)
                {
                    mSortOrderHint = sortOrderHint;
                }
                if (boundingBoxBones != null && boundingBoxBones.Length > 0)
                {
                    mBoundingBoxBones = new BoundingBox[boundingBoxBones.Length];
                    for (int i = 0; i < mBoundingBoxBones.Length; i++)
                    {
                        mBoundingBoxBones[i] = new BoundingBox(requestedApiVersion, handler, boundingBoxBones[i]);
                    }
                }

            }
            public Mesh(int apiVersion, EventHandler handler, MLOD owner, Stream s) : base(apiVersion, handler) { mOwner = owner; Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                long expectedSize = br.ReadUInt32();
                long start = s.Position;

                mName = br.ReadUInt32();
                mMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                mVertexFormatIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                mVertexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                mIndexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                uint val = br.ReadUInt32();
                mPrimitiveType = (ModelPrimitiveType)(val & 0x000000FF);
                mFlags = (MeshFlags)(val >> 8);
                mStreamOffset = br.ReadUInt32();
                mStartVertex = br.ReadInt32();
                mStartIndex = br.ReadInt32();
                mMinVertexIndex = br.ReadInt32();
                mVertexCount = br.ReadInt32();
                mPrimitiveCount = br.ReadInt32();
                mBounds = new BoundingBox(0, handler, s);
                mSkinControllerIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                mJointReferences = new UIntList(handler, s);
                mMeshMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                mGeometryStates = new GeometryStateList(handler, s);
                if (mOwner.Version > 0x00000201)
                {
                    mParentBoneName = br.ReadUInt32();
                    mMirrorPlane = new Vector4(0, handler, s);
                }
                if (mOwner.Version > 0x00000203)
                {
                    mSortOrderHint = br.ReadUInt32();
                }
                if (mOwner.Version >= 0x00000206 && ((uint)Flags & 0x4000) > 0 && mJointReferences.Count > 0)
                {
                    mBoundingBoxBones = new BoundingBox[mJointReferences.Count];
                    for (int i = 0; i < mBoundingBoxBones.Length; i++)
                    {
                        mBoundingBoxBones[i] = new BoundingBox(0, handler, s);
                    }
                }
                long actualSize = s.Position - start;
                if (checking && actualSize != expectedSize)
                    throw new Exception(String.Format("Expected end at {0}, actual end was {1}", expectedSize,
                                                      actualSize));
            }

            public void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                long sizeOffset = s.Position;
                bw.Write(0);
                long start = s.Position;
                bw.Write(mName);
                mMaterialIndex.UnParse(s);
                mVertexFormatIndex.UnParse(s);
                mVertexBufferIndex.UnParse(s);
                mIndexBufferIndex.UnParse(s);
                bw.Write((UInt32)mPrimitiveType | ((UInt32)mFlags << 8));
                bw.Write(mStreamOffset);
                bw.Write(mStartVertex);
                bw.Write(mStartIndex);
                bw.Write(mMinVertexIndex);
                bw.Write(mVertexCount);
                bw.Write(mPrimitiveCount);
                mBounds.UnParse(s);
                mSkinControllerIndex.UnParse(s);
                mJointReferences.UnParse(s);
                mMeshMaterialIndex.UnParse(s);
                mGeometryStates.UnParse(s);
                if (mOwner.Version > 0x00000201)
                {
                    bw.Write(mParentBoneName);
                    mMirrorPlane.UnParse(s);
                }
                if (mOwner.Version > 0x00000203)
                {
                    bw.Write(mSortOrderHint);
                }
                if (mOwner.Version >= 0x00000206 && ((uint)Flags & 0x4000) > 0 && mJointReferences.Count > 1)
                {
                    if (mBoundingBoxBones == null) mBoundingBoxBones = new BoundingBox[0];
                    for (int i = 0; i < mBoundingBoxBones.Length; i++)
                    {
                        mBoundingBoxBones[i].UnParse(s);
                    }
                }
                long end = s.Position;
                long size = end - start;
                s.Seek(sizeOffset, SeekOrigin.Begin);
                bw.Write((uint)size);
                s.Seek(end, SeekOrigin.Begin);
            }
            #endregion

            #region AHandlerElement
            public override List<string> ContentFields
            {
                get
                {
                    var fields = GetContentFields(base.requestedApiVersion, GetType());

                    if (mOwner.Version < 0x00000202)
                    {
                        fields.Remove("ParentBoneName");
                        fields.Remove("MirrorPlane");
                    }
                    if (mOwner.Version < 0x00000204)
                    {
                        fields.Remove("SortOrderHint");
                    }
                    if (mBoundingBoxBones == null)
                    {
                        fields.Remove("BoundingBoxBones");
                    }
                    return fields;
                }
            }

            public override int RecommendedApiVersion { get { return kRecommendedApiVersion; } }
            #endregion

            #region IEquatable<Mesh>
            public bool Equals(Mesh other)
            {
                if (other == null) return false;

                return mName.Equals(other.mName)
                    && mMaterialIndex.Equals(other.mMaterialIndex)
                    && mVertexFormatIndex.Equals(other.mVertexFormatIndex)
                    && mVertexBufferIndex.Equals(other.mVertexBufferIndex)
                    && mIndexBufferIndex.Equals(other.mIndexBufferIndex)
                    && mPrimitiveType.Equals(other.mPrimitiveType)
                    && mFlags.Equals(other.mFlags)
                    && mStreamOffset.Equals(other.mStreamOffset)
                    && mStartVertex.Equals(other.mStartVertex)
                    && mStartIndex.Equals(other.mStartIndex)
                    && mMinVertexIndex.Equals(other.mMinVertexIndex)
                    && mVertexCount.Equals(other.mVertexCount)
                    && mPrimitiveCount.Equals(other.mPrimitiveCount)
                    && mSkinControllerIndex.Equals(other.mSkinControllerIndex)
                    && mMeshMaterialIndex.Equals(other.mMeshMaterialIndex)
                    && mJointReferences.Equals(other.mJointReferences)
                    && mBounds.Equals(other.mBounds)
                    && mGeometryStates.Equals(other.mGeometryStates)
                    && mParentBoneName.Equals(other.mParentBoneName)
                    && mMirrorPlane.Equals(other.mMirrorPlane)
                    && mSortOrderHint.Equals(other.mSortOrderHint)
                    && mOwner.Equals(other.mOwner)
                    ;
            }

            public override bool Equals(object obj) { return this.Equals(obj as Mesh); }

            public override int GetHashCode()
            {
                return mName.GetHashCode()
                    ^ mMaterialIndex.GetHashCode()
                    ^ mVertexFormatIndex.GetHashCode()
                    ^ mVertexBufferIndex.GetHashCode()
                    ^ mIndexBufferIndex.GetHashCode()
                    ^ mPrimitiveType.GetHashCode()
                    ^ mFlags.GetHashCode()
                    ^ mStreamOffset.GetHashCode()
                    ^ mStartVertex.GetHashCode()
                    ^ mStartIndex.GetHashCode()
                    ^ mMinVertexIndex.GetHashCode()
                    ^ mVertexCount.GetHashCode()
                    ^ mPrimitiveCount.GetHashCode()
                    ^ mSkinControllerIndex.GetHashCode()
                    ^ mMeshMaterialIndex.GetHashCode()
                    ^ mJointReferences.GetHashCode()
                    ^ mBounds.GetHashCode()
                    ^ mGeometryStates.GetHashCode()
                    ^ mParentBoneName.GetHashCode()
                    ^ mMirrorPlane.GetHashCode()
                    ^ mSortOrderHint.GetHashCode()
                    ^ mOwner.GetHashCode()
                    ;
            }
            #endregion

            internal MLOD Owner
            {
                get { return mOwner; }
                set
                {
                    if (mOwner != value)
                    {
                        uint oldVersion = mOwner.Version;
                        mOwner = value;
                        if (mOwner.Version != oldVersion)
                            OnElementChanged();
                    }
                }
            }

            internal bool IsShadowCaster { get { return (mFlags & MeshFlags.ShadowCaster) != 0; } }

            #region ContentFields
            [ElementPriority(1)]
            public UInt32 Name
            {
                get { return mName; }
                set { if (mName != value) { mName = value; OnElementChanged(); } }
            }
            [ElementPriority(2)]
            public GenericRCOLResource.ChunkReference MaterialIndex
            {
                get { return mMaterialIndex; }
                set { if (mMaterialIndex != value) { mMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public GenericRCOLResource.ChunkReference VertexFormatIndex
            {
                get { return mVertexFormatIndex; }
                set { if (mVertexFormatIndex != value) { mVertexFormatIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(4)]
            public GenericRCOLResource.ChunkReference VertexBufferIndex
            {
                get { return mVertexBufferIndex; }
                set { if (mVertexBufferIndex != value) { mVertexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(5)]
            public GenericRCOLResource.ChunkReference IndexBufferIndex
            {
                get { return mIndexBufferIndex; }
                set { if (mIndexBufferIndex != value) { mIndexBufferIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(6)]
            public ModelPrimitiveType PrimitiveType
            {
                get { return mPrimitiveType; }
                set { if (mPrimitiveType != value) { mPrimitiveType = value; OnElementChanged(); } }
            }
            [ElementPriority(7)]
            public MeshFlags Flags
            {
                get { return mFlags; }
                set { if (mFlags != value) { mFlags = value; OnElementChanged(); } }
            }
            [ElementPriority(8)]
            public UInt32 StreamOffset
            {
                get { return mStreamOffset; }
                set { if (mStreamOffset != value) { mStreamOffset = value; OnElementChanged(); } }
            }
            [ElementPriority(9)]
            public Int32 StartVertex
            {
                get { return mStartVertex; }
                set { if (mStartVertex != value) { mStartVertex = value; OnElementChanged(); } }
            }
            [ElementPriority(10)]
            public Int32 StartIndex
            {
                get { return mStartIndex; }
                set { if (mStartIndex != value) { mStartIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(11)]
            public Int32 MinVertexIndex
            {
                get { return mMinVertexIndex; }
                set { if (mMinVertexIndex != value) { mMinVertexIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(12)]
            public Int32 VertexCount
            {
                get { return mVertexCount; }
                set { if (mVertexCount != value) { mVertexCount = value; OnElementChanged(); } }
            }
            [ElementPriority(13)]
            public Int32 PrimitiveCount
            {
                get { return mPrimitiveCount; }
                set { if (mPrimitiveCount != value) { mPrimitiveCount = value; OnElementChanged(); } }
            }
            [ElementPriority(14)]
            public BoundingBox Bounds
            {
                get { return mBounds; }
                set { if (mBounds != value) { mBounds = value; OnElementChanged(); } }
            }
            [ElementPriority(15)]
            public GenericRCOLResource.ChunkReference SkinControllerIndex
            {
                get { return mSkinControllerIndex; }
                set { if (mSkinControllerIndex != value) { mSkinControllerIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(16)]
            public GenericRCOLResource.ChunkReference MeshMaterialIndex
            {
                get { return mMeshMaterialIndex; }
                set { if (mMeshMaterialIndex != value) { mMeshMaterialIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } }
            }
            [ElementPriority(17)]
            public UIntList JointReferences
            {
                get { return mJointReferences; }
                set { if (mJointReferences != value) { mJointReferences = value == null ? null : new UIntList(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(18)]
            public GeometryStateList GeometryStates
            {
                get { return mGeometryStates; }
                set { if (mGeometryStates != value) { mGeometryStates = value == null ? null : new GeometryStateList(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(19)]
            public UInt32 ParentBoneName
            {
                get { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); return mParentBoneName; }
                set { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); if (mParentBoneName != value) { mParentBoneName = value; OnElementChanged(); } }
            }
            [ElementPriority(20)]
            public Vector4 MirrorPlane
            {
                get { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); return mMirrorPlane; }
                set { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); if (mMirrorPlane != value) { mMirrorPlane = value; OnElementChanged(); } }
            }
            [ElementPriority(21)]
            public UInt32 SortOrderHint
            {
                get { if (mOwner.Version < 0x0204) throw new InvalidOperationException(); return mSortOrderHint; }
                set { if (mOwner.Version < 0x0204) throw new InvalidOperationException(); if (mSortOrderHint != value) { mSortOrderHint = value; OnElementChanged(); } }
            }
            [ElementPriority(22)]
            public BoundingBox[] BoundingBoxBones
            {
                get { if (mOwner.Version < 0x0206) throw new InvalidOperationException(); return mBoundingBoxBones; }
                set { if (mOwner.Version < 0x0206) throw new InvalidOperationException(); if (mBoundingBoxBones != value) { mBoundingBoxBones = value; OnElementChanged(); } }
            }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class MeshList : DependentList<Mesh>
        {
            private MLOD mOwner;

            public MeshList(EventHandler handler, MLOD owner) : base(handler) { mOwner = owner; }
            public MeshList(EventHandler handler, MLOD owner, Stream s) : this(handler, owner) { Parse(s); }
            public MeshList(EventHandler handler, MLOD owner, IEnumerable<Mesh> ilt)
                : base(null)
            {
                mOwner = owner;
                elementHandler = handler;
                foreach (var t in ilt)
                {
                    base.Add(t);
                    t.Owner = mOwner;
                }
                this.handler = handler;
            }

            protected override Mesh CreateElement(Stream s) { return new Mesh(0, handler, mOwner, s); }
            protected override void WriteElement(Stream s, Mesh element) { element.UnParse(s); }

            public override void Add() { base.Add(new Mesh(0, handler, mOwner)); }
            public override void Add(Mesh item) { item.Owner = mOwner; base.Add(item); }
            //And, of course, all the other ways of getting an element into a list... bad, bad, bad...  But this covers s3pe's usage.
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version
        {
            get { return mVersion; }
            set { if (mVersion != value) { mVersion = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }

        [ElementPriority(12)]
        public MeshList Meshes
        {
            get { return mMeshes; }
            set { if (mMeshes != value) { mMeshes = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }

    #region Some flags here for some reason
    [Flags]
    public enum MeshFlags : uint
    {
        BasinInterior = 0x00000001,
        HDExteriorLit = 0x00000002,
        PortalSide = 0x00000004,
        DropShadow = 0x00000008,
        ShadowCaster = 0x00000010,
        Foundation = 0x00000020,
        Pickable = 0x00000040

    }
    public enum ModelPrimitiveType
    {
        PointList,
        LineList,
        LineStrip,
        TriangleList,
        TriangleFan,
        TriangleStrip,
        RectList,
        QuadList,
        DisplayList
    }
    #endregion
}