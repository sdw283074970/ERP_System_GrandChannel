#文档版本：20200522.1

## 1.	基本信息
### 1.1	BaseUrl

生产环境`BaseUrl`: https://grandchannellogistic.com/

测试环境`BaseUrl`: https://grandchanneltest.com/

### 1.2 身份验证

采用签名验证

### 1.3 签名(sign)算法

1. 由Grand Channel分发`secretKey`+ 升序排列请求参数（包括`RequestId`）组成字符串

2. 将字符串转换为全大写

3. 对字符串进行MD5加密(32位大写)

4. 生成的字符串中有横杠`-`，请去除。

### 1.4 Grand Channel颁发的应用识别符和密钥

AppKey: `3be1ed1659364dbbbbfcc5ac94df0f19`(测试用)

SecretKey: `f65c8855289446ae98c0ba20e4990d9f` (测试用)(机密)

## 2.	POST推送订单接口

### 2.1	拼接地址及参数

`api/FBAexAPI/?appKey=占位&customerCode=占位&requestId=占位&version=占位&sign=占位`

### 2.2	Url参数说明
`appKey`: 由Grand Channel 分发；

`secretKey`：由Grand Channel 分发；

`customerCode`：由Grand Channel分发，只有合法的customerCode才能推送订单；

`requestId`：由请求端自己准备的UID，用于做重复请求检验，也是查询问题的依据；

`version`：接口版本。目前只接受“V1”；

`sign`：由Grand Channel制定的签名算法规则计算得到。

### 2.3	签名算法示例

`appKey`: `3be1ed1659364dbbbbfcc5ac94df0f19`

`secretKey`：`f65c8855289446ae98c0ba20e4990d9f`

`customerCode`：`TEST`

`requestId`：`SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F`

`version`：`V1`

```c#
var sign = ("f65c8855289446ae98c0ba20e4990d9f".ToUpper() + "&customerCode=TESTCODE&requestId=SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F&version=V1").ToMD5(32).Replace("-", "S")
```

完整Api测试服请求URL示例：

`https://grandchanneltest.com/api/FBAexAPI/?appKey=3be1ed1659364dbbbbfcc5ac94df0f19&customerCode=TESTCODE&requestId=SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F&version=V1&sign=FA9792F680F36D6F84B3FAD4EDF9B520`

### 2.4 回执

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：

```JavaScript
{
  "code": 503,	// 错误代码
  "resultStatus": "Failed",	// 请求状态，成功或失败
  "message": "Faild to validate request body. See inner message.",	// 服务器返回的信息，通常包含错误信息提示
  "innerMessage": [	// 更多信息，一般在对request body模型的验证失败后将错误信息反馈在这里
    {
      "field": "order.ETADate",	// 验证没有通过的字段
      "message": "Estimate arrive date is required."	// 验证没有通过的原因
    },
    {
      "field": "order.FBAJobs[0].GrossWeight",
      "message": "字段 GrossWeight 必须在 0 和 99999.99 之间。"
    }
  ]
}
```

#### 状态代码列表

`code: 200`: 操作成功

`code：500`: 无效的`AppKey`

`code：501`: 无效的签名（签名验证失败）

`code：503`: Request Body中的模型验证失败

`code：504`: 重复请求被拦截，注意检查request Id的唯一性

`code：505`: 无效的API版本号

`code：506`: 重复的`Container`号

------------
