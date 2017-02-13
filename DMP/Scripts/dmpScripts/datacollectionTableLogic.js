
if (DataCollectionArrayFromServer !=null && DataCollectionArrayFromServer.length > 0) {
    var datacollection = {};
    for (var c = 0; c < DataCollectionArrayFromServer.length; c++) {
        datacollection = DataCollectionArrayFromServer[c];
        datacollectionArray.push(datacollection);
        CreatedatacollectionTable(datacollection);
    }
}


$("#adddatacollectionbtn").click(function (e) {
    var datacollection = {};
    datacollection["Id"] = datacollectionArray.length + 1;
    var dateStringArray = [];

    $("#datacollectionForm input, select, textarea").each(function () {
        if ($(this)[0].id == "DataType") {
            datacollection["DataType"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "dcReportingLevel") {
            datacollection["ReportingLevel"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataCollectionProcess") {
            datacollection["DataCollectionProcess"] = $(this)[0].value;
            $(this).val("");
        } 
        
        else if ($(this)[0].id == "DataCollectionAndReportingTools") {
            let selectedTools = '';
            $("#DataCollectionAndReportingTools option:selected").each(function () {
                selectedTools += $(this).val() + ',';
            });
            datacollection["DataCollectionAndReportingTools"] = selectedTools;
            $(this).val("");
        }
    });
    if (datacollection.DataCollectionProcess != null && datacollection.DataCollectionAndReportingTools != null && datacollection.ReportingLevel != null) {
        datacollectionArray.push(datacollection);
        CreatedatacollectionTable(datacollection);
    }
});

function CreatedatacollectionTable(datacollection) {
       
    var datacollectionId = "datacollectionx" + datacollection.Id;
    var html = '<tr id=' + datacollectionId + '>';
    html += '<td>' + datacollection.ReportingLevel + '</td>';
    html += '<td>' + datacollection.DataType + '</td>';
    html += '<td>' + datacollection.DataCollectionAndReportingTools + '</td>';
    html += '<td>' + datacollection.DataCollectionProcess + '</td>'; 
   
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + datacollectionId + ')"/></td>';
    }
    html += '</tr>';
    $('#datacollectiondataTable').append(html);
}