using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gif.Components;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;


namespace GifGenerate
{
    /// <summary>
    /// GifEntityInfo实体
    /// </summary>
    public class GifEntityInfo
    {
        /// <summary>
        /// 要处理的GIf的完全路径
        /// </summary>
        public string GifPath { get; set; }

        /// <summary>
        /// 要加入GIF的水印路径
        /// </summary>
        public string WaterPath { get; set; }

        /// <summary>
        /// GIf要保存的目录
        /// </summary>
        public string GifSavePath { get; set; }

        /// <summary>
        /// GIF要保存的名字
        /// </summary>
        public string GifSaveFileName { get; set; }
    }

    /// <summary>
    /// 生成器
    /// </summary>
    public class GifCreator
    {
        #region GetFramePlay
        /// <summary>
        /// 1.获取GIF的帧延迟时间
        /// </summary>
        /// <param name="gifPath">GIF的物理路径</param>
        /// <returns></returns>
        public int GetFramePlay(string gifPath)
        {
            List<int> ls = new List<int>();
            #region 获取每一帧的延迟时间
            //加载Gif图片
            Image img = Image.FromFile(gifPath, true);
            FrameDimension dim = new FrameDimension(img.FrameDimensionsList[0]);
            //遍历图像帧
            for (int i = 0; i < img.GetFrameCount(dim); i++)
            {
                //激活当前帧
                img.SelectActiveFrame(dim, i);
                //遍历帧属性
                for (int j = 0; j < img.PropertyIdList.Length; j++)
                {
                    if ((int)img.PropertyIdList.GetValue(j) == 0x5100)//.如果是延迟时间
                    {
                        PropertyItem pItem = (PropertyItem)img.PropertyItems.GetValue(j);//获取延迟时间属性

                        byte[] delayByte = new byte[4];//延迟时间，以1/100秒为单位
                        delayByte[0] = pItem.Value[i * 4];
                        delayByte[1] = pItem.Value[1 + i * 4];
                        delayByte[2] = pItem.Value[2 + i * 4];
                        delayByte[3] = pItem.Value[3 + i * 4];

                        int delay = BitConverter.ToInt32(delayByte, 0) * 10; //乘以10，获取到毫秒
                        //MessageBox.Show(delay.ToString());//弹出消息框，显示该帧时长
                        //this.txtResult.AppendText(i + ":" + delay.ToString() + Environment.NewLine);
                        if (!ls.Contains(delay))
                        {
                            ls.Add(delay);
                        }
                        break;
                    }
                }
            }
            #endregion
            return ls.Max();
        } 
        #endregion

        #region ExtractFrame
        //2.提取帧 GIF的物理路径
        public List<string> ExtractFrame(string gifPath)
        {
            List<string> lsFilename = new List<string>();

            if (!File.Exists(gifPath))
            {
                return null;
            }

            //以当前gif的名字创建一个文件夹
            string gifFileName = Path.GetFileNameWithoutExtension(gifPath);
            string path = Path.GetDirectoryName(gifPath);
            string dir = string.Concat(path, @"\", gifFileName, @"\");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            #region 提取帧
            //加载GIF
            Image gif = Image.FromFile(gifPath, true);
            //维度
            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
            //总帧数
            int count = gif.GetFrameCount(fd);
            //要保存的文件名
            string fileFullName = string.Empty;
            for (int i = 0; i < count; i++)
            {
                //激活当前帧
                gif.SelectActiveFrame(fd, i);
                //名字规则处理
                if (i < 10)
                {
                    fileFullName = dir + "0" + i + ".jpg";
                }
                else
                {
                    fileFullName = dir + i + ".jpg";
                }
                lsFilename.Add(fileFullName);
                gif.Save(fileFullName, ImageFormat.Jpeg);
            }
            gif.Dispose();
            #endregion
            return lsFilename;
        } 
        #endregion

        #region CreateGif
        /// <summary>
        /// 创建Gif
        /// </summary>
        /// <param name="lsFrame">一组按照顺序排列好的帧</param>
        /// <param name="saveGifPath">GIF保存的路径</param>
        /// <param name="delay">切换帧的时间间隔</param>
        public void CreateGif(List<string> lsFrame, string saveGifPath, int delay)
        {
            #region 使用 Gif.Components 创建
            AnimatedGifEncoder eG = new AnimatedGifEncoder();

            String[] imageFilePaths = lsFrame.ToArray();
            String outputFilePath = saveGifPath;

            eG.Start(outputFilePath);
            eG.SetDelay(delay);    // 延迟间隔
            eG.SetRepeat(0);       //-1:不循环,0:总是循环 播放   
            eG.SetQuality(100);

            for (int i = 0, count = imageFilePaths.Length; i < count; i++)
            {
                eG.AddFrame(Image.FromFile(imageFilePaths[i]));
            }
            eG.Finish();
            #endregion
        } 
        #endregion

        /*
        public static Bitmap WaterMarkWithText(Bitmap origialGif, string text, string filePath)
        {
            //用于存放桢 
            List<Frame> frames = new List<Frame>();

            //如果不是gif文件,直接返回原图像 
            if (origialGif.RawFormat.Guid != System.Drawing.Imaging.ImageFormat.Gif.Guid)
            {
                return origialGif;
            }

            //如果该图像是gif文件 
            foreach (Guid guid in origialGif.FrameDimensionsList)
            {
                System.Drawing.Imaging.FrameDimension frameDimension = new System.Drawing.Imaging.FrameDimension(guid);

                int frameCount = origialGif.GetFrameCount(frameDimension);

                for (int i = 0; i < frameCount; i++)
                {
                    if (origialGif.SelectActiveFrame(frameDimension, i) == 0)
                    {
                        int delay = Convert.ToInt32(origialGif.GetPropertyItem(20736).Value.GetValue(i));

                        Image img = Image.FromHbitmap(origialGif.GetHbitmap());
                        Font font = new Font(new FontFamily("宋体"), 35.0f, FontStyle.Bold);
                        Graphics g = Graphics.FromImage(img);
                        g.DrawString(text, font, Brushes.BlanchedAlmond, new PointF(10.0f, 10.0f));

                        Frame frame = new Frame(img, delay);

                        frames.Add(frame);
                    }
                }
                Gif.Components.AnimatedGifEncoder gif = new Gif.Components.AnimatedGifEncoder();
                gif.Start(filePath);

                gif.SetDelay(100);
                gif.SetRepeat(0);
                for (int i = 0; i < frames.Count; i++)
                {
                    gif.AddFrame(frames[i].Image);
                }

                gif.Finish();
                try
                {
                    Bitmap gifImg = (Bitmap)Bitmap.FromFile(filePath);
                    return gifImg;
                }
                catch
                {

                    return origialGif;
                }
            }
            return origialGif;
        }
        */
    }

