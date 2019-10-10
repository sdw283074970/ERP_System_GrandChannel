$(document).ready(function () {

	$(".iframe, .iframe-detail, .iframe-wo").hide();

	var customerCode;
	var index;
	var index_wo;
	var index_detail
	var table;
	var table_wo;
	var url;
	var searchBar;
	var customerId;
	var customerCode;
	var instructionColumnArray = [
		{
			data: "id",
			render: function (data) {
				return "<text>" + data + "</text>";
			}
		},
		{
			data: "description",
			render: function (data) {
				return "<text>" + data + "</text>";
			}
		},
		{
			data: "isApplyToShipOrder",
			render: function (data) {
				if (data == true)
					return "<text>√<text>";
				return "<text>X</text>";
			}
		},
		{
			data: "isApplyToMasterOrder",
			render: function (data) {
				if (data == true)
					return "<text>√<text>";
				return "<text>X</text>";
			}
		},
		{
			data: "status",
			render: function (data) {
				return "<text>" + data + "</text>";
			}
		},
		{
			data: "createDate",
			render: function (data) {
				return "<text>" + data.toString().substring(0, 10) + "</text>";
			}
		},
		{
			data: "createBy",
			render: function (data) {
				return "<text>" + data + "</text>";
			}
		},
		{
			data: "id",
			render: function (data) {
				return "<button iid='" + data + "' class='btn btn-link btn-detail-edit'>Edit</button><button iid='" + data + "' class='btn btn-link btn-detail-delete'>Delete</button>";
			}
		}
	];

	$.ajax({
		type: "GET",
		url: "/api/customermanagement/",
		contentType: 'application/json; charset=utf-8',
		dataType: "json",
		success: function (data) {
			$("#table").DataTable({
				data: data,
				order: [[0, "desc"]],
				destroy: true,
				//scrollX: true,
				scrollCollapse: true,
				scrollY: "600px",
				iDisplayLength: 100,
				columns: [
					{
						data: "id",
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
						data: "customerCode",
						render: function (data) {
							if (data == null)
								return "";

							customerCode = data;

							return "<text>" + data + "</text>";
						}
					},
					{
						data: "departmentCode",
						render: function (data) {
							if (data == null)
								return "";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "firstAddressLine",
						render: function (data) {
							if (data == null)
								return "";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "secondAddressLine",
						render: function (data) {
							if (data == null)
								return "";
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
					{
						data: "emailAddress",
						render: function (data) {
							if (data == null)
								return "";
							return "<text>" + data + "</text>";
						}
					},
					{
						data: "contactPerson",
						render: function (data) {
							if (data == null)
								return "";
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
							return "<button iid='" + data + "' customercode='" + customerCode + "' class='btn btn-link btn-charge'>Mange Charge Items</button><button iid='" + data + "' class='btn btn-deactive btn-link'>Active/Deactive</button><button iid='" + data + "' customercode='" + customerCode + "' class='btn btn-link btn-wo'>Mange WO Templite</button><button iid='" + data + "' class='btn btn-delete btn-link'>Delete</button>";
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

	//打开创建新用户弹窗
	$("#btn-create").click(function () {
		index = layer.open({
			title: false,
			type: 1,
			shadeclose: true,
			area: ["500px", "550px"],
			content: $(".iframe")
		});
	})

	//点击Add按钮，添加新的客户
	$("#btn-add").on("click", function () {

		var name = $("#input-name").val().toString();
		var customerCode = $("#input-code").val().toString();
		var departmentCode = $("#select-code option:selected").val().toString();

		var firstAddressLine = $("#input-firstaddress").val().toString();
		var secondAddressLine = $("#input-secondaddress").val().toString();

		var telNumber = $("#input-tel").val().toString();
		var emailAddress = $("#input-email").val().toString();
		var contactPerson = $("#input-person").val().toString();

		var warningQuantityLevel = $("#input-warning").val() == "" ? 0 : $("#input-warning").val();

		$.ajax({
			type: "POST",
			url: "/api/customermanagement/?name=" + name + "&customerCode=" + customerCode + "&departmentCode=" + departmentCode + "&firstAddressLine=" + firstAddressLine + "&secondAddressLine=" + secondAddressLine + "&telNumber=" + telNumber + "&emailAddress=" + emailAddress + "&contactPerson=" + contactPerson + "&warningQuantityLevel=" + warningQuantityLevel,
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

	//点击Edit按钮，编辑客户信息(暂时弹出暂未开放功能)
	$("#table").on("click", ".btn-edit", function () {
		layer.alert("Comming soon!");
	});

	//点击管理收费项目按钮，打开对应客户的收费项目
	$("#table").on("click", ".btn-charge", function () {
		$(window.location).attr('href', '/Customer/ChargeItemLists/?customerId=' + $(this).attr('iid') + "&customerCode=" + $(this).attr('customercode'));
	});

	//点击删除按钮，删除客户
	$("#table").on("click", ".btn-delete", function () {
		var id = $(this).attr("iid");

		$.ajax({
			type: "DELETE",
			url: "/api/customermanagement/" + id,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			success: function () {
				layer.alert("Delete success!", function () {
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

	//点击客户的管理WO模板按钮，弹出WO模板页面
	$("#table").on("click", ".btn-wo", function () {
		customerId = $(this).attr('iid');
		customerCode = $(this).parent().parent("tr").children("td").eq(2).children("text").html();
		$("#h2-customercode").html("Customer Code: " + customerCode);
		grandChannel.openiframe(index, ".iframe-wo", "1000px", "800px");

		url = "/api/customermanagement/?customerId=" + customerId;
		grandChannel.getAjaxMiniTable("GET", url, "#table-detail", table_wo, instructionColumnArray, index_wo);
	});

	//点击添加项目按钮，打开一个简易填写界面
	$("#btn-detail-create").on("click", function () {
		grandChannel.openiframe(index_detail, ".iframe-detail", "500px", "250px");
	});

	//点击添加按钮，添加WO指示模板
	$("#btn-detail-add").on("click", function () {
		var description = $("#input-detail-description").val();
		var isApplyToShipOrder = $("#cb-shiporder").prop("checked");
		var isApplyToMasterOrder = $("#cb-masterorder").prop("checked");
		var isAppliedToAll = $("#cb-all").prop("checked");
		var isChargingItem = $("#cb-charge").prop("checked");

		if (description != "") {
			url = "/api/CustomerManagement/?customerId=" + customerId + "&description=" + encodeURIComponent(description) + "&isChargingItem=" + isChargingItem + "&isAppliedToAll=" + isAppliedToAll + "&isApplyToShipOrder=" + isApplyToShipOrder + "&isApplyToMasterOrder=" + isApplyToMasterOrder;

			$.ajax({
				type: "POST",
				url: url,
				contentType: 'application/json; charset=utf-8',
				dataType: "json",
				success: function (data) {
					url = "/api/customermanagement/?customerId=" + customerId;
					grandChannel.getAjaxMiniTable("GET", url, "#table-detail", table_wo, instructionColumnArray, index_wo);
				},
				error: function (XMLHttpRequest, textStatus, errorThrown) {
					layer.alert(XMLHttpRequest.responseJSON.exceptionMessage, function () {
						window.location.reload();
					});
				}
			});
		}
		else
			alert("Description cannot be empty.");
	});

	//点击指示模板里的删除按钮，删除指定的模板条目
	$("#table-detail").on("click", ".btn-detail-delete", function () {
		var id = $(this).attr('iid');

		url = "/api/fba/customermanagement/?instructionId=" + id;

		$.ajax({
			type: "DELETE",
			url: url,
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			success: function () {
				url = "/api/customermanagement/?customerId=" + customerId;
				layer.alert("Success!", function () {
					grandChannel.getAjaxMiniTable("GET", url, "#table-detail", table_wo, instructionColumnArray, index_wo);
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