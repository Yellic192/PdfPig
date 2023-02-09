namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using static UglyToad.PdfPig.Functions.Type4.Parser;
    using System.Xml.Linq;

    /**
    * Parser for PDF Type 4 functions. This implements a small subset of the PostScript
    * language but is no full PostScript interpreter.
    *
    */
    public class Parser
    {
        /** Used to indicate the parsers current state. */
        internal enum State
        {
            NEWLINE, WHITESPACE, COMMENT, TOKEN
        }

        private Parser()
        {
            //nop
        }

        /**
         * Parses a Type 4 function and sends the syntactic elements to the given
         * syntax handler.
         * @param input the text source
         * @param handler the syntax handler
         */
        public static void parse(string input, SyntaxHandler handler)
        {
            Tokenizer tokenizer = new Tokenizer(input, handler);
            tokenizer.tokenize();
        }

        /**
         * This interface defines all possible syntactic elements of a Type 4 function.
         * It is called by the parser as the function is interpreted.
         */
        public interface SyntaxHandler
        {
            /**
             * Indicates that a new line starts.
             * @param text the new line character (CR, LF, CR/LF or FF)
             */
            void newLine(string text);

            /**
             * Called when whitespace characters are encountered.
             * @param text the whitespace text
             */
            void whitespace(string text);

            /**
             * Called when a token is encountered. No distinction between operators and values
             * is done here.
             * @param text the token text
             */
            void token(string text);

            /**
             * Called for a comment.
             * @param text the comment
             */
            void comment(string text);
        }

        /**
         * Abstract base class for a {@link SyntaxHandler}.
         */
        public class AbstractSyntaxHandler : SyntaxHandler
        {
            /** {@inheritDoc} */
            public void comment(string text)
            {
                //nop
            }

            /** {@inheritDoc} */
            public void newLine(string text)
            {
                //nop
            }

            /** {@inheritDoc} */
            public void whitespace(string text)
            {
                //nop
            }

            /// <summary>
            /// TODO
            /// </summary>
            public virtual void token(string text)
            {
                throw new NotImplementedException();
            }
        }

        /**
         * Tokenizer for Type 4 functions.
         */
        internal class Tokenizer
        {
            private const char NUL = '\u0000'; //NUL
            private const char EOT = '\u0004'; //END OF TRANSMISSION
            private const char TAB = '\u0009'; //TAB CHARACTER
            private const char FF = '\u000C'; //FORM FEED
            private const char CR = '\r'; //CARRIAGE RETURN
            private const char LF = '\n'; //LINE FEED
            private const char SPACE = '\u0020'; //SPACE

            private readonly string input;
            private int index;
            private readonly SyntaxHandler handler;
            private State state = State.WHITESPACE;
            private readonly StringBuilder buffer = new StringBuilder();

            internal Tokenizer(string text, SyntaxHandler syntaxHandler)
            {
                this.input = text;
                this.handler = syntaxHandler;
            }

            private bool hasMore()
            {
                return index < input.Length;
            }

            private char currentChar()
            {
                return input[index];
            }

            private char nextChar()
            {
                index++;
                if (!hasMore())
                {
                    return EOT;
                }
                else
                {
                    return currentChar();
                }
            }

            private char peek()
            {
                if (index < input.Length - 1)
                {
                    return input[index + 1];
                }
                else
                {
                    return EOT;
                }
            }

            private State nextState()
            {
                char ch = currentChar();
                switch (ch)
                {
                    case CR:
                    case LF:
                    case FF: //FF
                        state = State.NEWLINE;
                        break;
                    case NUL:
                    case TAB:
                    case SPACE:
                        state = State.WHITESPACE;
                        break;
                    case '%':
                        state = State.COMMENT;
                        break;
                    default:
                        state = State.TOKEN;
                        break;
                }
                return state;
            }

            internal void tokenize()
            {
                while (hasMore())
                {
                    buffer.Length = 0; // buffer.setLength(0);
                    nextState();
                    switch (state)
                    {
                        case State.NEWLINE:
                            scanNewLine();
                            break;
                        case State.WHITESPACE:
                            scanWhitespace();
                            break;
                        case State.COMMENT:
                            scanComment();
                            break;
                        default:
                            scanToken();
                            break;
                    }
                }
            }

            private void scanNewLine()
            {
                System.Diagnostics.Debug.Assert(state == State.NEWLINE);
                char ch = currentChar();
                buffer.Append(ch);
                if (ch == CR && peek() == LF)
                {
                    //CRLF is treated as one newline
                    buffer.Append(nextChar());
                }
                handler.newLine(buffer.ToString());
                nextChar();
            }

            private void scanWhitespace()
            {
                System.Diagnostics.Debug.Assert(state == State.WHITESPACE);
                buffer.Append(currentChar());

                //loop:
                bool loop = true;
                while (hasMore() && loop)
                {
                    char ch = nextChar();
                    switch (ch)
                    {
                        case NUL:
                        case TAB:
                        case SPACE:
                            buffer.Append(ch);
                            break;
                        default:
                            loop = false;
                            break; // loop;
                    }
                }
                handler.whitespace(buffer.ToString());
            }

            private void scanComment()
            {
                System.Diagnostics.Debug.Assert(state == State.COMMENT);
                buffer.Append(currentChar());

                //loop:
                bool loop = true;
                while (hasMore() && loop)
                {
                    char ch = nextChar();
                    switch (ch)
                    {
                        case CR:
                        case LF:
                        case FF:
                            loop = false;
                            break; // loop;
                        default:
                            buffer.Append(ch);
                            break;
                    }
                }
                //EOF reached
                handler.comment(buffer.ToString());
            }

            private void scanToken()
            {
                System.Diagnostics.Debug.Assert(state == State.TOKEN);
                char ch = currentChar();
                buffer.Append(ch);
                switch (ch)
                {
                    case '{':
                    case '}':
                        handler.token(buffer.ToString());
                        nextChar();
                        return;
                    default:
                        //continue
                        break;
                }

                //loop:
                bool loop = true;
                while (hasMore() && loop)
                {
                    ch = nextChar();
                    switch (ch)
                    {
                        case NUL:
                        case TAB:
                        case SPACE:
                        case CR:
                        case LF:
                        case FF:
                        case EOT:
                        case '{':
                        case '}':
                            loop = false;
                            break; // loop;

                        default:
                            buffer.Append(ch);
                            break;
                    }
                }
                //EOF reached
                handler.token(buffer.ToString());
            }
        }
    }
}
