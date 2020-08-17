using MetadataExtractor;
using Pictures.Helpers;
using Pictures.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pictures.Services
{
    /// <summary>
    /// This reads from a set of folders and caches picture dimensions and paths
    /// </summary>
    public class PhotoService : IDisposable
    {
        private SQLiteConnection _sqlite = null;
        private string[] _paths = null;
        private Thread _thread = null;

        private object _lock = new object();
        private static Random _random = new Random(Environment.TickCount);

        public bool Loaded = false;

        public PhotoService(SettingService settingService)
        {
            _sqlite = new SQLiteConnection("pictures.sqlite");
            _sqlite.CreateTable<Picture>();

            _paths = new[] { settingService.Path };

            _thread = new Thread(LoadPaths);

            Refresh();
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_thread.IsAlive)
                {
                    _thread.Abort();
                    _sqlite.Close();
                }
            }
        }

        public void Refresh()
        {
            lock (_lock)
            {
                if (_thread.IsAlive) return;

                _thread.Start();
            }
        }

        private void LoadPaths()
        {
            try
            {
                // loads pictures from the paths
                foreach (var path in _paths)
                {
                    LoadPath(path);
                }
            }
            catch (Exception e)
            {
                // TODO: log something
            }

            Loaded = true;
        }

        private void LoadPath(string path)
        {
            var files = System.IO.Directory.GetFiles(path);
            foreach (var file in files)
            {
                var lower = file.ToLower();

                var width = 0;
                var height = 0;
                var rotate = 0;

                try
                {
                    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
                    var jpeg = directories.FirstOrDefault(d => d.Name.Equals("JPEG"));
                    if (jpeg != null)
                    {
                        var heightTag = jpeg.Tags.FirstOrDefault(t => t.Type == 1).Description;
                        heightTag = heightTag.Substring(0, heightTag.IndexOf(' '));
                        height = int.Parse(heightTag);

                        var widthTag = jpeg.Tags.FirstOrDefault(t => t.Type == 3).Description;
                        widthTag = widthTag.Substring(0, widthTag.IndexOf(' '));
                        width = int.Parse(widthTag);

                        var exif = directories.FirstOrDefault(d => d.Name.Equals("Exif IFD0"));
                        if (exif != null)
                        {
                            var orientTag = exif.Tags.FirstOrDefault(t => t.Type == 274);
                            if (orientTag != null)
                            {
                                if (orientTag.Description.Contains("Rotate 90"))
                                {
                                    rotate = 90;
                                }
                                else if (orientTag.Description.Contains("Rotate 180"))
                                {
                                    rotate = 180;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }

                // get the image size based on extension
                //Size size = Size.Empty;
                //if (lower.EndsWith(".jpg") || lower.EndsWith(".jpeg"))
                //    size = ImageHelper.GetJpegImageSize(file);
                //if (lower.EndsWith(".bmp"))
                //    size = ImageHelper.GetBmpImageSize(file);
                //if (lower.EndsWith(".png"))
                //    size = ImageHelper.GetPngImageSize(file);
                //if (lower.EndsWith(".gif"))
                //    size = ImageHelper.GetGifImageSize(file);
                
                // ensure we only try to work with JPEGs
                if (height > 0 && width > 0)
                {
                    // check to see if this picture is in the database
                    Picture picture = null;
                    lock (_sqlite)
                    {
                        picture = _sqlite.Table<Picture>().Where(p => p.Path.Equals(file)).FirstOrDefault();
                    }

                    if (picture == null)
                    {
                        try
                        {
                            picture = new Picture() { Path = file, Width = width, Height = height, Rotate = rotate };

                            lock (_sqlite)
                            {
                                var id = _sqlite.Insert(picture);
                            }
                        }
                        catch (Exception e)
                        {
                            // TODO: log something
                        }
                    }
                }
            }

            // recursively traverse folders
            foreach (var directory in System.IO.Directory.GetDirectories(path))
                LoadPath(directory);
        }

        public Picture GetRandomPicture()
        {
            return GetPicture(p => p.Random > _random.Next());
        }

        public Picture GetRandomWidePicture()
        {
            return GetPicture(p => p.Random > _random.Next() && p.Wide == true);
        }

        private Picture GetPicture(Func<Picture,bool> query)
        {
            Picture picture = null;

            var findsw = Stopwatch.StartNew();
            while (picture == null)
            {
                var selectsw = Stopwatch.StartNew();
                // get the first ten pictures with a bigger random file
                lock (_sqlite)
                {
                    picture = _sqlite.Table<Picture>().Where(query).OrderBy(p => p.Random).FirstOrDefault();
                }
                selectsw.Stop();

                if (picture != null)
                {
                    // make sure the picture is in one of our paths
                    if (_paths.Any(p => picture.Path.StartsWith(p)) == false)
                    {
                        lock (_sqlite)
                        {
                            _sqlite.Delete<Picture>(picture.Id);
                        }
                    }

                    // make sure the picture exists
                    if (File.Exists(picture.Path) == false)
                    {
                        lock (_sqlite)
                        {
                            _sqlite.Delete<Picture>(picture.Id);
                        }
                    }
                }
                else
                {
                    var count = 0;
                    lock (_sqlite)
                    {
                        _sqlite.Table<Picture>().Count();
                    }

                    if (count == 0)
                    {
                        Refresh();

                        break;
                    }
                }
            }
            findsw.Stop();

            return picture;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}