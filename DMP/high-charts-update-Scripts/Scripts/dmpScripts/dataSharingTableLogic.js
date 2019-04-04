
if (dasDataCollectionArrayFromServer != null && dasDataCollectionArrayFromServer.length > 0) {
    var dataSharing = {};
    for (var c = 0; c < dasDataCollectionArrayFromServer.length; c++) {
        dataSharing = dasDataCollectionArrayFromServer[c];
        dataSharing.Id = c + 1;
        dasDatacollectionArray.push(dataSharing);
        CreatedasTable(dataSharing);
    }
}

$("#adddasbtn").click(function (e) {
    var dataSharing = {};
    dataSharing["Id"] = dasDatacollectionArray.length + 1;
    var dateStringArray = [];

    $("#DataAccessansSharing input, select, textarea").each(function () {
        if ($(this)[0].id == "dasThematicArea") {
            let th = '';
            $("#dasThematicArea option:selected").each(function () {
                th += $(this).val() + ',';
            });
            dataSharing["ThematicArea"] = th;
            $("#dasThematicArea").select2('val', 'All');
        }
        else if ($(this)[0].id == "dasReportingLevel") {
            let reportinglevel = '';
            $("#dasReportingLevel option:selected").each(function () {
                reportinglevel += $(this).val() + ',';
            });
            dataSharing["ReportingLevel"] = reportinglevel;
            $("#dasReportingLevel").select2('val', 'All');
        }
        else if ($(this)[0].id == "DataAccess") {
            dataSharing["DataAccess"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataSharingPolicies") {
            dataSharing["DataSharingPolicies"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataTransmissionPolicies") {
            dataSharing["DataTransmissionPolicies"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "SharingPlatForms") {
            dataSharing["SharingPlatForms"] = $(this)[0].value;
            $(this).val("");
        }
    });
    if (dataSharing.ThematicArea != null && dataSharing.DataSharingPolicies != null && dataSharing.ReportingLevel != null) {
        dasDatacollectionArray.push(dataSharing);
        CreatedasTable(dataSharing);
    }
});

function CreatedasTable(dataSharing) {

    var dasId = "dataSharingx" + dataSharing.Id;
    var html = '<tr id=' + dasId + '>';
    html += '<td>' + dataSharing.ReportingLevel + '</td>';
    html += '<td>' + dataSharing.ThematicArea + '</td>';
    html += '<td>' + dataSharing.DataAccess + '</td>';
    html += '<td>' + dataSharing.DataSharingPolicies + '</td>';
    html += '<td>' + dataSharing.DataTransmissionPolicies + '</td>';
    html += '<td>' + dataSharing.SharingPlatForms + '</td>';
    if (editMode) {
        html += '<td><input type="button" value="Edit" class="btn btn-info btn-sm btn-outline" onclick="MakeEditable(' + dasId + ')"/></td>';
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="Deletedas(' + dasId + ')"/></td>';
    }
    html += '</tr>';
    $('#dasTable').append(html);
}

function Deletedas(row) {
    if (row.id.indexOf("dataSharing") !== -1) {
        var rowid = row.id.split('x')[1];
        $(row).remove();
        dasDatacollectionArray.splice(rowid - 1, 1);
    }
}