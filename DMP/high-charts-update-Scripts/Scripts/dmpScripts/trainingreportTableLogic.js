
if (TrainingArrayFromServer !=null && TrainingArrayFromServer.length > 0) {
    var training = {};
    for (var c = 0; c < TrainingArrayFromServer.length; c++) {
        training = TrainingArrayFromServer[c];
        var tL = training.TimelinesForTrainings;
        var timeLines = [];

        if (tL != null) {
            for (var i = 0; i < tL.length; i++) {
                var dt = tL[i].toString();
                if (dt.indexOf("Date") !== -1) {
                    dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
                    var dtt = new Date(dt);
                    timeLines.push(dtt);
                }
            }
            training.TimelinesForTrainings = timeLines;
            training.Id = c + 1;
        }        

        trainingArray.push(training);
        CreatetrainingTable(training);
    }
}


$("#addtrainingbtn").click(function (e) {
    var training = {};
    training["Id"] = trainingArray[trainingArray.length - 1].Id + 1; //trainingArray.length + 1;
    var dateStringArray = [];

    $("#trainingForm input, select").each(function () {
        if ($(this)[0].id == "NameOfTraining") {
            training["NameOfTraining"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "TimelinesForTrainings") {
            if ($(this)[0].value == "") {
                alert("please select timelines")
                return;
            }
            dateStringArray = $(this)[0].value.split(',');
            training["TimelinesForTrainings"] = dateStringArray;
            $(this).val("");
            $('#TimelinesForTrainings').multiDatesPicker('resetDates', 'picked');
        }
        else if ($(this)[0].id == "FequencyOfTrainings") {
            training["FequencyOfTrainings"] = $(this)[0].value;
            $(this).val("");
        }
        else if ($(this)[0].id == "trDurationdrpdwn") {
            var durationDrpDwn = $(this)[0].value;
            var inputValue = $("#trDurationField")[0].value;
            var duration = inputValue;
            switch (durationDrpDwn) {
                case "Weeks":
                    duration = inputValue * 7; break;
                case "Months":
                    duration = inputValue * 30; break;
                case "Years":
                    duration = inputValue * 365; break;                
            }
            if (duration == "") {
                alert("please specify duration");
                return;
            }
            training["DurationOfTrainings"] = duration; // $(this)[0].value;
            $(this).val("");
        }
    });

    if (training.TimelinesForTrainings != null && training.DurationOfTrainings !=null) {
        trainingArray.push(training);
        CreatetrainingTable(training);
    }    
});

function CreatetrainingTable(training) {
    var dateArray = training.TimelinesForTrainings;
    var removeId = "remove" + training.Id;
    var timelines = '';
    if (dateArray != null) {
        for (var i = 0; i < dateArray.length; i++) {
            var dt = dateArray[i].toString();
            if (dt.indexOf("Date") !== -1) {
                dt = parseInt(dt.replace(/\/Date\((\d+)\)\//, '$1'));
            }
            var dtt = new Date(dt);
            dt = dtt.getDate() + '-' + months[dtt.getMonth()] + '-' + dtt.getFullYear();
            timelines += dt + " &#013;";
        }
    }

    var trainingId = "trainingx" + training.Id;
    var html = '<tr id=' + trainingId + '>';
    html += '<td>' + training.NameOfTraining + '</td>'; 
    html += '<td>' + training.FequencyOfTrainings + '</td>';
    html += '<td>' + training.DurationOfTrainings + '</td>';
    html += '<td><a class="btn btn-info btn-sm btn-outline" title=' + JSON.stringify(timelines) + '>View</a></td>';
    if (editMode) {
        html += '<td><input type="button" value="Remove" class="btn btn-danger btn-sm btn-outline" onclick="DeleteRow(' + trainingId + ')"  id=' + removeId + '/></td>';
    }
    html += '</tr>';
    $('#trainingTable').append(html);
}