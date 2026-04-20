using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TestWeiboMobileApi.Models
{
    public partial class WeiboCnMobileModel
    {
        [JsonProperty("ok")]
        public long? Ok { get; set; }

        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("cardlistInfo")]
        public CardlistInfo? CardlistInfo { get; set; }

        [JsonProperty("cards")]
        public List<Card>? Cards { get; set; }

        [JsonProperty("scheme")]
        public string? Scheme { get; set; }

        [JsonProperty("showAppTips")]
        public long? ShowAppTips { get; set; }
    }

    public partial class CardlistInfo
    {
        [JsonProperty("containerid")]
        public string? Containerid { get; set; }

        [JsonProperty("v_p")]
        public long? VP { get; set; }

        [JsonProperty("show_style")]
        public long? ShowStyle { get; set; }

        [JsonProperty("total")]
        public long? Total { get; set; }

        [JsonProperty("autoLoadMoreIndex")]
        public long? AutoLoadMoreIndex { get; set; }

        [JsonProperty("since_id")]
        public long? SinceId { get; set; }
    }

    public partial class Card
    {
        [JsonProperty("card_type")]
        public long? CardType { get; set; }

        [JsonProperty("profile_type_id")]
        public string? ProfileTypeId { get; set; }

        [JsonProperty("itemid")]
        public string? Itemid { get; set; }

        [JsonProperty("scheme")]
        public Uri? Scheme { get; set; }

        [JsonProperty("mblog")]
        public Mblog? Mblog { get; set; }
    }

    public partial class Mblog
    {
        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("mid")]
        public string? Mid { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("pic_ids")]
        public List<string>? PicIds { get; set; }

        [JsonProperty("user")]
        public User? User { get; set; }

        [JsonProperty("retweeted_status")]
        public object? RetweetedStatus { get; set; }

        [JsonProperty("reposts_count")]
        public long? RepostsCount { get; set; }

        [JsonProperty("comments_count")]
        public long? CommentsCount { get; set; }

        [JsonProperty("attitudes_count")]
        public long? AttitudesCount { get; set; }

        [JsonProperty("pic_num")]
        public long? PicNum { get; set; }

        [JsonProperty("page_info")]
        public PageInfo? PageInfo { get; set; }

        [JsonProperty("live_photo")]
        public List<string>? LivePhoto { get; set; }
    }

    public partial class PageInfo
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("urls")]
        public Urls? Urls { get; set; }
    }

    public partial class Urls
    {
        [JsonProperty("mp4_8k_mp4")]
        public string? Mp48kMp4 { get; set; }

        [JsonProperty("mp4_4k_mp4")]
        public string? Mp44kMp4 { get; set; }

        [JsonProperty("mp4_2k_mp4")]
        public string? Mp42kMp4 { get; set; }

        [JsonProperty("mp4_1080p_mp4")]
        public string? Mp41080pMp4 { get; set; }

        [JsonProperty("mp4_720p_mp4")]
        public string? Mp4720pMp4 { get; set; }

        [JsonProperty("mp4_hd_mp4")]
        public string? Mp4HDMp4 { get; set; }

        [JsonProperty("mp4_ld_mp4")]
        public string? Mp4LDMp4 { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("screen_name")]
        public string? ScreenName { get; set; }

        [JsonProperty("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("followers_count")]
        public string? FollowersCount { get; set; }

        [JsonProperty("avatar_hd")]
        public string? AvatarHd { get; set; }

        [JsonProperty("verified")]
        public bool? Verified { get; set; }

        [JsonProperty("gender")]
        public string? Gender { get; set; }
    }
}
