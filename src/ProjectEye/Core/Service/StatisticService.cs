﻿using ProjectEye.Core.Models;
using ProjectEye.Core.Models.Statistic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectEye.Core.Service
{
    /// <summary>
    /// 统计类型
    /// </summary>
    public enum StatisticType
    {
        /// <summary>
        /// 用眼时长
        /// </summary>
        WorkingTime,
        /// <summary>
        /// 休息时长
        /// </summary>
        ResetTime,
        /// <summary>
        /// 跳过次数
        /// </summary>
        SkipCount
    }
    /// <summary>
    /// 统计 Service
    /// 记录和管理用眼统计数据
    /// </summary>
    public class StatisticService : IService
    {
        private readonly string xmlPath;
        private readonly App app;
        private XmlExtensions xml;
        //存放文件夹
        private readonly string dir = "Data";
        //统计数据
        private StatisticListModel statisticList;
        //今日数据
        private StatisticModel todayStatistic;

        public StatisticService(App app)
        {
            this.app = app;
            this.app.Exit += app_Exit;
            statisticList = new StatisticListModel();
            statisticList.Data = new List<StatisticModel>();
            xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                dir,
                "statistic.xml");
            xml = new XmlExtensions(xmlPath);
        }

        private void app_Exit(object sender, ExitEventArgs e)
        {
            Save();
        }

        public void Init()
        {
            LoadStatisticData();
            ClearBefore7Data();
            var todayStatistic = Find(DateTime.Now.Date);
            if (todayStatistic == null)
            {
                todayStatistic = new StatisticModel()
                {
                    Date = DateTime.Now.Date,
                    WorkingTime = 0,
                    ResetTime = 0,
                    SkipCount = 0
                };
                statisticList.Data.Add(todayStatistic);

            }
            this.todayStatistic = Find(DateTime.Now.Date);
        }

        #region 加载统计数据
        /// <summary>
        /// 加载统计数据
        /// </summary>
        public void LoadStatisticData()
        {
            if (File.Exists(xmlPath))
            {
                var data = xml.ToModel(typeof(StatisticListModel));
                if (data != null)
                {
                    statisticList = data as StatisticListModel;
                }
                else
                {
                    xml.Save(statisticList);
                }
            }
            else
            {
                xml.Save(statisticList);
            }

        }

        #endregion

        #region 更新今日统计数据
        /// <summary>
        /// 更新今日统计数据(叠加)
        /// </summary>
        /// <param name="type">统计类型</param>
        /// <param name="value">增加的值(可以为负数)</param>
        public void Update(StatisticType type, int value)
        {
            switch (type)
            {
                case StatisticType.WorkingTime:
                    todayStatistic.WorkingTime += Math.Round(((double)value) / 60, 1);
                    break;
                case StatisticType.ResetTime:
                    todayStatistic.ResetTime += Math.Round(((double)value) / 60, 1);
                    break;
                case StatisticType.SkipCount:
                    todayStatistic.SkipCount += value;
                    break;
            }
        }
        #endregion

        #region 查找日期数据
        /// <summary>
        /// 查找日期数据
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public StatisticModel Find(DateTime date)
        {
            var data = statisticList.Data.Where(m => m.Date == date);
            if (data.Count() == 1)
            {
                return data.Single();
            }
            return null;
        }
        #endregion

        #region 清除7天前的数据
        /// <summary>
        /// 清除7天前的数据
        /// </summary>
        private void ClearBefore7Data()
        {
            statisticList.Data.RemoveAll(m => m.Date.Date < DateTime.Now.AddDays(-7).Date);
            xml.Save(statisticList);
        }
        #endregion

        #region 获取图表数据
        /// <summary>
        /// 获取图表数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public double[] GetChartData(StatisticType type)
        {
            ArrayList result = new ArrayList();
            foreach (StatisticModel statistic in statisticList.Data)
            {
                if (type == StatisticType.WorkingTime)
                {
                    result.Add(statistic.WorkingTime);
                }
                else if (type == StatisticType.ResetTime)
                {
                    result.Add(statistic.ResetTime);
                }
                else
                {
                    result.Add(statistic.SkipCount);
                }
            }
            return (double[])result.ToArray().Select(o => Convert.ToDouble(o)).ToArray();
        }
        #endregion

        #region 获取图表标签
        /// <summary>
        /// 获取图表标签
        /// </summary>
        /// <returns></returns>
        public string[] GetChartLabels()
        {
            ArrayList result = new ArrayList();
            foreach (StatisticModel statistic in statisticList.Data)
            {

                result.Add(statistic.Date.Day + "日");

            }
            return (string[])result.ToArray(typeof(string));
        }
        #endregion

        #region 数据持久化
        /// <summary>
        /// 数据持久化
        /// </summary>
        public void Save()
        {
            xml.Save(statisticList);
        }
        #endregion

    }
}