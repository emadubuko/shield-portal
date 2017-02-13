
if (nondigitalDataCollectionArrayFromServer != null && nondigitalDataCollectionArrayFromServer.length > 0) {
    var nondigitaldata = {};
    for (var c = 0; c < nondigitalDataCollectionArrayFromServer.length; c++) {
        nondigitaldata = nondigitalDataCollectionArrayFromServer[c];
        nondigitalDatacollectionArray.push(nondigitaldata);
        CreatendgtTable(nondigitaldata);
    }
}

$("#addndgtbtn").click(function (e) {
    var nondigitaldata = {};
    nondigitaldata["Id"] = nondigitalDatacollectionArray.length + 1;
    var dateStringArray = [];

    $("#NonDigitalPnl input, select, textarea").each(function () {
        if ($(this)[0].id == "ndgtThematicArea") {
            nondigitaldata["ThematicArea"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "ndgtReportingLevel") {
            nondigitaldata["ReportingLevel"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "NonDigitalDataTypes") {
            nondigitaldata["NonDigitalDataTypes"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "NonDigitalStorageLocation") {
            nondigitaldata["StorageLocation"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "SafeguardsAndRequirements") {
            nondigitaldata["SafeguardsAndRequirements"] = $(this)[0].value;
            $(this).val("");
        } 
    });
    if (nondigitaldata.ThematicArea != null && nondigitaldata.NonDigitalDataTypes != null && nondigitaldata.ReportingLevel != null) {
        nondigitalDatacollectionArray.push(nondigitaldata);
        CreatendgtTable(nondigitaldata);
    }
});

function CreatendgtTable(nondigitaldata) {

    var nondgtid = "nondigitalx" + nondigitaldata.Id;
    var html = '<tr id=' + nondgtid + '>';
    html += '<td>' + nondigitaldata.ReportingLevel + '</td>';
    html += '<td>' + nondigitaldata.ThematicArea + '</td>';
    html += '<td>' + nondigitaldata.NonDigitalDataTypes + '</td>';
    html += '<td>' + nondigitaldata.StorageLocation + '</td>';
    html += '<td>' + nondigitaldata.SafeguardsAndRequirements + '</td>'; 
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="Deletendgt(' + nondgtid + ')"/></td>';
    }
    html += '</tr>';
    $('#ndigitaldataTable').append(html);
}

function Deletendgt(row) {
    if (row.id.indexOf("nondigital") !== -1) {
        var rowid = row.id.split('x')[1];
        $(row).remove();
        nondigitalDatacollectionArray.splice(rowid - 1, 1);
    }
}