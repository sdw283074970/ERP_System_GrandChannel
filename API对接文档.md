# 文档版本：v 1.0.9
# 更新日期：2020-11-16

# 简介
这是一套由Grand Channel提供的基础免费API。客户可以通过这套API向Grand Channel下达各种业务请求或指令。

通读文档，开发者可以不需要Grand Channel提供任何协助就能实现测试环境的对接。

# 谁能使用？
这是一套免费的基础API，任何在Grand Channel系统中注册的客户都能获得Grand Channel颁发的应用识别符`appKey`和密钥`secretKey`，用于通过授权验证。授权后的请求可调用业务上一些基础的增删查改操作。

如果需要进一步如定制接口回调等服务，请邮件联系Grand Channel IT部门经理stone@grandchannel.us

沙盒测试环境中使用统一的`appKey`，`secretKey`以及`customerCode`
## 1.	基本信息
### 基础参数说明
所有请求采用统一的请求格式，uri中的参数始终固定(值不固定)，任何其他请求可能使用到的参数放在`body`中传递。

#### 拼接地址格式
`baseUrl` + `api/` + `API Name` + `/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

##### Url参数说明

###### 沙盒环境 Sand-box Env
`baseUrl`: `https://grandchanneltest.com/`

`API Name`: 所涉资源的接口名称

`appKey`: `3be1ed1659364dbbbbfcc5ac94df0f19`

`secretKey`: `f65c8855289446ae98c0ba20e4990d9f`

`customerCode`: `TEST`

`requestId`: 任意不重复的字符串

`version`: `V1`

`sign`：由Grand Channel制定的签名算法规则计算得到

###### 生产环境 Prod Env
`baseUrl`: `https://grandchanneltest.com/`

`API Name`: 所涉资源的接口名称

`appKey`: 由Grand Channel 分发

`secretKey`: 由Grand Channel 分发

`customerCode`: 由Grand Channel分发，只有合法的customerCode才能推送订单

`requestId`：由请求端自己准备的UID，用于做重复请求检验，也是查询问题的依据

`version`: `V1`

`sign`：由Grand Channel制定的签名算法规则计算得到

### 授权验证/`sign`算法

本套API采用url签名验证，签名`sign`算法如下：

1. 由Grand Channel分发`apptKey`和`secretKey`

2. 将`secretKey`字符串转换为大写

3. 升序排列请求参数（不含`secretKey`）组成字符串，拼接在`secretKey`后面，参数之间用`&`分隔，组成新字符串

4. 对以上新字符串进行MD5加密(32位大写)

5. 生成的字符串中有横杠`-`，请去除。

#### 示例

`baseUrl`: `https://grandchanneltest.com/`

`API Name`: `FBAexAPI`

`appKey`: `3be1ed1659364dbbbbfcc5ac94df0f19`

`secretKey`：`f65c8855289446ae98c0ba20e4990d9f`

`customerCode`：`TEST`

`requestId`：`SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F`

`version`：`V1`

```c#
var secretKey = "f65c8855289446ae98c0ba20e4990d9f".ToUpper();
var paramsStr = "&appKey=3be1ed1659364dbbbbfcc5ac94df0f19&customerCode=TEST&requestId=SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F&version=V1";
var signStr = secretKey + paramsStr;
var sign = signStr.ToMD5(32).Replace("-", "")   // sign = "E85154800B0DD09792E410A1E0BDA96D"
```

#### 完整Api测试服请求URL示例：

`https://grandchanneltest.com/api/FBAexAPI/?appKey=3be1ed1659364dbbbbfcc5ac94df0f19&customerCode=TEST&requestId=SDF1S3DF21-S5DF4136S2DF1-SD7F89SD51G6S-SD65FS6D31F&version=V1&sign=E85154800B0DD09792E410A1E0BDA96D`

# 错误代码
以下是一些通用的错误代码。不同的API还可能产生不同的错误代码，详情请见文档。

Error 200: 操作成功

Error 500: 无效的AppKey

Error 501: 无效的签名（签名验证失败）

Error 503: Request Body中的模型验证失败

Error 504: 重复请求被拦截，注意检查request Id的唯一性

Error 505: 无效的API版本号
------------

## 2.	POST推送入库单接口

### 2.1	拼接地址及参数

