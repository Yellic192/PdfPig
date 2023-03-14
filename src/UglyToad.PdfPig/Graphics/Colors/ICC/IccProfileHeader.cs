namespace UglyToad.PdfPig.Graphics.Colors.ICC
{
    using System;
    using UglyToad.PdfPig.Graphics.Colors.ICC.Tags;

    /// <summary>
    /// ICC profile header.
    /// </summary>
    public struct IccProfileHeader
    {
        /// <summary>
        /// Profile size.
        /// </summary>
        public uint ProfileSize { get; internal set; }

        /// <summary>
        /// Profile major version.
        /// </summary>
        public int VersionMajor { get; internal set; }

        /// <summary>
        /// Profile minor version.
        /// </summary>
        public int VersionMinor { get; internal set; }

        /// <summary>
        /// Profile bug fix version.
        /// </summary>
        public int VersionBugFix { get; internal set; }

        /// <summary>
        /// Preferred CMM type.
        /// </summary>
        public object Cmm { get; internal set; }

        /// <summary>
        /// Profile/Device class.
        /// </summary>
        public IccProfileClass ProfileClass { get; internal set; }

        /// <summary>
        /// Colour space of data (possibly a derived space).
        /// </summary>
        public IccColourSpaceType ColourSpace { get; internal set; }

        /// <summary>
        /// PCS.
        /// </summary>
        public object Pcs { get; internal set; }

        /// <summary>
        /// Date and time this profile was first created.
        /// </summary>
        public DateTime Created { get; internal set; }

        /// <summary>
        /// profile file signature.
        /// </summary>
        public string ProfileSignature { get; internal set; }

        /// <summary>
        /// Primary platform signature.
        /// </summary>
        public IccPrimaryPlatforms PrimaryPlatformSignature { get; internal set; }

        /// <summary>
        /// Profile flags to indicate various options for the CMM such as distributed
        /// processing and caching options.
        /// </summary>
        public object ProfileFlags { get; internal set; }

        /// <summary>
        /// Device manufacturer of the device for which this profile is created.
        /// </summary>
        public object DeviceManufacturer { get; internal set; }

        /// <summary>
        /// Device model of the device for which this profile is created.
        /// </summary>
        public object DeviceModel { get; internal set; }

        /// <summary>
        /// Device attributes unique to the particular device setup such as media type.
        /// </summary>
        public object DeviceAttributes { get; internal set; }

        /// <summary>
        /// Rendering Intent.
        /// </summary>
        public IccRenderingIntent RenderingIntent { get; internal set; }

        /// <summary>
        /// The nCIEXYZ values of the illuminant of the PCS.
        /// </summary>
        public IccXyzType nCIEXYZ { get; internal set; }

        /// <summary>
        /// Profile creator signature.
        /// </summary>
        public object ProfileCreatorSignature { get; internal set; }

        /// <summary>
        /// Profile ID.
        /// </summary>
        public object ProfileId { get; internal set; }

        /*
        /// <summary>
        /// Bytes reserved for future expansion and shall be set to zero (00h).
        /// </summary>
        public byte[] Reserved { get; internal set; }
        */

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{VersionMajor}.{VersionMinor}.{VersionBugFix}";
        }
    }

    /// <summary>
    /// There are three basic classes of device profiles, which are Input, Display and Output. In addition to the three
    /// basic device profile classes, four additional colour processing profiles are defined. These profiles provide a
    /// standard implementation for use by the CMM in general colour processing, or for the convenience of CMMs
    /// which may use these types to store calculated transforms.These four additional profile classes are DeviceLink,
    /// ColorSpace, Abstract and NamedColor.
    /// </summary>
    public enum IccProfileClass
    {
        /// <summary>
        /// Input device profile (‘scnr’).
        /// </summary>
        InputDeviceProfile,

        /// <summary>
        /// Display device profile (‘mntr’).
        /// </summary>
        DisplayDeviceProfile,

        /// <summary>
        /// Output device profile (‘prtr’).
        /// </summary>
        OutputDeviceProfile,

        /// <summary>
        /// DeviceLink profile (‘link’).
        /// </summary>
        DeviceLinkProfile,

        /// <summary>
        /// ColorSpace profile (‘spac’).
        /// </summary>
        ColorSpaceProfile,

        /// <summary>
        /// Abstract profile (‘abst’).
        /// </summary>
        AbstractProfile,

        /// <summary>
        /// NamedColor profile (‘nmcl’).
        /// </summary>
        NamedColorProfile
    }

    /// <summary>
    /// This field shall contain the signature of the data colour space expected on the A side (device side) of the profile
    /// transforms. The names and signatures of the permitted data colour spaces are shown in Table 19. Signatures
    /// are left justified.
    /// </summary>
    public enum IccColourSpaceType
    {
        /// <summary>
        /// nCIEXYZ or PCSXYZ.
        /// </summary>
        nCIEXYZorPCSXYZ,

        /// <summary>
        /// CIELAB or PCSLAB.
        /// </summary>
        CIELABorPCSLAB,

        /// <summary>
        /// CIELUV.
        /// </summary>
        CIELUV,

        /// <summary>
        /// YCbCr.
        /// </summary>
        YCbCr,

        /// <summary>
        /// CIEYxy.
        /// </summary>
        CIEYxy,

        /// <summary>
        /// RGB.
        /// </summary>
        RGB,

        /// <summary>
        /// Gray.
        /// </summary>
        Gray,

        /// <summary>
        /// HSV.
        /// </summary>
        HSV,

        /// <summary>
        /// HLS.
        /// </summary>
        HLS,

        /// <summary>
        /// CMYK.
        /// </summary>
        CMYK,

        /// <summary>
        /// CMY.
        /// </summary>
        CMY,

        /// <summary>
        /// 2 colour.
        /// </summary>
        Colour2,

        /// <summary>
        /// 3 colour (other than those listed above).
        /// </summary>
        Colour3,

        /// <summary>
        /// 4 colour (other than CMYK).
        /// </summary>
        Colour4,

        /// <summary>
        /// 5 colour.
        /// </summary>
        Colour5,

        /// <summary>
        /// 6 colour.
        /// </summary>
        Colour6,

        /// <summary>
        /// 7 colour.
        /// </summary>
        Colour7,

        /// <summary>
        /// 8 colour.
        /// </summary>
        Colour8,

        /// <summary>
        /// 9 colour.
        /// </summary>
        Colour9,

        /// <summary>
        /// 10 colour.
        /// </summary>
        Colour10,

        /// <summary>
        /// 11 colour.
        /// </summary>
        Colour11,

        /// <summary>
        /// 12 colour.
        /// </summary>
        Colour12,

        /// <summary>
        /// 13 colour.
        /// </summary>
        Colour13,

        /// <summary>
        /// 14 colour.
        /// </summary>
        Colour14,

        /// <summary>
        /// 15 colour.
        /// </summary>
        Colour15,
    }

    /// <summary>
    /// The rendering intent field shall specify the rendering intent which should be used (or, in the case of a DeviceLink
    /// profile, was used) when this profile is (was) combined with another profile. In a sequence of more than two
    /// profiles, it applies to the combination of this profile and the next profile in the sequence and not to the entire
    /// sequence. Typically, the user or application will set the rendering intent dynamically at runtime or embedding
    /// time. Therefore, this flag may not have any meaning until the profile is used in some context, e.g. in a DeviceLink
    /// or an embedded source profile
    /// </summary>
    public enum IccRenderingIntent : uint
    {
        /// <summary>
        /// Perceptual.
        /// </summary>
        Perceptual = 0,

        /// <summary>
        /// Media-relative colorimetric.
        /// </summary>
        MediaRelativeColorimetric = 1,

        /// <summary>
        /// Saturation.
        /// </summary>
        Saturation = 2,

        /// <summary>
        /// ICC-absolute colorimetric.
        /// </summary>
        IccAbsoluteColorimetric = 3
    }

    /// <summary>
    /// Identify the primary platform/operating system framework for which the profile was created.
    /// </summary>
    public enum IccPrimaryPlatforms
    {
        /// <summary>
        /// f there is no primary platform identified, this field shall be set to zero (00000000h)
        /// </summary>
        Unidentified,

        /// <summary>
        /// Apple Computer, Inc.
        /// </summary>
        AppleComputer,

        /// <summary>
        /// Microsoft Corporation.
        /// </summary>
        MicrosoftCorporation,

        /// <summary>
        /// Silicon Graphics, Inc.
        /// </summary>
        SiliconGraphics,

        /// <summary>
        /// Sun Microsystems, Inc.
        /// </summary>
        SunMicrosystems
    }
}
