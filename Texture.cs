using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;

namespace Giselle.DCConDownloader
{
    public class Texture : IDisposable
    {
        private Stream Stream = null;
        private Image Image = null;

        public Texture()
        {

        }

        public void Save(string path)
        {
            this.Image.Save(path);
        }

        public void Update()
        {
            ImageAnimator.UpdateFrames(this.Image);
        }

        public void Load(Stream stream)
        {
            this.DisposeQuietly();
            this.Image = null;

            try
            {
                this.Stream = new MemoryStream();
                stream.CopyTo(this.Stream);

                this.Image = Image.FromStream(this.Stream, false, false);
                ImageAnimator.Animate(this.Image, null);
            }
            catch (Exception e)
            {
                this.Dispose();
                Console.WriteLine(e);
            }

        }

        public void Render(Graphics g, Rectangle bounds)
        {
            this.Render(g, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public void Render(Graphics g, float x, float y, float width, float height)
        {
            try
            {
                g.DrawImage(this.Image, x, y, width, height);
            }
            catch
            {

            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Image != null)
            {
                ImageAnimator.StopAnimate(this.Image, null);
                this.Image.DisposeQuietly();
            }

            this.Stream.DisposeQuietly();
        }

        ~Texture()
        {
            this.Dispose(false);
        }

    }

}