`api/FBAexAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

### 2.2 请求Body

请求中的Body为入库单详细内容，包括了入库单集装箱信息以及Packing List。当调用这个接口时，必须附带Body。

#### Body结构：

```JavaScript
{
	"agency": "agency test",  // 代理名称（调用此接口的平台名称），必填
	"mbl": "",  // MB/L，选填
	"container": "TEST123AAA23",  // 柜号，必填
	"subcustomer": "",  // 自定义子客户代码，选填
	"portOfLoading": "Port A",  // 卸货港，必填
	"deliveryPort": "Port V", // 到货港，必填
	"etlDate": "2020-05-13",  // 预计发船日期，必填
	"etaDate": "2020-06-13",  // 预计到港日期，必填
	"carrier": "test carrier",  // 货车公司，必填
	"vessel": "test vessel",  // 船名/船公司，选填
	"sealNumber": "seal no test", // 航班号，选填
	"containerSize": "40HQ",  // 货柜尺寸，必填
	"instruction": "Memo", // 订单备注指令
	"fbaJobs": [{ // 这个订单下的Packing List信息
		"shipmentId": "SKU1111",  // SKU，必填
		"amzRefId": "Amz11111", // 亚马逊Id，必填
		"productType": "type test", // 产品类型，选填
		"subcustomer": "1111",  // 自定义子客户代码，选填
		"warehouseCode": "ONT8",  // 转运仓库代码/地址，必填
		"quantity": 20, // 数量（箱），必填
		"grossWeight": 20.33,  // 总毛重（KG），必填
		"cbm": 22,  // 总CBM，必填
		"palletQuantity": 3,  // 到货状态下的托盘数量，选填
		"packageType": "CTN"  // 包装类型，选填
	},{
		"shipmentId": "SKU1122",
		"amzRefId": "Amz11331",
		"productType": "type test",
		"subcustomer": "144",
		"warehouseCode": "ONT2",
		"quantity": 330,
		"grossWeight": 222.33,
		"cbm": 334,
		"palletQuantity": 33,
		"packageType": "CTN"
	}]
}
```

### 2.3 回执

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：

```JavaScript
{
	"code": 503,	// 状态代码
	"validationStatus": "Failed",	// 请求状态，成功或失败
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

## 3.	POST推送出库单接口

### 3.1	拼接地址及参数

`api/FBAShipmentAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

### 3.2 请求Body

请求中的Body为出库单详细内容，包括了出库单地址信息以及拣货单。当调用这个接口时，必须附带Body。

#### Body结构：

```JavaScript
{
	"agency": "agency test",  // 代理名称（调用此接口的平台名称），必填
	"shipOrderNumber": "TEST123AAA23-TEST1",  // 运单号，选填，如果为空，系统会自动分配一个运单号
	"orderType": "Standard", // 运单类型，Standard，Ecommerce, DerectSale, Transfer,默认Standard,选填
	"ets": "2020-05-13",  // 预计发货时间，必填，格式（yyyy-MM-dd）
	"etsTimeRange": "4PM-5PM",  // 预计发货时间，精确到几点钟的范围，选填
	"destionation": "ONT8",  // 目的地代码，选填
	"address": "13780 Central Ave. Chino CA 91709",  // 目的地地址，选填
	"instruction": "Memo", // 订单备注指令
	"pickingList": [{ // 这个出库单中的拣货单
		"container": "TEST123332", // 从哪个集装箱中拣货，选填
		"shipmentId": "SKU1111",  // SKU，选填
		"amzRefId": "Amz11111", // 亚马逊Id，选填
		"warehouseCode": "ONT8",  // 转运仓库代码/地址，选填
		"quantity": 20, // 数量（箱），必填
		"palletQuantity": 3,  // 从库存中减去的托盘数量，选填
		"newPallet": 0, // 要求新打的托盘数量，选填
	},{
		"container": "TEST123332",
		"shipmentId": "SKU1122",
		"amzRefId": "Amz11331",
		"warehouseCode": "ONT2",
		"quantity": 330,
		"palletQuantity": 33,
		"newPallet": 0,
	}]
}
```
**注意：** `container` `shipmentId` `amzRefId` `warehouseCode`这四个字段相当于过滤器的选填条件，提供的信息越多越好。只有当过滤出唯一结果时，才会成功拣货，否则报错`code 3001`

### 3.3 回执

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：（2020-08-14更新）

```JavaScript
{
	"code": 507,	// 错误代码
	"validationStatus": "Failed",	// 请求状态，成功或失败
	"message": "Faild to validate request body. See inner message.",	// 服务器返回的信息，通常包含错误信息提示
	"innerMessage": [{	// 更多信息，一般在对request body模型的验证失败后将错误信息反馈在这里
		"field": "order.ETADate",	// 验证没有通过的字段
		"message": "Estimate shipping date is required."	// 验证没有通过的原因
	},
	{
		"field": "order.quantity",
		"message": "字段 quantity 必须在 0 和 99999 之间。"
	}],
	"pickingStatus":{
		"status": "Success", // 拣货总状态反馈，成功则在系统中生成出货单，失败则不生成
		"details": 	[{ // 拣货状态反馈细节，每一条SKU的成功与否都会显示在这里
			"shipOrderNumber": "TEST123321", // 由传入数据决定或系统自动分配
			"status": "Success", // 拣货结果状态
			"statusCode": 2000, // 成功拣货，无异常
			"container": "TEST123321", // 目标container
			"shipmentId": "SKU123321", // 目标SKU号
			"amzRefId": "AMZ1233213", // 目标AMZ ID
			"warehouseCode": "ONT8", // 目标的仓库目的地
			"etsCtns": 20, // 预计拣货箱数
			"pickedCtns": 20, // 实际成功拣货的箱数
			"instockCtns": 300, // 目标在仓库中剩下的箱数
			"estPlts": 0, // 本轮拣货预计扣除托盘数量
			"pickedPlts": 0, // 本轮拣货实际从库存中扣除的托盘数量
			"newPlts": 0, // 本轮拣货预计新打托盘的数量
			"message": "Success" // 更多拣货信息
		},
		{
			"shipOrderNumber": "TEST123321",
			"status": "Failed",
			"statusCode": 3001, // 异常1：在系统库存中搜索到多条记录
			"container": "TEST12323231",
			"shipmentId": "SKU12322321",
			"amzRefId": "AMZ124313",
			"warehouseCode": "ONT8",
			"etsCtns": 20,
			"pickedCtns": 0,
			"instockCtns": 0,
			"estPlts": 0,
			"pickedPlts": 0,
			"newPlts": 0,
			"message": "Picking failed. More than one target found in inventory"
		},
		{
			"status": "Failed",
			"statusCode": 3002, // 异常2：在库存中未找到拣货目标
			"container": "TEST12323231",
			"shipmentId": "SKU12322321",
			"amzRefId": "AMZ124313",
			"warehouseCode": "ONT8",
			"etsCtns": 20,
			"pickedCtns": 0,
			"instockCtns": 0,
			"estPlts": 0,
			"pickedPlts": 0,
			"newPlts": 0,
			"message": "Picking failed. No target was found in inventory"
		},
		{
			"status": "Uncompeleted",
			"statusCode": 3003, // 异常3：库存数量不足, 但仍然会把剩下的数量拣出来
			"container": "TEST12323231",
			"shipmentId": "SKU12322321",
			"amzRefId": "AMZ124313",
			"warehouseCode": "ONT8",
			"etsCtns": 20,
			"pickedCtns": 12,
			"instockCtns": 0,
			"estPlts": 0,
			"pickedPlts": 0,
			"newPlts": 0,
			"message": "Picking uncompeleted. Not enough quantity was found in inventory"
		}]
	}
}
```

#### 状态代码列表

`code: 200`: 操作成功

`code：500`: 无效的`AppKey`

`code：501`: 无效的签名（签名验证失败）

`code：503`: Request Body中的模型验证失败

`code：504`: 重复请求被拦截，注意检查request Id的唯一性

`code：505`: 无效的API版本号

`code：506`: 重复的`ShipmentOrderNumber`号

`code: 507`: 包含未成功拣货的SKU，具体原因见inner message

`code: 2000`: 成功拣货，无异常

`code: 3001`: 在系统库存中搜索到多条记录，系统不会执行任何拣货操作

`code 3002`: 在库存中未找到拣货目标

`code 3003`: 库存数量不足, 但仍然会把剩下的数量拣出来

------------

## 4.	GET查询/同步库存接口

### 4.1	拼接地址及参数

`api/FBAInventoryAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

### 4.2 请求Body

不接受请求Body。

### 4.3 回执

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：

```JavaScript
{
  "code": 200,	// 错误代码
  "resultStatus": "Success",	// 请求状态，成功或失败
  "message": "foo bar",	// 服务器返回的信息，通常包含错误信息提示
  "body": [
  	"customerCode": "TEST", // 客户代码
	"dateRange": "2019-06-01 ~ 2020-07-01", // 库存查询时间区间
	"reportDate": "2020-07-30", // 调用这个接口的日期
	"instockCtns": 230, // 库存总箱数
	"instockPlts": 20, // 库存总托盘数
	"processingCtns": 2000, // 正在处理出库的总箱数
	"processingPlts": 190, // 正在处理出库的总托盘数
	"inventoryCtns": [ // 库存箱子细节
		{
			"container": "TEST123321231", // 集装箱或入库单号,
			"storageType": "InPallet", // 库存类型
			"subCustomer": NULL, // 所属子客户
			"shipmentId": "SKU1231123", // ShipmentId或SKU号
			"amzRefId": "AMZ1231354", // Amz Id
			"warehouseCode": "ADS2", // whs code
			"inboundDate": "2019-06-01", // 该SKU的入库日期
			"grossWeightPerCtn": 15.22, // 每箱毛重
			"cbmPerCtn": 3.12, // 每箱cbm
			"originalQuantity": 20, // 实际原始收货数量
			"instockQuantity": 12, // 库存数量
			"processingQuantity": 8, // 正在操作的数量
			"holdCtns": 0, // 客户要求hold的在库数量
			"location": "F1", // 库位
		}，
		{
			"container": "TEST12sdf1231",
			"storageType": "InPallet",
			"subCustomer": NULL,
			"shipmentId": "SKU12sd123",
			"amzRefId": "AMZ1231sd54",
			"warehouseCode": "AOK2",
			"inboundDate": "2019-06-01",
			"grossWeightPerCtn": 19.62, 
			"cbmPerCtn": 3.69,
			"originalQuantity": 30,
			"instockQuantity": 18,
			"processingQuantity": 8,
			"holdCtns": 4,
			"location": "F2",
		}
	]
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

------------

## 5.	GET查询订单状态接口

### 5.1	拼接地址及参数

`api/FBAOrderStatusAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

### 5.2 请求Body (2020-08-13更新)

```JavaScript
{
	"orderType": "Inbound",	// "Inbound"或者"Outbound"
	"reference": [	// 需要查询的入库单柜号或者出库单单号
		"TEST-1", "TEST-2", "TEST-3"
	],
	"fromDate": "yyyy-MM-dd",	// 要查询日期范围的开始，以建单时间为准
	"toDate": "yyyy-MM-dd",	// 日期范围的结尾
	"status": "NA"	// 要查询的订单状态，NA返回所有状态的订单
}
```

### 5.3 回执 （2020-08-14更新）

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：

```JavaScript
{
  "code": 200,	// 错误代码
  "resultStatus": "Success",	// 请求状态，成功或失败
  "message": "foo bar",	// 服务器返回的信息，通常包含错误信息提示
  "qureyStatus":[	// 数组查询状态结果
	  {
		"orderType": "Inbound Order",	// 要查询的订单类型
		"reference": "TEST-1",	// 要查询的订单号
		"status": "Success",	// 查询结果状态
		"message": "Success"	// 异常信息
	  },
	  {
		"orderType": "Inbound Order",
		"reference": "TEST-2",
		"status": "Failed",
		"message": "TEST-2 was not found in system."
	  },
	  {
		"orderType": "Inbound Order",
		"reference": "TEST-3",
		"status": "Failed",
		"message": "TEST-2 was not found in system."
	  }
  ],
  "qureyResults":{
		"inboundOrders": [
			{
				"status": "Old Order",
				"container": "OOCU7921482"
			}
		],
		"outboundOrders":[
			{
				"shipOrderNumber": "GC2702-7052684",
				"status": "New Order"
			}
		]
  	}
}
```

#### 状态代码列表

`code: 200`: 操作成功

`code：500`: 无效的`AppKey`

`code：501`: 无效的签名（签名验证失败）

`code：503`: Request Body中的模型验证失败

`code：504`: 重复请求被拦截，注意检查request Id的唯一性

`code：505`: 无效的API版本号

------------

## 6.	DELETE删除订单状态接口

### 6.1	拼接地址及参数

`api/FBADeleteAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo`

### 6.2 请求Body

只有当订单存在系统中且订单状态为`Draft`的时候才能删除。否则需要联系客服删除。

```JavaScript
{
	"orderType": "Inbound",	// "Inbound"或者"Outbound"
	"reference": "FOO" // 需要删除的单号
}
```

### 6.3 回执 

当操作完成或遇到异常，系统会返回一个`Json`对象，包括请求状态、错误代码、错误信息和详细错误信息。

#### 回执对象结构：

``` JavaScript
{
	"code": "200",
	"message": "Delete Success!"
}
```

#### 状态代码列表

`code: 200`: 操作成功

`code: 404`: 没有找到要删除的订单号或该订单不符合删除条件

`code：500`: 无效的`AppKey`

`code：501`: 无效的签名（签名验证失败）

`code：503`: Request Body中的模型验证失败

`code：504`: 重复请求被拦截，注意检查request Id的唯一性

`code：505`: 无效的API版本号

------------