    /// <summary>
    /// 
    /// </summary>
    public class Giflibs
    {
        public Giflibs()
        {

        }
        public Giflibs(string gifPath, string savePath)
        {
            this.GifPath = gifPath;
            this.SavePath = savePath;
        }

        public string GifPath { get; set; }
        public string SavePath { get; set; }

        /// <summary>
        /// 提取GIF帧
        /// </summary>
        /// <param name="gifpath">GIF图片路径</param>
        /// <param name="savepath">帧输出目录</param>
        public static void ExtractFrame(string gifpath, string savepath)
        {
            //参数获取可以不用，直接使用属性在内部获取
            if (!File.Exists(gifpath))
            {
                return;
            }
            #region 提取帧
            //加载GIF 例如：Application.StartupPath + "\\Gif\\a.gif"
            Image gif = Image.FromFile(gifpath, true);
            //维度
            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
            //总帧数
            int count = gif.GetFrameCount(fd);

            //要保存的文件名
            string fileName = string.Empty;

            for (int i = 0; i < count; i++)
            {
                //激活当前帧
                gif.SelectActiveFrame(fd, i);
                //名字规则处理
                if (i < 10)
                {
                    fileName = string.Concat(savepath, "0", i, ".jpg");
                }
                else
                {
                    fileName = string.Concat(savepath, i, ".jpg");
                }
                gif.Save(fileName, ImageFormat.Jpeg);
            }
            gif.Dispose();
            #endregion
        }
    }

    /// <summary>
    /// 图片水印助手
    /// </summary>
    public class WaterMark
    {
        #region 图片水印
        /// <summary>
        /// 图片水印
        /// </summary>
        /// <param name="imgPath">服务器图片相对路径</param>
        /// <param name="filename">保存文件名</param>
        /// <param name="watermarkFilename">水印文件相对路径</param>
        /// <param name="watermarkStatus">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="quality">附加水印图片质量,0-100</param>
        /// <param name="watermarkTransparency">水印的透明度 1--10 10为不透明</param>
        /// <param name="OffsetX"></param>
        /// <param name="OffsetY"></param>
        public static void AddImageSignPic(string imgPath, string filename, string watermarkFilename, int watermarkStatus,
            int quality, int watermarkTransparency, int OffsetX, int OffsetY)
        {
            if (!File.Exists(imgPath))
                return;
            byte[] _ImageBytes = File.ReadAllBytes(imgPath);

            Image img = Image.FromStream(new MemoryStream(_ImageBytes));

            if (!File.Exists(watermarkFilename))
                return;

            Graphics g = Graphics.FromImage(img);
            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Image watermark = new Bitmap(watermarkFilename);

            if (watermark.Height >= img.Height || watermark.Width >= img.Width)
                return;

            ImageAttributes imageAttributes = new ImageAttributes();
            ColorMap colorMap = new ColorMap();

            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
            ColorMap[] remapTable = { colorMap };

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

            float transparency = 0.5F;
            if (watermarkTransparency >= 1 && watermarkTransparency <= 10)
                transparency = (watermarkTransparency / 10.0F);


            float[][] colorMatrixElements = 
            {
				new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
				new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
				new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
				new float[] {0.0f,  0.0f,  0.0f,  transparency, 0.0f},
				new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
			};

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            int xpos = 0;
            int ypos = 0;

            switch (watermarkStatus)
            {
                case 1:
                    xpos = (int)(img.Width * (float).01);
                    ypos = (int)(img.Height * (float).01);
                    break;
                case 2:
                    xpos = (int)((img.Width * (float).50) - (watermark.Width / 2));
                    ypos = (int)(img.Height * (float).01);
                    break;
                case 3:
                    xpos = (int)((img.Width * (float).99) - (watermark.Width));
                    ypos = (int)(img.Height * (float).01);
                    break;
                case 4:
                    xpos = (int)(img.Width * (float).01);
                    ypos = (int)((img.Height * (float).50) - (watermark.Height / 2));
                    break;
                case 5:
                    xpos = (int)((img.Width * (float).50) - (watermark.Width / 2));
                    ypos = (int)((img.Height * (float).50) - (watermark.Height / 2));
                    break;
                case 6:
                    xpos = (int)((img.Width * (float).99) - (watermark.Width));
                    ypos = (int)((img.Height * (float).50) - (watermark.Height / 2));
                    break;
                case 7:
                    xpos = (int)(img.Width * (float).01);
                    ypos = (int)((img.Height * (float).99) - watermark.Height);
                    break;
                case 8:
                    xpos = (int)((img.Width * (float).50) - (watermark.Width / 2));
                    ypos = (int)((img.Height * (float).99) - watermark.Height);
                    break;
                case 9:
                    xpos = (int)((img.Width * (float).99) - (watermark.Width));
                    ypos = (int)((img.Height * (float).99) - watermark.Height);
                    break;
            }

            g.DrawImage(watermark, new Rectangle(xpos, ypos, watermark.Width, watermark.Height), 0, 0,
                watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAttributes);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType.IndexOf("jpeg") > -1)
                    ici = codec;
            }

