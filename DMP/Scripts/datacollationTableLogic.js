
if (datacollationArrayFromServer !=null && datacollationArrayFromServer.length > 0) {
    var datacollation = {};
    for (var c = 0; c < datacollationArrayFromServer.length; c++) {
        datacollation = datacollationArrayFromServer[c]; 
        datacollationArray.push(datacollation);
        CreatedatacollationTable(datacollation);
    }
}


$("#adddatacollationbtn").click(function (e) {
    var datacollation = {};
    datacollation["Id"] = datacollationArray.length + 1;
    var dateStringArray = [];
    
    var rl = $("#dtReportingLevel")[0].value;
    datacollation["ReportingLevel"] = rl;
    $("#dtReportingLevel").val("");

    var cf = $("#CollationFrequency")[0].value;
    datacollation["CollationFrequency"] = cf;
    $("#CollationFrequency").val("");

    var dtype =  $("#DataType")[0].value;
    datacollation["DataType"] =dtype;
    $("#DataType").val("");
     
    if (dtype != "" && cf != "" && rl !="") {
        datacollationArray.push(datacollation);
        CreatedatacollationTable(datacollation);
    }    
});

function CreatedatacollationTable(datacollation) {
   
    var datacollationId = "datacollationx" + datacollation.Id;
    var html = '<tr id=' + datacollationId + '>';
    html += '<td>' + datacollation.ReportingLevel + '</td>';
    html += '<td>' + datacollation.DataType + '</td>'; 
    html += '<td>' + datacollation.CollationFrequency + '</td>';     
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + datacollationId + ')"/></td>';
    }
    html += '</tr>';
    $('#datacollationdataTable').append(html);
}