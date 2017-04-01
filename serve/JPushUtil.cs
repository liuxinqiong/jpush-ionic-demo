using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cn.jpush.api;
using Newtonsoft.Json;
using cn.jpush.api.push.mode;
using cn.jpush.api.push.notification;
using cn.jpush.api.push;

namespace JPushTest
{
    class JPushUtil
    {
        private static string appKey = "a650733b66ef847d45d63959"; // Your App Key from JPush
        private static string masterSecret = "cc041c2cbe82d9f54741c23c"; // Your Master Secret from JPush

        /*
         * @return MessageResult
         * msg_id 返回消息号
         * sendno 由你赋值的字段，这里会传递回来
         * ResponseResult http返回的相关信息
         */
        public MessageResult sendNotification(HashSet<string> targets, TargetType targetType, string content)  
        {
            JPushClient client = new JPushClient(appKey, masterSecret);
            PushPayload pushPayload = new PushPayload();

            //step1:Platform 平台信息（必填），这里不纠结，直接选择全平台
            pushPayload.platform = Platform.all();

            //step2：准备告警内容
            pushPayload.notification = createNotification(content);

            //step3: option设置
            pushPayload.options = initOption();

            //step4：发送目标,JPush提供了多种方式，别名，标签，注册id，分群，广播等。s_表示是静态函数的意思，Audience中还有个tag的成员函数，为了避免命名冲突
            //pushPayload.audience = Audience.all();//测试是暂时使用all
            pushPayload.audience = createAudience(targets, targetType); 

            //last step:发送并返回结果
            return client.SendPush(pushPayload);
        }

        private Notification createNotification(string content)
        {
            Notification not = new Notification();
            //统一制定设置通知内容，如果各平台单独指定，这里会被覆盖
            not.setAlert(content);
            not.setAndroid(createAndroidNotification());
            not.setIos(createIosNotification());
            return not;
        }

        private IosNotification createIosNotification()
        {
            //定制ios效果
            IosNotification ios = new IosNotification();
            ios.setSound("default");//必须设置，否则无声音
            ios.setContentAvailable(false);//非静默推送
            //ios.setCategory();ios8才支持
            ios.autoBadge();//原有的badge基础上+1
            //ios.incrBadge(5);原有的badge值加上你所设定值
            return ios;
        }

        private AndroidNotification createAndroidNotification()
        {   
            //定制android效果
            AndroidNotification android = new AndroidNotification();
            //android.setTitle();如果指定了，则通知里原来展示App名称的地方，将展示成这个字段。
            //android.setBuilderID();Android SDK 可设置通知栏样式，这里根据样式 ID 来指定该使用哪套样式。
            android.setPriority(2);//表示最高优先级PRIORIY_MAX
            android.setCategory("alarm");
            //设置 提示音，振动，指示灯 sdk暂时不支持
            //android.alert_type = 1 | 2 | 4;
            return android;
        }

        private Options initOption()
        {
            Options ops = new Options();
            ops.time_to_live = 86400 * 10;//离线消息保存10天,已经是最大值
            //ops.override_msg_id //设置是否覆盖上一条通知
            //ops.apns_production //True 表示推送生产环境，False 表示要推送开发环境； 如果不指定则为推送生产环境。
            //ops.big_push_duration 缓慢推送，把原本尽可能快的推送速度，降低下来，在给定的 n 分钟内，均匀地向这次推送的目标用户推送最大值为 1440。未设置则不是定速推送。
            //ops.sendno 纯粹用来作为 API 调用标识，API 返回时被原样返回，以方便 API 调用方匹配请求与返回。
            return ops;
        }

        private Audience createAudience(HashSet<string> targets, TargetType targetType)
        {
            Audience audience = null;
            switch(targetType){
                case TargetType.TAG: audience = tag(targets); break;
                case TargetType.ALIAS: audience = alias(targets); break;
                case TargetType.REGISTRATIONID: audience = registrationId(targets); break;
                case TargetType.BROADCAST: audience = broadcast(); break;
                default: break;
            }
            return audience;
        }

        //标签发送--项目
        private Audience tag(HashSet<string> targets)
        {
            return Audience.s_tag(targets);
        }

        //别名发送--用户
        private Audience alias(HashSet<string> targets)
        {
            return Audience.s_alias(targets);
        }

        //registrationId发送
        private Audience registrationId(HashSet<string> targets)
        {
            return Audience.s_registrationId(targets);
        }

        //广播发送
        private Audience broadcast()
        {
            return Audience.all();
        }

        /*
         * 更多高级功能：
         * 1.Message 消息内容（可选）应用内消息。
         *      自定义消息，透传消息。此部分内容不会展示到通知栏上，JPush SDK 收到消息内容后透传给 App。App 需要自行处理。
         * 2.Received API 以 msg_id 作为参数，去获取该 msg_id 的送达统计数据。
         *      由于统计数据并非非是即时的,所以等待一小段时间再执行下面的获取结果方法
         *      System.Threading.Thread.Sleep(10000);
         *      ReceivedResult getReceivedApi(String msg_ids)
         * 3.提供一定时间段的用户相关统计数据：新增用户、在线用户、活跃用户。vip用户专用
         *      UsersResult getReportUsers(TimeUnit timeUnit, String start, int duration)
         * 4.消息统计查询接口，这个接口是vip用户专用
         *      MessagesResult getReportMessages(params String[] msgIds)
         * 5.Device-API,选择客户端设置，这里了解即可
         *      Device API 用于在服务器端查询、设置、更新、删除设备的 tag,alias 信息，使用时需要注意不要让服务端设置的标签又被客户端给覆盖了。
         *      如果不是熟悉 tag，alias 的逻辑建议只使用客户端或者服务端二者中的一种。如果是两边同时使用，请确认自己应用可以处理好标签和别名的同步。
         *      主要api有
         *      Method TagAliasResult getDeviceTagAlias(String registrationId)
         *      Method - DefaultResult updateDeviceTagAlias(String registrationId, bool clearAlias, bool clearTag)
         *      Method - DefaultResult updateDeviceTagAlias(String registrationId,String alias,HashSet tagsToAdd,HashSet tagsToRemove)
         *      Method - TagListResult getTagList()获取当前应用的所有标签列表
         *      Method - BooleanResult isDeviceInTag(String theTag, String registrationID)
         *      Method - public DefaultResult addRemoveDevicesFromTag(String theTag,HashSet toAddUsers, HashSet toRemoveUsers)
         *      Method - public DefaultResult deleteTag(String theTag, String platform)
         *      Method - AliasDeviceListResult getAliasDeviceList(String alias, String platform)
         *      Method - public DefaultResult deleteAlias(String alias, String platform)
         */
    }

    public enum TargetType   
    {
        TAG,
        ALIAS,
        REGISTRATIONID,
        BROADCAST
    }
}
