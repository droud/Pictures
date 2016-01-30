using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Pictures.Helpers
{
    class ImageHelper
    {
        // gets a JPEG image size quickly, pulled from StackOverflow
        public static Size GetJpegImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                stream = File.OpenRead(filename);
                rdr = new BinaryReader(stream);
                // keep reading packets until we find one that contains Size info
                for (;;)
                {
                    byte code = rdr.ReadByte();
                    if (code != 0xFF)
                        throw new ApplicationException(
                 "Unexpected value in file " + filename);
                    code = rdr.ReadByte();
                    switch (code)
                    {
                        // filler byte
                        case 0xFF:
                            stream.Position--;
                            break;
                        // packets without data
                        case 0xD0:
                        case 0xD1:
                        case 0xD2:
                        case 0xD3:
                        case 0xD4:
                        case 0xD5:
                        case 0xD6:
                        case 0xD7:
                        case 0xD8:
                        case 0xD9:
                            break;
                        // packets with size information
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                        case 0xC4:
                        case 0xC5:
                        case 0xC6:
                        case 0xC7:
                        case 0xC8:
                        case 0xC9:
                        case 0xCA:
                        case 0xCB:
                        case 0xCC:
                        case 0xCD:
                        case 0xCE:
                        case 0xCF:
                            ReadBEUshort(rdr);
                            rdr.ReadByte();
                            ushort h = ReadBEUshort(rdr);
                            ushort w = ReadBEUshort(rdr);
                            return new Size(w, h);
                        // irrelevant variable-length packets
                        default:
                            int len = ReadBEUshort(rdr);
                            stream.Position += len - 2;
                            break;
                    }
                }
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        // gets a PNG image size quickly, pulled from StackOverflow
        public static Size GetPngImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                byte[] buffer = new byte[24];
                stream = File.OpenRead(filename);
                stream.Read(buffer, 0, 24);

                var width = buffer[16] << 24 | buffer[17] << 16 | buffer[18] << 8 | buffer[19];
                var height = buffer[20] << 24 | buffer[21] << 16 | buffer[22] << 8 | buffer[23];

                return new Size(width, height);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        // gets a GIF image size quickly, pulled from StackOverflow
        public static Size GetGifImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                stream = File.OpenRead(filename);
                stream.Seek(6, SeekOrigin.Begin);
                rdr = new BinaryReader(stream);

                var width = rdr.ReadUInt16();
                var height = rdr.ReadUInt16();

                return new Size(width, height);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        // gets a BMP image size quickly, pulled from StackOverflow
        public static Size GetBmpImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                stream = File.OpenRead(filename);
                stream.Seek(18, SeekOrigin.Begin);
                rdr = new BinaryReader(stream);

                var width = (int)rdr.ReadUInt32();
                var height = (int)rdr.ReadUInt32();

                return new Size(width, height);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        // helper for JPEG short encoding
        private static ushort ReadBEUshort(BinaryReader rdr)
        {
            ushort hi = rdr.ReadByte();
            hi <<= 8;
            ushort lo = rdr.ReadByte();
            return (ushort)(hi | lo);
        }
    }
}
