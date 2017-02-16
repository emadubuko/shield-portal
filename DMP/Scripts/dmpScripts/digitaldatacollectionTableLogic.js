
if (digitalDataCollectionArrayFromServer != null && digitalDataCollectionArrayFromServer.length > 0) {
    var digitaldata = {};
    for (var c = 0; c < digitalDataCollectionArrayFromServer.length; c++) {
        digitaldata = digitalDataCollectionArrayFromServer[c];
        digitalDatacollectionArray.push(digitaldata);
        CreatedgtTable(digitaldata);
    }
}

$("#adddgtbtn").click(function (e) {
    var digitaldata = {};
    digitaldata["Id"] = digitalDatacollectionArray.length + 1; 

    $("#DigitalPnl input, select, textarea").each(function () {
        if ($(this)[0].id == "dgtThematicArea") {
            let th = '';
            $("#dgtThematicArea option:selected").each(function () {
                th += $(this).val() + ',';
            });
            digitaldata["ThematicArea"] = th;
            $("#dgtThematicArea").select2('val', 'All');
        }
        else if ($(this)[0].id == "dgtReportingLevel") {
            let reportinglevel = '';
            $("#dgtReportingLevel option:selected").each(function () {
                reportinglevel += $(this).val() + ',';
            });
            digitaldata["ReportingLevel"] = reportinglevel;
            $("#dgtReportingLevel").select2('val', 'All');
        }
        else if ($(this)[0].id == "VolumeOfdigitalData") {
            digitaldata["VolumeOfDigitalData"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataStorageFormat") {
            let dstf = '';
            $("#DataStorageFormat option:selected").each(function () {
                dstf += $(this).val() + ',';
            });
            digitaldata["DataStorageFormat"] = dstf;
            $("#DataStorageFormat").select2('val', 'All');
        }
        else if ($(this)[0].id == "Storagelocation") {
            digitaldata["StorageLocation"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "Backup") {
            digitaldata["Backup"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "Datasecurity") {
            digitaldata["DataSecurity"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "Patientconfidentialitypolicies") {
            digitaldata["PatientConfidentialityPolicies"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "StorageOfPreExistingData") {
            digitaldata["StorageOfPreExistingData"] = $(this)[0].value;
            $(this).val("");
        }
    });
    if (digitaldata.ThematicArea != null && digitaldata.DataStorageFormat != null && digitaldata.ReportingLevel != null) {
        digitalDatacollectionArray.push(digitaldata);
        CreatedgtTable(digitaldata);
    }
});

function CreatedgtTable(digitaldata) {

    var dgtid = "digitalx" + digitaldata.Id;
    var html = '<tr id=' + dgtid + '>';
    html += '<td>' + digitaldata.ReportingLevel + '</td>';
    html += '<td>' + digitaldata.ThematicArea + '</td>';
    html += '<td>' + digitaldata.VolumeOfDigitalData + '</td>';
    html += '<td>' + digitaldata.DataStorageFormat + '</td>';
    html += '<td>' + digitaldata.StorageLocation + '</td>';
    html += '<td>' + digitaldata.Backup + '</td>';
    html += '<td>' + digitaldata.DataSecurity + '</td>';
    html += '<td>' + digitaldata.PatientConfidentialityPolicies + '</td>';
    html += '<td>' + digitaldata.StorageOfPreExistingData + '</td>';
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="Deletedgt(' + dgtid + ')"/></td>';
    }
    html += '</tr>';
    $('#digitaldataTable').append(html);
}

function Deletedgt(row) {
    if (row.id.indexOf("digital") !== -1) {
        var rowid = row.id.split('x')[1];
        $(row).remove();
        digitalDatacollectionArray.splice(rowid - 1, 1);
    }
}