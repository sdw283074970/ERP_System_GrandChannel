$(document).ready(function () {

	var index;
	var customerId;
	var table;
	var url;

	$(".iframe-export, .iframe-customer").hide();

	$.ajax({
		type: "GET",
		url: "/api/fba/FBAindex/",
		contentType: 'application/json; charset=utf-8',
		dataType: "json",
		beforeSend: function () {
			index = layer.msg('Processing...', {
				icon: 1,
				shade: 0.01,
				time: 99 * 1000
			});
		},
		success: function (data) {
			layer.close(index);

			$("#table").DataTable({
				destroy: true,
				data: data,
				scrollCollapse: true,
				scrollY: "600px",
				scrollX: true,
				order: [[0, "desc"]],
				iDisplayLength: 100,
				columns: [
					{
						data: "id",
						render: function (data) {
							return "<font>" + data + "</font>";
						}
					},
					{
						data: "customerCode",
						render: function (data) {
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "name",
						render: function (data) {
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "telNumber",
						render: function (data) {
							if (data == null)
								return "";
							return "<text>" + data + "</text>";
						}
					},
					//{
					//    data: "emailAddress",
					//    render: function (data) {
					//        if (data == null)
					//            return "";
					//        return "<text>" + data + "</text>";
					//    }
					//},
					{
						data: "contactPerson",
						render: function (data) {
							if (data == null)
								return "";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "processingPlts",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "processingCtns",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "instockPlts",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "instockCtns",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "warningQuantityLevel",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<font color='red'>" + data + "</font>";
						}
					},
					{
						data: "payableInvoices",
						render: function (data) {
							if (data == 0)
								return "<text>-</text>";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "status",
						render: function (data) {
							if (data == "Active")
								return "<font color='green'>" + data + "</font>";
							else
								return "<font color='red'>" + data + "</font>";
						}
					},
					{
						data: "id",
						render: function (data) {
							return "<button iid='" + data + "' class='btn btn-info btn-receivingorders'>Rcv Orders</button><button iid='" + data + "' class='btn btn-info btn-shippingorders' style='margin-left:5px'>Shp Orders</button><button iid='" + data + "' class='btn btn-info btn-manageinventory' style='margin-left:5px'>Ivtr Report</button><button iid='" + data + "' class='btn btn-info btn-export' style='margin-left:5px'>Other Reports</button><button iid='" + data + "' class='btn btn-info btn-manage' style='margin-left:5px'>Manage</button>";
						}
					},
				]
			});
		},
		error: function (XMLHttpRequest, textStatus, errorThrown) {
			layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
				window.location.reload();
			});
		}
	});

	//点击管理收货订单，跳转到该用户的说活订单页面
	$("#table").on("click", ".btn-receivingorders", function () {
		var customerId = $(this).attr('iid');
		$(window.location).attr('href', '/fba/masterorder/?customerId=' + customerId);
	});

	//点击管理客户，跳转到客户管理页面
	$("#btn-customer").on("click", function () {
		$(window.location).attr('href', '/customer');
	});

	//点击管理shipping order，跳转到该用户的运单页面
	$("#table").on("click", ".btn-shippingorders", function () {
		var customerId = $(this).attr('iid');
		$(window.location).attr('href', '/shiporder/fbaShipOrder/?customerId=' + customerId);
	});

	//点击库存报告按钮，跳转到该用户当前的库存统计页面
	$("#table").on("click", ".btn-manageinventory", function () {
		alert("Comming soon!");
	});

	//点击管理按钮，打开管理客户基本信息弹窗
	$("#table").on("click", ".btn-manage", function () {
		customerId = $(this).attr('iid');
		url = '/api/fba/fbaindex/?customerId=' + customerId;

		$.ajax({
			type: "GET",
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			beforeSend: function () {
				layer.close(index);
				index = layer.msg('Processing...', {
					icon: 3,
					shade: 0.01,
					time: 99 * 1000
				});
			},
			success: function (data) {
				layer.close(index);
				$("#input-firstaddress").val(data.firstAddressLine);
				$("#input-secondaddress").val(data.secondAddressLine);

				$("#input-tel").val(data.telNumber);
				$("#input-email").val(data.emailAddress);
				$("#input-person").val(data.contactPerson);

				$("#input-warning").val(data.warningQuantityLevel);

				index = layer.open({
					title: false,
					type: 1,
					shadeclose: true,
					area: ["500px", "400px"],
					content: $(".iframe-customer")
				});
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
					window.location.reload();
				});
			}
		});
	});

	//点击导出收费汇总表按钮，弹出选择导出日期界面
	$("#table").on("click", ".btn-export", function () {
		customerId = $(this).attr('iid');

		index = layer.open({
			title: false,
			type: 1,
			shadeclose: true,
			area: ["400px", "250px"],
			content: $(".iframe-export")
		});
	});

	$("#btn-export").on("click", function () {
		customerId = $(this).attr('iid');

		index = layer.open({
			title: false,
			type: 1,
			shadeclose: true,
			area: ["400px", "250px"],
			content: $(".iframe-export")
		});
	});

	//点击导出收费总表弹窗的下载按钮，下载Excel表格
	$("#btn-download").on("click", function () {
		var startDate = $("#input-date-start").val();
		var closeDate = $("#input-date-close").val();

		if (startDate != "" && closeDate != "") {
			url = "/api/fba/fbaindex/?customerId=" + customerId + "&startDate=" + startDate + "&closeDate=" + closeDate;

			$.ajax({
				type: "GET",
				url: url,
				contentType: 'application/json; charset=utf-8',
				dataType: "json",
				beforeSend: function () {
					layer.close(index);
					index = layer.msg('Processing...', {
						icon: 3,
						shade: 0.01,
						time: 99 * 1000
					});
				},
				success: function (data) {
					layer.alert("Success! File will download automatically.");
					$(window.location).attr('href', "/api/fba/FBAReportDonwload/?fullPath=" + encodeURIComponent(data).toString() + "&prefix=InvoiceReport&suffix=" + encodeURIComponent(".xls"));
				},
				error: function (XMLHttpRequest, textStatus, errorThrown) {
					layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
						window.location.reload();
					});
				}
			});
		}
	});

	//点击update按钮，更新客户信息
	$("#btn-update").on("click", function () {
		var firstAddressLine = $("#input-firstaddress").val().toString();
		var secondAddressLine = $("#input-secondaddress").val().toString();

		var telNumber = $("#input-tel").val().toString();
		var emailAddress = $("#input-email").val().toString();
		var contactPerson = $("#input-person").val().toString();

		var warningQuantityLevel = $("#input-warning").val() == "" ? 0 : $("#input-warning").val();

		$.ajax({
			type: "PUT",
			url: "/api/customermanagement/?customerId=" + customerId + "&firstAddressLine=" + firstAddressLine + "&secondAddressLine=" + secondAddressLine + "&telNumber=" + telNumber + "&emailAddress=" + emailAddress + "&contactPerson=" + contactPerson + "&warningQuantityLevel=" + warningQuantityLevel,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			success: function () {
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
	});

	$(".btn-back").on("click", function () {
		window.history.back(-1);
	});
});