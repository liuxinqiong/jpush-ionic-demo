错误码定义
Code	描述	详细解释
6001	无效的设置，tag / alias 不应参数都为 null。	
6002	设置超时。	建议重试。
6003	alias 字符串不合法。	有效的别名、标签组成：字母（区分大小写）、数字、下划线、汉字。
6004	alias超长。	最多 40个字节，中文 UTF-8 是 3 个字节。
6005	某一个 tag 字符串不合法。	有效的别名、标签组成：字母（区分大小写）、数字、下划线、汉字。
6006	某一个 tag 超长。	一个 tag 最多 40个字节，中文 UTF-8 是 3 个字节。
6007	tags 数量超出限制，最多 100 个。	这是一台设备的限制，一个应用全局的标签数量无限制。
6008	tag / alias 超出总长度限制。	总长度最多 1K 字节。
6011	10s内设置 tag 或 alias 大于 3 次。	短时间内操作过于频繁。