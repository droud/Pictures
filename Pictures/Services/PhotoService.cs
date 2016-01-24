using Pictures.Helpers;
using Pictures.Models;
using SQLite;
using System;
using System.Collections.Generic;
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
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var lower = file.ToLower();

                // ensure we only try to work with JPEGs
                if (lower.EndsWith(".jpg") || lower.EndsWith(".jpeg"))
                {
                    // check to see if this picture is in the database
                    var picture = _sqlite.Table<Picture>().Where(p => p.Path.Equals(file)).FirstOrDefault();
                    if (picture == null)
                    {
                        try
                        {
                            var size = ImageHelper.GetJpegImageSize(file);
                            picture = new Picture() { Path = file, Width = size.Width, Height = size.Height };

                            lock (_lock)
                            {
                                var id = _sqlite.Insert(picture);
                            }
                        }
                        catch (Exception e)
                        {
                            // TODO: log something
                            Console.WriteLine("Could not load picture: " + file);
                        }
                    }
                }

                // TODO: support BMPs and PNGs and such (need quick dimension loader)
            }

            // recursively traverse folders
            foreach (var directory in Directory.GetDirectories(path))
                LoadPath(directory);
        }

        public Picture GetRandomPicture()
        {
            var random = _random.Next();
            return GetPicture(p => p.Random > random);
        }

        public Picture GetRandomWidePicture()
        {
            var random = _random.Next();
            return GetPicture(p => p.Random > random && p.Wide == true);
        }

        private Picture GetPicture(Func<Picture,bool> query)
        {
            Picture picture = null;

            while (picture == null)
            {
                // get the first ten pictures with a bigger random field
                picture = _sqlite.Table<Picture>().Where(query).OrderBy(p => p.Random).FirstOrDefault();

                if (picture != null)
                {
                    // make sure the picture is in one of our paths
                    if (_paths.Any(p => picture.Path.StartsWith(p)) == false)
                    {
                        _sqlite.Delete<Picture>(picture.Id);
                    }

                    // make sure the picture exists
                    if (File.Exists(picture.Path) == false)
                    {
                        _sqlite.Delete<Picture>(picture.Id);
                    }

                    Console.WriteLine("Loaded: " + picture.Id + " " + picture.Random);
                    return picture;
                }
                else
                {
                    // break if we don't have any pictures
                    if (_sqlite.Table<Picture>().Count() == 0)
                    {
                        Refresh();

                        break;
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}