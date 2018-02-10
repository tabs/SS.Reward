﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using SiteServer.Plugin;
using SS.Reward.Core;
using SS.Reward.Model;
using SS.Reward.Pages;
using SS.Reward.Parse;
using SS.Reward.Provider;

namespace SS.Reward
{
    public class Main : PluginBase
    {
        public static Dao Dao { get; private set; }
        public static RecordDao RecordDao { get; private set; }

        private static readonly Dictionary<int, ConfigInfo> ConfigInfoDict = new Dictionary<int, ConfigInfo>();

        public ConfigInfo GetConfigInfo(int siteId)
        {
            if (!ConfigInfoDict.ContainsKey(siteId))
            {
                ConfigInfoDict[siteId] = ConfigApi.GetConfig<ConfigInfo>(siteId) ?? new ConfigInfo();
            }
            return ConfigInfoDict[siteId];
        }

        internal static Main Instance { get; private set; }

        public override void Startup(IService service)
        {
            Instance = this;

            Dao = new Dao();
            RecordDao = new RecordDao();

            service
                .AddDatabaseTable(RecordDao.TableName, RecordDao.Columns)
                .AddSiteMenu(siteId => new Menu
                {
                    Text = "文章打赏",
                    IconClass = "ion-social-yen",
                    Menus = new List<Menu>
                    {
                        new Menu
                        {
                            Text = "文章打赏记录",
                            Href = $"{nameof(PageRecords)}.aspx"
                        },
                        new Menu
                        {
                            Text = "文章打赏设置",
                            Href = $"{nameof(PageSettings)}.aspx"
                        }
                    }
                })
                .AddStlElementParser(StlReward.ElementName, StlReward.Parse);

            service.ApiGet += Service_ApiGet;
            service.ApiPost += Service_ApiPost;
        }

        private object Service_ApiGet(object sender, ApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.Action, nameof(StlReward.ApiQrCode)))
            {
                return StlReward.ApiQrCode(args.Request);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);

        }

        private object Service_ApiPost(object sender, ApiEventArgs args)
        {
            var request = args.Request;
            var action = args.Action;
            var id = args.Id;

            if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(id))
            {
                if (Utils.EqualsIgnoreCase(action, nameof(StlReward.ApiWeixinNotify)))
                {
                    return StlReward.ApiWeixinNotify(request, id);
                }
            }
            else if (!string.IsNullOrEmpty(action))
            {
                if (Utils.EqualsIgnoreCase(action, nameof(StlReward.ApiPay)))
                {
                    return StlReward.ApiPay(request);
                }
                if (Utils.EqualsIgnoreCase(action, nameof(StlReward.ApiPaySuccess)))
                {
                    return StlReward.ApiPaySuccess(request);
                }
                if (Utils.EqualsIgnoreCase(action, nameof(StlReward.ApiWeixinInterval)))
                {
                    return StlReward.ApiWeixinInterval(request);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}