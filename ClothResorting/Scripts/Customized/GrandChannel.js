//解析url中的参数函数
(function(){
	window.grandChannel = {};

	//获得浏览器URL地址中的参数
	var getUrlParameter = function (sParam) {

		var sPageURL = decodeURIComponent(window.location.search.substring(1)),
			sURLVariables = sPageURL.split('&'),
			sParameterName,
			i;

		for (i = 0; i < sURLVariables.length; i++) {
			sParameterName = sURLVariables[i].split('=');

			if (sParameterName[0] === sParam) {
				return sParameterName[1] === undefined ? true : sParameterName[1];
			}
		}
	};

	grandChannel.getUrlParameter = getUrlParameter;

	//打开iframe弹出窗口
	var openiframe = function (index, elementId, width, height) {

		index = layer.open({
			title: false,
			type: 1,
			shadeclose: true,
			area: [width, height],
			content: $(elementId)
		});
	};

	grandChannel.openiframe = openiframe;

	//发送Ajax方法
	var sendAjaxMethod = function (method, url, obj) {

		$.ajax({
			type: method,
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			data: JSON.stringify(obj),
			success: function () {
				layer.alert("Item has been put back.", function () {
					window.location.reload();
				});
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
					window.location.reload();
				});
			}
		});
	};

	grandChannel.sendAjaxMethod = sendAjaxMethod;
})();