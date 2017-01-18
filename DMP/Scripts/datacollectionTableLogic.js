
if (DataCollectionArrayFromServer.length > 0) {
    var datacollection = {};
    for (var c = 0; c < DataCollectionArrayFromServer.length; c++) {
        datacollection = DataCollectionArrayFromServer[c];
        var tL = datacollection.DataCollectionTimelines;
        var timeLines = [];
        for (var i = 0; i < tL.length; i++) {
            var dt = tL[i].toString();
            if (dt.indexOf("Date") !== -1) {
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                var dtt = new Date(dt);
                timeLines.push(dtt);
            }
        }
        datacollection.DataCollectionTimelines = timeLines;
        datacollectionArray.push(datacollection);
        CreatedatacollectionTable(datacollection);
    }
}


$("#adddatacollectionbtn").click(function (e) {
    var datacollection = {};
    datacollection["Id"] = datacollectionArray.length + 1;
    var dateStringArray = [];

    $("#datacollectionForm input, select").each(function () {
        if ($(this)[0].id == "DataType") {
            datacollection["DataType"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataSources") {
            datacollection["DataSources"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataCollectionTimelines") {
            dateStringArray = $(this)[0].value.split(',');
            datacollection["DataCollectionTimelines"] = dateStringArray;
            $(this).val("");
            $('#DataCollectionTimelines').multiDatesPicker('resetDates', 'picked');
        }
        else if ($(this)[0].id == "FrequencyOfDataCollection") {
            datacollection["FrequencyOfDataCollection"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DurationOfDataCollection") {
            datacollection["DurationOfDataCollection"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "DataCollectionAndReportingTools") {
            datacollection["DataCollectionAndReportingTools"] = $(this)[0].value;
            $(this).val("");
        }
    });

    datacollectionArray.push(datacollection);
    CreatedatacollectionTable(datacollection);
});

function CreatedatacollectionTable(datacollection) {
    var dateArray = datacollection.DataCollectionTimelines;
    var removeId = "remove" + datacollection.Id;
    var timelines = '';
    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    for (var i = 0; i < dateArray.length; i++) {
        var dt = dateArray[i].toString();
        if (dt.indexOf("Date") !== -1) {
            dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
        }
        var dtt = new Date(dt);
        dt = dtt.getDate() + '-' + months[dtt.getMonth()] + '-' + dtt.getFullYear();
        timelines += dt + " &#013;";
    }

    var html = '<tr id=datacollection' + datacollection.Id + '>';
    html += '<td>' + datacollection.NameOfdatacollection + '</td>';
    html += '<td>' + datacollection.ThematicArea + '</td>';
    html += '<td>' + datacollection.FrequencyOfdatacollectioning + '</td>';
    html += '<td>' + datacollection.DurationOfdatacollectioning + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + datacollection.Id + ',"datacollection")"  id=' + removeId + '/></td>';
    html += '</tr>';
    $('#datacollectiondataTable').append(html);
}