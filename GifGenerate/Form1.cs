using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Gif.Components;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;

namespace GifGenerate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            #region GIF图片加水印

            #region 定义变量

            //输入Gif源目录
            string gifSource = @"E:\gif\a\";

            //输出GIF保存目录
            string gifSavePath = @"E:\gif\d\";

            //要处理的GIF
            string gifPath = string.Empty;

            //要加入的水印
            string waterPath = Application.StartupPath + "\\Gif\\watermark.png";

            //输出GIF保存名字 【名字和原名字保持一致】
            string gifSaveFileName = string.Empty;

            #endregion

            //获得目录下所有的Gif
            string[] ProcessGif = Directory.GetFileSystemEntries(gifSource, "*.gif");

            foreach (var item in ProcessGif)
            {
                gifPath = item;
                gifSaveFileName = Path.GetFileName(gifPath);

                GifEntityInfo gd = new GifEntityInfo()
                {
                    GifPath = item,
                    WaterPath = waterPath,
                    GifSavePath = gifSavePath,
                    GifSaveFileName = gifSaveFileName
                };

                //一个线程处理一个任务
                Thread th = new Thread(new ParameterizedThreadStart(GifRun));
                th.Start(gd);
            }
            #endregion
        }

        public void GifRun(object gd)
        {
            #region 开始处理
            /*
            //获取延迟时间
            int delay = gc.GetFramePlay(gifPath);

            //解压帧
            List<string> lsFrame = gc.ExtractFrame(gifPath);

            //对每帧加水印
            if (lsFrame != null)
            {
                //x轴偏移量
                //y轴偏移量

                //加水印的类型
                //文字的颜色
                //文字的字体
                //图片质量

                int y = 1;

                foreach (var item in lsFrame)
                {
                    //WaterMark.AddImageSignPic(item, item, waterPath, 9, 100, 10, y);
                    WaterMark.AddImageSignText(item, item, "首发@baidu.com", 9, 100, "Tahoma", 13, y);
                    y += 3;
                }
            }

            //生成gif
            gc.CreateGif(lsFrame, Application.StartupPath + "\\img\\x.gif", delay);

            MessageBox.Show("加水印GIF创建成功!");
            */

            #endregion

            #region 开始处理

            GifEntityInfo gifData = (GifEntityInfo)gd;

            GifCreator gc = new GifCreator();

            //获取延迟时间
            int delay = gc.GetFramePlay(gifData.GifPath);

            //解压帧
            List<string> lsFrame = gc.ExtractFrame(gifData.GifPath);

            //对每帧加水印
            if (lsFrame != null)
            {
                #region 参数
                //x轴偏移量
                //y轴偏移量

                //加水印的类型
                //文字的颜色
                //文字的字体
                //图片质量 
                #endregion

                int x = 1;//水平方向偏移量
                int y = 1;//垂直方向偏移量

                foreach (var item in lsFrame)
                {
                    //WaterMark.AddImageSignPic(item, item, waterPath, 9, 100, 10, y);
                    WaterMark.AddImageSignText(item, item, "内涵首发@afuli.mobi", 9, 100, "Tahoma", 13, x, y);
                    y += 3;
                    x += 3;
                }
            }

            //生成gif
            gc.CreateGif(lsFrame, string.Concat(gifData.GifSavePath, @"\", gifData.GifSaveFileName), delay);

            //MessageBox.Show("加水印GIF创建成功!");
            #endregion
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //提取图片的第一帧
            string gifSource = @"C:\Users\Administrator\Desktop\VTSBlog\VTS.Web\photo\temp\027-xianren\1002\_b\";
            string gifCover = @"C:\Users\Administrator\Desktop\VTSBlog\VTS.Web\photo\temp\027-xianren\1002\_c\";
            string[] ProcessGif = Directory.GetFileSystemEntries(gifSource, "*.gif");
            string filename = string.Empty;
            string filepath = string.Empty;

            for (int i = 0; i < ProcessGif.Length; i++)
            {
                //提取每个GIf的第一帧并保存
                filename = Path.GetFileNameWithoutExtension(ProcessGif[i]);
                GetCovermapFromGif(ProcessGif[i], string.Concat(filename, "_c", ".jpg"), gifCover);
            }
            MessageBox.Show("ok");
        }

        #region 从Gif中提取封面图
        /// <summary>
        /// 从Gif中提取封面图
        /// </summary>
        /// <param name="gifPath">Gif地址</param>
        /// <param name="fileName">保存的文件名</param>
        /// <param name="filePath">保存地址</param>
        public void GetCovermapFromGif(string gifPath, string fileName, string filePath)
        {
            #region 提取帧
            //加载GIF
            Image gif = Image.FromFile(gifPath, true);
            //维度
            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
            //总帧数
            int count = gif.GetFrameCount(fd);

            for (int i = 0; i < count; i++)
            {
                //激活当前帧
                gif.SelectActiveFrame(fd, i);
                if (i == 0)
                {
                    gif.Save(string.Concat(filePath, fileName), ImageFormat.Jpeg);
                    break;
                }
            }
            gif.Dispose();
            #endregion
        }
        #endregion
    }
}