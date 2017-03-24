$("#form").submit(function () {

    var formData = new FormData($(this)[0]);

    $.ajax({
        url: baseUrl()+ '/dqaapi/post',
        type: 'POST',
        data: formData,
        async: true,
        success: function (data) {
            $('#output').html(data);          
        },
        cache: false,
        contentType: false,
        processData: false
    });


    return false;
});


function getIPDQA(ip) {
    $.ajax({
        url: baseUrl()+"dqaapi/GetIpDQA/"+ip,
        method: "GET",
        contentType: "application/json",
        success: function (data) {
            var table_value="";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'>";
                table_value += "<td>" + data[i].Id + "</td><td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td><td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td>";
                table_value += "<td>" + data[i].FiscalYear + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();
        }

    })
}

function searchIPDQA(ip) {
    var dt = { ip: ip, lga: $("#lga").val(), state: $("#states").val(), period: $("#period").val() };
    $.ajax({
        url: baseUrl() + "dqaapi/SearchIPDQA/" + ip,
        method: "POST",
        contentType: "application/json",
        success: function (data) {
            var table_value = "";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'>";
                table_value += "<td>" + data[i].Id + "</td><td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td><td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td>";
                table_value += "<td>" + data[i].FiscalYear + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").empty();
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();
        }

    })
}
function deleteDQA(metadataId) {
    var r = confirm("you are sure ?");
    if (r) {
        $.ajax({
            url: baseUrl() + "dqaapi/delete/" + metadataId,
            method: "DELETE",
            success: function (data) {
                $("#tr_" + metadataId).remove();
                alert("DQA data for the facility has been removed");
            }
        });
    }   

}

function getDQA(metadataId) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetDQAReport/" + metadataId,
        method: "GET",
        contentType:"application/json",
        success: function (data) {
            var jsonHtmlTable = ConvertJsonToTable(data.Table1, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable1").html(jsonHtmlTable);

            jsonHtmlTable = ConvertJsonToTable(data.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);

            $(".panel-title").text(data.Table2[0].FacilityName);
        }
    })
}

function getAllIPDQASummary() {
    $.ajax({
        url: baseUrl() + "dqaapi/GetSummaryResult",
        method: "GET",
        contentType: "application/json",
        success: function (data) {
            var jsonHtmlTable = ConvertJsonToTable(data.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);
            var tbl = "";
            var ips = data.Table1;
            for (var i = 0; i < ips.length; i++) {
                tbl += "<tr><td><a href='/dqa/IPDQAResult/" + ips[i].Id + "'>" + ips[i].Name + "</a></td>";
                tbl += "<td>" + ips[i].ShortName + "</td>";
                tbl += "<td>" + ips[i].Address + "</td>";
                tbl += "<td>" + ips[i].PhoneNumber + "</td></tr>";

            }

            $("#output tbody").empty();
            $("#output tbody").append(tbl);
            $('#output').DataTable();
        }
    })
}

function getIPDQASummaryAdmin(id) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetIpSummaryResult/"+id,
        method: "GET",
        contentType: "application/json",
        success: function (result) {
            var jsonHtmlTable = ConvertJsonToTable(result.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);

            var data = result.Table1;
            var table_value = "";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'>";
                table_value += "<td>" + data[i].Id + "</td><td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td><td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td>";
                table_value += "<td>" + data[i].FiscalYear + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();
           
        }
    })
}

function getIPDQASummary(id) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetIpSummaryResult/" + id,
        method: "GET",
        contentType: "application/json",
        success: function (result) {
            var jsonHtmlTable = ConvertJsonToTable(result.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);

            var data = result.Table1;
            var table_value = "";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'>";
                table_value += "<td>" + data[i].Id + "</td><td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td><td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td>";
                table_value += "<td>" + data[i].FiscalYear + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                //table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();

        }
    })
}