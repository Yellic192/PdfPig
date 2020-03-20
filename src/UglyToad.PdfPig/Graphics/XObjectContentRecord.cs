namespace UglyToad.PdfPig.Graphics
{
    using Core;
    using PdfPig.Core;
    using System;
    using Tokens;
    using UglyToad.PdfPig.Core.Graphics.Colors;
    using Util.JetBrains.Annotations;
    using XObjects;

    internal class XObjectContentRecord
    {
        public XObjectType Type { get; }

        [NotNull]
        public StreamToken Stream { get; }

        public TransformationMatrix AppliedTransformation { get; }

        public RenderingIntent DefaultRenderingIntent { get; }

        public ColorSpace DefaultColorSpace { get; }

        public XObjectContentRecord(XObjectType type, StreamToken stream, TransformationMatrix appliedTransformation,
            RenderingIntent defaultRenderingIntent,
            ColorSpace defaultColorSpace)
        {
            Type = type;
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            AppliedTransformation = appliedTransformation;
            DefaultRenderingIntent = defaultRenderingIntent;
            DefaultColorSpace = defaultColorSpace;
        }
    }
}
