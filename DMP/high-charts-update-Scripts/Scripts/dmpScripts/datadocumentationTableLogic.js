﻿
if (ddocDataCollectionArrayFromServer != null && ddocDataCollectionArrayFromServer.length > 0) {
    var datadocumentation = {};
    for (var c = 0; c < ddocDataCollectionArrayFromServer.length; c++) {
        datadocumentation = ddocDataCollectionArrayFromServer[c];
        datadocumentation.Id = c + 1;
        ddocDatacollectionArray.push(datadocumentation);
        CreateddocTable(datadocumentation);
    }
}

$("#addddocbtn").click(function (e) {
    var datadocumentation = {};
    datadocumentation["Id"] = ddocDatacollectionArray.length + 1;
    var dateStringArray = [];

    $("#DataDocumentationManagementAndEntry input, select, textarea").each(function () {
        if ($(this)[0].id == "ddocThematicArea") {
            let th = '';
            $("#ddocThematicArea option:selected").each(function () {
                th += $(this).val() + ',';
            });
            datadocumentation["ThematicArea"] = th;
            $("#ddocThematicArea").select2('val', 'All');
        }
        else if ($(this)[0].id == "ddocReportingLevel") {
            let reportinglevel = '';
            $("#ddocReportingLevel option:selected").each(function () {
                reportinglevel += $(this).val() + ',';
            });
            datadocumentation["ReportingLevel"] = reportinglevel;
            $("#ddocReportingLevel").select2('val', 'All');
        }
        else if ($(this)[0].id == "StoredDocumentationAndDataDescriptors") {
            datadocumentation["StoredDocumentationAndDataDescriptors"] = $(this)[0].value;
            $(this).val("");
        }

        else if ($(this)[0].id == "NamingStructureAndFilingStructures") {
            datadocumentation["NamingStructureAndFilingStructures"] = $(this)[0].value;
            $(this).val("");
        }

    });
    if (datadocumentation.ThematicArea != null && datadocumentation.NamingStructureAndFilingStructures != null && datadocumentation.ReportingLevel != null) {
        ddocDatacollectionArray.push(datadocumentation);
        CreateddocTable(datadocumentation);
    }
});

function CreateddocTable(datadocumentation) {

    var datadoccollectionId = "datadocumentationx" + datadocumentation.Id;
    var html = '<tr id=' + datadoccollectionId + '>';
    html += '<td>' + datadocumentation.ReportingLevel + '</td>';
    html += '<td>' + datadocumentation.ThematicArea + '</td>';
    html += '<td>' + datadocumentation.StoredDocumentationAndDataDescriptors + '</td>';
    html += '<td>' + datadocumentation.NamingStructureAndFilingStructures + '</td>';
    if (editMode) {
        html += '<td><input type="button" value="Edit" class="btn btn-info btn-sm btn-outline" onclick="MakeEditable(' + datadoccollectionId + ')"/></td>';
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="Deleteddoc(' + datadoccollectionId + ')"/></td>';
    }
    html += '</tr>';
    $('#ddocTable').append(html);
}

function Deleteddoc(row) {

    if (row.id.indexOf("datadocumentation") !== -1) {
        var rowid = row.id.split('x')[1];
        $(row).remove();
        ddocDatacollectionArray.splice(rowid - 1, 1); 
    }
}