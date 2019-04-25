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
	var sendAjaxMethod = function (method, url, obj, index) {
		$.ajax({
			type: method,
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			data: JSON.stringify(obj),
			beforeSend: function () {
				index = layer.msg('Processing...', {
					icon: 1,
					shade: 0.01,
					time: 99 * 1000
				});
			},
			success: function (data) {
				layer.alert("Success!", function () {
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

	//发送Ajax方法并下载
	var sendAjaxMethodAndDownloadFileByFullPath = function (method, url, obj, index) {
		$.ajax({
			type: method,
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			data: JSON.stringify(obj),
			beforeSend: function () {
				layer.close(index);
				index = layer.msg('Processing...', {
					icon: 1,
					shade: 0.01,
					time: 99 * 1000
				});
			},
			success: function (fullPath) {
				layer.alert("Success! File will be downloaded automaticlly.");
				$(window.location).attr('href', "/api/fba/downloadfile/?fullPath=" + encodeURIComponent(fullPath).toString());
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
					window.location.reload();
				});
			}
		});
	};

	grandChannel.sendsendAjaxMethodAndDownloadFileByFullPath = sendAjaxMethodAndDownloadFileByFullPath;

	//发送Ajac方法并取得表
	var getAjaxTable = function (methodType, url, tableId, table, columsArray, index, searchBar) {
		$.ajax({
			type: methodType,
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			beforeSend: function () {
				layer.close(index);
				index = layer.msg('Processing...', {
					icon: 1,
					shade: 0.01,
					time: 99 * 1000
				});
			},
			success: function (data) {
				layer.close(index);
				if (table) {
					table.destroy();
				}

				table = $(tableId).DataTable({
					data: data,
					destroy: true,
					scrollX: true,
					order: [[0, "desc"]],
					scrollCollapse: true,
					scrollY: "600px",
					iDisplayLength: 100,
					columns: columsArray
				});

				var tableSearch = $("#table_filter").children('label').children('input');

				if (searchBar != undefined) {
					tableSearch.val(searchBar);
					table.search(searchBar).draw();
				}
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
					window.location.reload();
				});
			}
		});
	};

	grandChannel.getAjaxTable = getAjaxTable;
})();