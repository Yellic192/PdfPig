namespace UglyToad.PdfPig.Graphics.Operations.PathConstruction
{
    using System.IO;
    using PdfPig.Core;

    /// <inheritdoc />
    /// <summary>
    /// Begin a new subpath by moving the current point to coordinates (x, y), omitting any connecting line segment.
    /// </summary>
    public class BeginNewSubpath : IGraphicsStateOperation
    {
        /// <summary>
        /// The symbol for this operation in a stream.
        /// </summary>
        public const string Symbol = "m";

        /// <inheritdoc />
        public string Operator => Symbol;

        /// <summary>
        /// The x coordinate for the subpath to begin at.
        /// </summary>
        public decimal X { get; }

        /// <summary>
        /// The y coordinate for the subpath to begin at.
        /// </summary>
        public decimal Y { get; }

        /// <summary>
        /// Create a new <see cref="BeginNewSubpath"/>.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public BeginNewSubpath(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc />
        public void Run(IOperationContext operationContext)
        {
            // store previous stroking info
            var stroked = false;
            if (operationContext.CurrentPath != null && !operationContext.CurrentPath.IsClipping)
            {
                stroked = operationContext.CurrentPath.IsStroked;
            }
            else if (operationContext.Paths.Count >= 1)
            {
                var previousPath = operationContext.Paths[operationContext.Paths.Count - 1];
                if (!previousPath.IsClipping)
                {
                    stroked = previousPath.IsStroked;
                }
                else
                {
                    System.Console.WriteLine("previous path is clipping.");
                    if (operationContext.Paths.Count >= 2)
                    {
                        previousPath = operationContext.Paths[operationContext.Paths.Count - 2];
                        if (!previousPath.IsClipping)
                        {
                            stroked = previousPath.IsStroked;
                        }
                    }
                }
            }

            var point = new PdfPoint(X, Y);
            operationContext.BeginSubpath();
            var pointTransform = operationContext.CurrentTransformationMatrix.Transform(point);
            operationContext.CurrentPosition = pointTransform;
            operationContext.CurrentPath.MoveTo(pointTransform.X, pointTransform.Y);

            // apply stroke info
            operationContext.CurrentPath.IsStroked = stroked;
        }

        /// <inheritdoc />
        public void Write(Stream stream)
        {
            stream.WriteDecimal(X);
            stream.WriteWhiteSpace();
            stream.WriteDecimal(Y);
            stream.WriteWhiteSpace();
            stream.WriteText(Symbol);
            stream.WriteNewLine();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{X} {Y} {Symbol}";
        }
    }
}