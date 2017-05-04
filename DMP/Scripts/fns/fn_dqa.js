$("#form").submit(function () {

    var formData = new FormData($(this)[0]);

    $.ajax({
        url: baseUrl() + '/dqaapi/post',
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
        url: baseUrl() + "dqaapi/GetIpDQA/" + ip,
        method: "GET",
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
        contentType: "application/json",
        success: function (data) {
            var jsonHtmlTable = ConvertJsonToTable(data.Table1, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable1").html(jsonHtmlTable);

            jsonHtmlTable = ConvertJsonToTable(data.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);

            $(".panel-title").text(data.Table2[0].FacilityName);
        }
    })
}

function getAllIPDQASummary(table_container,year,quarter) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetSummaryResult?year="+year +"&quarter="+quarter,
        method: "GET",
        contentType: "application/json",
        success: function (data) {

            var tbl = "";
            if (data.Table1 != undefined) {                
                var ips = data.Table1;
                for (var i = 0; i < ips.length; i++) {
                    tbl += "<tr><td><a href='/dqa/IPDQAResult/" + ips[i].Id + "'>" + ips[i].Name + "&nbsp; (" + ips[i].ShortName + ")" + "</a></td>";
                }
            }            

            $("#ipdqatbl tbody").empty();
            $("#ipdqatbl tbody").append(tbl);
            $('#ipdqatbl').DataTable();
            $("#" + table_container).css('display', 'block')
        }
    })
}

function getIPDQASummary(id, table_container,year,quarter) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetIpSummaryResult/?year="+year +"&quarter="+quarter+"&id=" + id,
        method: "GET",
        contentType: "application/json",
        success: function (result) {
            //var jsonHtmlTable = ConvertJsonToTable(result.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            //$("#divTable").html(jsonHtmlTable);

            var table_value = "";
            if (result != undefined && result.Table1 != undefined) {
                var data = result.Table1;               
                for (var i = 0; i < data.length; i++) {
                    table_value += "<tr id='tr_" + data[i].Id + "'>";
                    table_value += "<td>" + data[i].Id + "</td><td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td><td>" + data[i].LgaId + "</td>";
                    table_value += "<td>" + data[i].StateId + "</td>";
                    table_value += "<td>" + data[i].FiscalYear + "</td>";
                    table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                    //table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                    table_value += "</tr>";
                }
            }

            $("#facilitytbl tbody").prepend(table_value);
            $('#facilitytbl').DataTable();
            $("#" + table_container).css('display', 'block');
        }
    })
}

function getIPDQASummaryAdmin(id, year, quarter) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetIpSummaryResult/?year="+year +"&quarter="+quarter+"&id=" + id, //"dqaapi/GetIpSummaryResult/" + id,
        method: "GET",
        contentType: "application/json",
        success: function (result) {
            var jsonHtmlTable = ConvertJsonToTable(result.Table, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);

            var data = result.Table1;
            var table_value = "";
            for (var i = 0; i < data.length; i++) {
                table_value += "<tr id='tr_" + data[i].Id + "'>";
                table_value += "<td><a href='/DQA/GetDQA/" + data[i].Id + "'>" + data[i].SiteId + "</a></td>";
                table_value += "<td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td>";
                table_value += "<td>" + data[i].FiscalYear + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                table_value += "<td><button class='btn btn-danger btn-sm'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();

        }
    })
}

