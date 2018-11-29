using System;
using System.IO;
using System.Globalization;
using System.Text;
using System.Xml;

namespace WFCTestLib.Logging
{
    //
    //  Supports basic functions to write information to a file in XML format.
    //  This is basically a wrapper around System.Xml.XmlTextWriter.
    //
    //  We do some custom stuff to support our own style of indentation.
    //  XmlTextWriter apparently won't indent for you if you write whitespace.
    //
    //  This code basically copied and modified from the old
    //  WFCTestLib.Logging.XmlWriter class.
    //
    public class XmlLogWriter
    {

		public void WriteRaw(string s)
		{
			writer.WriteRaw(s);
		}
		private const int INDENT = 4;       //  number of spaces to indent
        private int indent = 0;             //  indenting level
        private bool needsIndenting = true; //  true if the data about to be written needs to be indented first

        // We need one for the log file and one for the console.  I don't know of any way to
        // write to two streams at the same time.
        private XmlTextWriter writer;
        private XmlTextWriter consoleWriter;

		private static string path = "\\\\mdfile\\whidbey\\Files\\Core\\TeamData\\Client\\vss\\shadow\\WinFormsRuntime\\Libs\\WFCTestLib\\Log\\NetClientRunTimeStyle.xsl";
		private string xsl_string = "\r\n<?xml-stylesheet  type=\"text/xsl\" href=\"" + path + "\"?>\r\n";

        private readonly string newLine;

        // <doc>
        // <desc>
        //   Constructs a new XMLWriter object. The file replaces any previous file (if exisiting).
        //   Data is written in ASCII.
        // </desc>
        // <param term="fileName">
        //   The name of the file to write
        // </param>
        // <seealso class="System.IO.StreamWriter"/>
        // </doc>
        public XmlLogWriter(String fileName) {
            StreamWriter sw = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
            sw.AutoFlush = true;
            newLine = sw.NewLine;

            writer = new XmlTextWriter(sw);
            consoleWriter = new XmlTextWriter(Console.Out);

            writer.WriteStartDocument();
            consoleWriter.WriteStartDocument();

			writer.WriteRaw(xsl_string);
			consoleWriter.WriteRaw(xsl_string);

            WriteLine();
        }

        // <doc>
        // <desc>
        //   Writes out a tag with optional attributes and data to the XML stream.
        // </desc>
        // <param term="tagName">
        //   The name of the tag to write. For instance in the tag <Testcase name="Foo">,
        //   the tagName would be "Testcase".
        // </param>
        // <param term="closeToo">
        //   Specifies whether the tag being written is to be closed as well. If this parameter
        //   is False, the user is responsible for calling "CloseTag" to close the tag themselves.
        // </param>
        // <param term="elementText">
        //   The contents of the data to be written after the tag. For instance, in the tag:
        //   <Testcase name="Foo">The contents goes here...
        //   If the "closeToo" parameter is true, this parameter is ignored.
        // </param>
        // <param term="data">
        //   The data contained within the tag. For instance, in the tag <Testcase name="Foo" bar="Bletch">,
        //   the data would be "name="Foo" bar="Bletch"".
        // </param>
        // <seealso member="CloseTag"/>
        // </doc>
        public void WriteTag(string tagName) {
            WriteTag(tagName, false, null, null);
        }

        public void WriteTag(string tagName, bool closeToo) {
            WriteTag(tagName, closeToo, null, null);
        }

        public void WriteTag(string tagName, bool closeToo, LogAttribute data) {
            if ( data == null )
                WriteTag(tagName, closeToo, null, null);
            else
                WriteTag(tagName, closeToo, null, new LogAttribute[] { data });
        }

        public void WriteTag(string tagName, bool closeToo, LogAttribute[] data) {
            WriteTag(tagName, closeToo, null, data);
        }

        public void WriteTag(string tagName, bool closeToo, string elementText, LogAttribute[] data) {
            if ( !needsIndenting )
                WriteLine();        // Write this tag on a new line

            WriteSpaces();
            writer.WriteStartElement(tagName);
            consoleWriter.WriteStartElement(tagName);
            indent++;

            if ( data != null ) {
                foreach ( LogAttribute attr in data ) {
                    writer.WriteAttributeString(attr.Name, attr.Value);
                    consoleWriter.WriteAttributeString(attr.Name, attr.Value);
                }
            }

            if ( elementText != null ) {
                WriteLine();
                WriteLine(elementText);

                // If we're closing the tag too, set up the indent for the close tag.
                if ( closeToo ) {
                    indent--;
                    WriteSpaces();
                }
            }

            if ( closeToo ) {
                writer.WriteEndElement();
                consoleWriter.WriteEndElement();

                // Need an unindent if there's no element text
                if ( elementText == null )
                    indent--;
            }

            WriteLine();
        }


        // <doc>
        // <desc>
        //   Closes an open tag.  Assumes this tag has content, i.e. element text.
        // </desc>
        // <param term="tagName">
        //   The name of the tag to close
        // </param>
        // <seealso member="WriteTag"/>
        // </doc>
        public virtual void CloseTag()
        {
            if ( !needsIndenting )
                WriteLine();            // Write this tag on a new line

            indent--;
            WriteSpaces();
            writer.WriteEndElement();
            consoleWriter.WriteEndElement();
            WriteLine();

            needsIndenting = true;
        }

        public void Close() {
            writer.WriteEndDocument();
            consoleWriter.WriteEndDocument();
        }

        // <doc>
        // <desc>
        //   Writes a string to the XML stream. Indenting is performed if necessary.
        // </desc>
        // <param term="data">
        //   The string to write to the XML stream.
        // </param>
        // </doc>
        public void Write(String data) {
            if (needsIndenting) {
                needsIndenting = false;
                WriteSpaces();
            }

            writer.WriteString(data);
            consoleWriter.WriteString(data);
        }
        
        // <doc>
        // <desc>
        //   Writes a string to the XML stream followed by a CRLF pair. Indenting is performed if necessary.
        // </desc>
        // <param term="data">
        //   The string to write to the XML stream.
        // </param>
        // </doc>
        public void WriteLine(String data) {
            if (needsIndenting) {
                needsIndenting = false;
                WriteSpaces();
            }

            Write(data);
            WriteLine();

            needsIndenting = true;
        }

        // <doc>
        // <desc>
        //   Writes a string to the XML stream followed by a CRLF pair. Indenting is performed if necessary.
        // </desc>
        // </doc>
        public void WriteLine() {
            writer.WriteWhitespace(newLine);
            consoleWriter.WriteWhitespace(newLine);
            needsIndenting = true;
        }

        // <doc>
        // <desc>
        //   Writes the spaces to indent the data in the XML stream.
        // </desc>
        // </doc>
        private void WriteSpaces() {
            for (int i=0; i<indent * INDENT; i++) {
                writer.WriteWhitespace(" ");
                consoleWriter.WriteWhitespace(" ");
            }
        }
    }
}

