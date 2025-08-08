using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels
{
    public class PlatformInternalAccountTaskModel
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public string? SlaveAccountWhoCreateTaskLogin { get; set; } = null;
        public string? MainAccountWhoExecuteTaskLogin { get; set; } = null;
        public string? InternalNumberId { get; set; } = null;
        /// <summary>
        /// 
        /// </summary>
        public string? session { get; set; } = null;
        public string? name { get; set; } = null;
        public string? url { get; set; } = null;
        public string? url_count { get; set; } = null;
        public string? type { get; set; } = null;
        public string? description { get; set; } = null;
        public string? approve_type { get; set; } = null;
        public string? approve_count { get; set; } = null;
        public string? approve_text { get; set; } = null;
        public string? approve_quest_0 { get; set; } = null;
        public string? approve_answer_0_1 { get; set; } = null;
        public string? approve_answer_0_count { get; set; } = null;
        public string? approve_quest_1 { get; set; } = null;
        public string? approve_answer_1_1 { get; set; } = null;
        public string? approve_answer_1_count { get; set; } = null;
        public string? special_adult { get; set; } = null;         // if null --- dont paste
        public string? day_1 { get; set; } = null;
        public string? day_2 { get; set; } = null;
        public string? day_3 { get; set; } = null;
        public string? day_4 { get; set; } = null;
        public string? day_5 { get; set; } = null;
        public string? day_6 { get; set; } = null;
        public string? day_7 { get; set; } = null;
        public string? time_6_9_flag { get; set; } = null;
        public string? time_6_9 { get; set; } = null;
        public string? time_9_12_flag { get; set; } = null;
        public string? time_9_12 { get; set; } = null;
        public string? time_12_15_flag { get; set; } = null;
        public string? time_12_15 { get; set; } = null;
        public string? time_15_18_flag { get; set; } = null;
        public string? time_15_18 { get; set; } = null;
        public string? time_18_21_flag { get; set; } = null;
        public string? time_18_21 { get; set; } = null;
        public string? time_21_24_flag { get; set; } = null;
        public string? time_21_24 { get; set; } = null;
        public string? time_0_3_flag { get; set; } = null;
        public string? time_0_3 { get; set; } = null;
        public string? time_3_6_flag { get; set; } = null;
        public string? time_3_6 { get; set; } = null;
        public string? timeout { get; set; } = null;
        public string? work_filter { get; set; } = null;
        public string? family_filter { get; set; } = null;
        public string? gender_filter { get; set; } = null;
        public string? age_from { get; set; } = null;
        public string? age_to { get; set; } = null;
        public string? geo_filter { get; set; } = null;
        public string? geo_ru { get; set; } = null;
        public string? geo_ru_48 { get; set; } = null;
        public string? geo_ru_47 { get; set; } = null;
        public string? geo_ru_66 { get; set; } = null;
        public string? geo_ru_71 { get; set; } = null;
        public string? geo_ru_38 { get; set; } = null;
        public string? geo_ru_111 { get; set; } = null;
        public string? geo_ru_13 { get; set; } = null;
        public string? geo_ru_53 { get; set; } = null;
        public string? geo_ru_65 { get; set; } = null;
        public string? geo_ru_86 { get; set; } = null;
        public string? geo_ru_51 { get; set; } = null;
        public string? geo_ru_8 { get; set; } = null;
        public string? geo_ru_29 { get; set; } = null;
        public string? geo_ru_73 { get; set; } = null;
        public string? geo_ru_61 { get; set; } = null;
        public string? geo_ua_5 { get; set; } = null;
        public string? geo_ua_14 { get; set; } = null;
        public string? geo_ru_other { get; set; } = null;
        public string? geo_ua { get; set; } = null;
        public string? geo_ua_13 { get; set; } = null;
        public string? geo_ua_12 { get; set; } = null;
        public string? geo_ua_4 { get; set; } = null;
        public string? geo_ua_7 { get; set; } = null;
        public string? geo_ua_26 { get; set; } = null;
        public string? geo_ua_17 { get; set; } = null;
        public string? geo_ua_other { get; set; } = null;
        public string? per_24 { get; set; } = null;
        public string? repeat_value { get; set; } = null;
        public string? work_time { get; set; } = null;
        public string? repeat_before_check { get; set; } = null;
        public string? user_xp { get; set; } = null;
        public string? ip_filter { get; set; } = null;
        public string? captcha_type { get; set; } = null;
        public string? ref_filter { get; set; } = null;
        public string? proove_email { get; set; } = null;
        public string? proove_phone { get; set; } = null;
        public string? price_user { get; set; } = null;
        public string? auto_funds { get; set; } = null;
        /// <summary>
        /// /
        /// </summary>
        public string? platformTaskTextForSolveTask { get; set; } = null;
    }
}