            EncoderParameters encoderParams = new EncoderParameters();
            long[] qualityParam = new long[1];
            if (quality < 0 || quality > 100)
                quality = 80;

            qualityParam[0] = quality;

            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityParam);
            encoderParams.Param[0] = encoderParam;

            if (ici != null)
                img.Save(filename, ici, encoderParams);
            else
                img.Save(filename);

            g.Dispose();
            img.Dispose();
            watermark.Dispose();
            imageAttributes.Dispose();
        }

        #endregion

        #region 文字水印

        /// <summary>
        /// 文字水印
        /// </summary>
        /// <param name="imgPath">服务器图片相对路径</param>
        /// <param name="filename">保存文件名</param>
        /// <param name="watermarkText">水印文字</param>
        /// <param name="watermarkStatus">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="quality">附加水印图片质量,0-100</param>
        /// <param name="fontname">字体</param>
        /// <param name="fontsize">字体大小</param>
        /// <param name="OffsetX"></param>
        /// <param name="OffsetY"></param>
        public static void AddImageSignText(string imgPath, string filename, string watermarkText, int watermarkStatus,
            int quality, string fontname, int fontsize, int OffsetX, int OffsetY)
        {
            byte[] _ImageBytes = File.ReadAllBytes(imgPath);
            Image img = Image.FromStream(new MemoryStream(_ImageBytes));

            Graphics g = Graphics.FromImage(img);
            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            //Font drawFont = new Font(fontname, fontsize, FontStyle.Regular, GraphicsUnit.Pixel);
            Font drawFont = new Font(fontname, fontsize, FontStyle.Bold, GraphicsUnit.Pixel);

            SizeF crSize;
            crSize = g.MeasureString(watermarkText, drawFont);

            float xpos = 0;
            float ypos = 0;

            switch (watermarkStatus)
            {
                case 1:
                    xpos = (float)img.Width * (float).01;
                    ypos = (float)img.Height * (float).01;
                    break;
                case 2:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = (float)img.Height * (float).01;
                    break;
                case 3:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = (float)img.Height * (float).01;
                    break;
                case 4:
                    xpos = (float)img.Width * (float).01;
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 5:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 6:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 7:
                    xpos = (float)img.Width * (float).01;
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    break;
                case 8:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    break;
                case 9:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    if (OffsetY > 0)
                    {
                        ypos = ypos - OffsetY;
                    }
                    if (OffsetX > 0)
                    {
                        xpos = xpos - OffsetX;
                    }
                    break;
            }

            //g.DrawString(watermarkText, drawFont, new SolidBrush(Color.White), xpos + 1, ypos + 1);
            //g.DrawString(watermarkText, drawFont, new SolidBrush(Color.Black), xpos, ypos);

            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.Black), xpos + 1, ypos + 1);
            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.Yellow), xpos, ypos);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType.IndexOf("jpeg") > -1)
                    ici = codec;
            }
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qualityParam = new long[1];
            if (quality < 0 || quality > 100)
                quality = 80;

            qualityParam[0] = quality;

            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityParam);
            encoderParams.Param[0] = encoderParam;

            if (ici != null)
                img.Save(filename, ici, encoderParams);
            else
                img.Save(filename);
            g.Dispose();
            img.Dispose();
        }

        #endregion
    }
}