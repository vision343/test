﻿/***************************************************************************
 *  Copyright (C) 2014, 2015 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Keyi Zhang, kz005@bucknell.edu                                         *
 *  Snaitf                                                                 *
 *  Cmar                                                                   *
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

using s4pi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace CASPartResource
{
    public class Sculpt : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s4pi.Settings.Settings.Checking;

        #region Attributes
        private uint contextVersion;
        private TGIBlock[] publicKey { get; set; }
        private TGIBlock[] externalKey { get; set; }
        private TGIBlockList delayLoadKey { get; set; }
        private SculptBlock[] sculpts { get; set; }
        #endregion


        public Sculpt(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            this.contextVersion = r.ReadUInt32();
            uint publicKeyCount = r.ReadUInt32();
            uint externalKeyCount = r.ReadUInt32();
            uint delayLoadKeyCount = r.ReadUInt32();
            uint objectCount = r.ReadUInt32();
            this.publicKey = new TGIBlock[publicKeyCount];
            for (int i = 0; i < publicKeyCount; i++)
            {
                this.publicKey[i] = new TGIBlock(recommendedApiVersion, OnResourceChanged, "ITG", s);
            }
            this.externalKey = new TGIBlock[externalKeyCount];
            for (int i = 0; i < externalKeyCount; i++)
            {
                this.externalKey[i] = new TGIBlock(recommendedApiVersion, OnResourceChanged, "ITG", s);
            }
            this.delayLoadKey = new TGIBlockList(OnResourceChanged);
            for (int i = 0; i < delayLoadKeyCount; i++)
            {
                this.delayLoadKey.Add(new TGIBlock(recommendedApiVersion, OnResourceChanged, "ITG", s));
            }
            this.sculpts = new SculptBlock[objectCount];
            for (int i = 0; i < objectCount; i++)
            {
                this.sculpts[i] = new SculptBlock(recommendedApiVersion, OnResourceChanged, s);
            }
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(this.contextVersion);
            w.Write(this.publicKey.Length);
            w.Write(this.externalKey.Length);
            w.Write(this.delayLoadKey.Count);
            w.Write(this.sculpts.Length);

            for (int i = 0; i < publicKey.Length; i++)
            {
                this.publicKey[i].UnParse(ms);
            }
            for (int i = 0; i < externalKey.Length; i++)
            {
                this.externalKey[i].UnParse(ms);
            }
            for (int i = 0; i < delayLoadKey.Count; i++)
            {
                w.Write(this.delayLoadKey[i].Instance);
                w.Write(this.delayLoadKey[i].ResourceType);
                w.Write(this.delayLoadKey[i].ResourceGroup);
            }
            for (int i = 0; i < sculpts.Length; i++)
            {
                this.sculpts[i].UnParse(ms);
            }
            ms.Position = 0;
            return ms;
        }

        #endregion

        #region Sub-Class

        public class SculptBlock : AHandlerElement, IEquatable<SculptBlock>
        {
            uint unknown1;
            uint unknown2;
            uint unknown3;
            AgeGenderFlags ageGender;
            SimRegion region;
            uint unknown5;
            LinkTags linkTag;
            TGIBlock textureRef;
            TGIBlock specularRef;
            TGIBlock imageRef;
            byte unknown7;
            TGIBlock[] dmapRef;
            byte[] unknown8;

            public SculptBlock(int apiVersion, EventHandler handler) : base(apiVersion, handler) { this.UnParse(new MemoryStream()); }

            public SculptBlock(int apiVersion, EventHandler handler, Stream s)
                : base(apiVersion, handler)
            {
                BinaryReader r = new BinaryReader(s);
                this.unknown1 = r.ReadUInt32();
                this.unknown2 = r.ReadUInt32();
                this.unknown3 = r.ReadUInt32();
                this.ageGender = (AgeGenderFlags)r.ReadUInt32();
                this.region = (SimRegion)r.ReadUInt32();
                this.unknown5 = r.ReadUInt32();
                this.linkTag = (LinkTags)r.ReadUInt32();
                this.textureRef = new TGIBlock(RecommendedApiVersion, handler, "ITG", s);
                this.specularRef = new TGIBlock(RecommendedApiVersion, handler, "ITG", s);
                this.imageRef = new TGIBlock(RecommendedApiVersion, handler, "ITG", s);
                this.unknown7 = r.ReadByte();
                this.dmapRef = new TGIBlock[2];
                for (int i = 0; i < this.dmapRef.Length; i++) this.dmapRef[i] = new TGIBlock(RecommendedApiVersion, handler, "ITG", s);
                this.unknown8 = r.ReadBytes(20);
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.unknown1);
                w.Write(this.unknown2);
                w.Write(this.unknown3);
                w.Write((uint)this.ageGender);
                w.Write((uint)this.region);
                w.Write(this.unknown5);
                w.Write((uint)this.linkTag);
                this.textureRef.UnParse(s);
                this.specularRef.UnParse(s);
                this.imageRef.UnParse(s);
                w.Write(this.unknown7);
                if (this.dmapRef == null) this.dmapRef = new TGIBlock[0];
                for (int i = 0; i < this.dmapRef.Length; i++) this.dmapRef[i].UnParse(s);
                w.Write(this.unknown8);
            }

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public bool Equals(SculptBlock other)
            {
                return this.unknown1 == other.unknown1 && this.unknown2 == other.unknown2 && this.unknown3 == other.unknown3 &&
                    this.ageGender == other.ageGender && this.region == other.region && this.unknown5 == other.unknown5 &&
                    this.linkTag == other.linkTag && this.textureRef == other.textureRef && this.specularRef == other.specularRef &&
                    this.imageRef == other.imageRef && this.unknown7 == other.unknown7 && this.dmapRef == other.dmapRef &&
                    this.unknown8 == other.unknown8;
            }

            #region Content-Field
            [ElementPriority(0)]
            public uint Unknown1 { get { return this.unknown1; } set { if (!value.Equals(this.unknown1)) { this.unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(1)]
            public uint Unknown2 { get { return this.unknown2; } set { if (!value.Equals(this.unknown2)) { this.unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown3 { get { return this.unknown3; } set { if (!value.Equals(this.unknown3)) { this.unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public AgeGenderFlags AgeGender { get { return this.ageGender; } set { if (!value.Equals(this.ageGender)) { this.ageGender = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public SimRegion Region { get { return this.region; } set { if (!value.Equals(this.region)) { this.region = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public uint Unknown5 { get { return this.unknown5; } set { if (!value.Equals(this.unknown5)) { this.unknown5 = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public LinkTags LinkTag { get { return this.linkTag; } set { if (!value.Equals(this.linkTag)) { this.linkTag = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public TGIBlock TextureRef { get { return this.textureRef; } set { if (!value.Equals(this.textureRef)) { this.textureRef = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public TGIBlock SpecularRef { get { return this.specularRef; } set { if (!value.Equals(this.specularRef)) { this.specularRef = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public TGIBlock ImageRef { get { return this.imageRef; } set { if (!value.Equals(this.imageRef)) { this.imageRef = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public byte Unknown7 { get { return this.unknown7; } set { if (!value.Equals(this.unknown7)) { this.unknown7 = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public TGIBlock[] DeformerMapRef { get { return this.dmapRef; } set { if (!value.Equals(this.dmapRef)) { this.dmapRef = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public byte[] Unknown8 { get { return this.unknown8; } set { if (!value.Equals(this.unknown8)) { this.unknown8 = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        #endregion

        #region Content-Field
        [ElementPriority(0)]
        public uint ContextVersion { get { return this.contextVersion; } set { if (!value.Equals(this.contextVersion)) { OnResourceChanged(this, EventArgs.Empty); this.contextVersion = value; } } }
        [ElementPriority(1)]
        public TGIBlock[] PublicKey { get { return this.publicKey; } set { if (!value.Equals(this.publicKey)) { OnResourceChanged(this, EventArgs.Empty); this.publicKey = value; } } }
        [ElementPriority(2)]
        public TGIBlock[] ExternalKey { get { return this.externalKey; } set { if (!value.Equals(this.externalKey)) { OnResourceChanged(this, EventArgs.Empty); this.externalKey = value; } } }
        [ElementPriority(3)]
        public TGIBlockList BlendGeometry_Key { get { return this.delayLoadKey; } set { if (!value.Equals(this.delayLoadKey)) { OnResourceChanged(this, EventArgs.Empty); this.delayLoadKey = value; } } }
        [ElementPriority(4)]
        public SculptBlock[] SculptsBlocks { get { return this.sculpts; } set { if (!value.Equals(this.sculpts)) { OnResourceChanged(this, EventArgs.Empty); this.sculpts = value; } } }
        #endregion

        public string Value { get { return ValueBuilder; } }
    }

    /// <summary>
    /// ResourceHandler for Sculpt wrapper
    /// </summary>
    public class SculptResourceHandler : AResourceHandler
    {
        public SculptResourceHandler()
        {
            this.Add(typeof(Sculpt), new List<string>(new string[] { "0x9D1AB874", }));
        }
    }
}