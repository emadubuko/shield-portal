$("form").submit(function () {

    var formData = new FormData($(this)[0]);

    $.ajax({
        url: '/Home/PostForm',
        type: 'POST',
        data: formData,
        async: false,
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
                table_value += "<tr id='tr_"+data[i].Id+"'>";
                table_value += "<td><a href='/DQA/GetDQA/"+data[i].Id+"'>" + data[i].Id + "</a></td><td>" + data[i].SiteId + "</td><td>" + data[i].LgaId + "</td>";
                table_value += "<td>" + data[i].StateId + "</td><td>" + data[i].LgaLevel + "<td></td>" + data[i].FundingAgency + "</td>";
                table_value += "<td>" + data[i].ImplementingPartner + "</td><td>" + data[i].FiscalYear + "</td><td>" + data[i].AssessmentWeek + "</td>";
                table_value += "<td>" + data[i].CreateDate + "</td><td>" + data[i].Month + "</td>";
                table_value += "<td><button class='btn btn-danger'  onclick='deleteDQA(" + data[i].Id + ")'><i class='fa fa-trash'></i> Delete</button></td>";
                table_value += "</tr>";
            }
            $("#output tbody").prepend(table_value);
            $('#output').DataTable();
        }

    })
}
function deleteDQA(metadataId) {
    $.ajax({
        url: baseUrl() + "dqaapi/delete/" + metadataId,
        method: "DELETE",
        success: function (data) {
            $("#tr_" + metadataId).remove();
        }
    });

}

function getDQA(metadataId) {
    $.ajax({
        url: baseUrl() + "dqaapi/GetDQAReport/" + metadataId,
        method: "GET",
        contentType:"application/json",
        success: function (data) {
            var jsonHtmlTable = ConvertJsonToTable(data, 'table', "table table-bordered table-striped table-hover", 'Download');
            $("#divTable").html(jsonHtmlTable);
        }
    })
}