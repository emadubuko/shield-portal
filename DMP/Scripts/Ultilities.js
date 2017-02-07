
var base64Image;
function imagePreview(e, control) {
    var input = e;

    var reader = new FileReader();
    reader.onload = function () {
        base64Image = reader.result;
        if (control != undefined) {
            control.attr('src', base64Image);
        }
    };
    reader.readAsDataURL(input.files[0]);
}

function BindImageToControl(control, base64ImageString) {
   // control.attr('src', "/^data:image\/(png|jpg|x-icon);base64,/" + btoa(String.fromCharCode(...new Uint8Array(base64ImageString))));
    control.attr('src', "data:image/jpg;base64," + btoa(String.fromCharCode(...new Uint8Array(base64ImageString))));

    //$('#imageURL').attr('src', "data:image/jpg;base64,"+ btoa(String.fromCharCode(...new Uint8Array(selectedOrg.Logo))));
}

function BindPureBase64StringToControl(control, base64ImageString) {
    control.attr('src', base64ImageString);
}


function uploadfileToServer(e, control, url, controlToUpdate) {   
    var data = new FormData();
    try {
        var file = $(control)[0].files[0];
        data.append('file', file);
    }
    catch (err) {
        alert(err);
        return false;
    }

    $.ajax({
        type: "POST",
        url: url,
        contentType: false,
        processData: false,
        data: data,
        cache: false,
    }).done(function (result) {
        try{
            CreateRolesTable(result.roles);
            CreateRespTable(result.responsibility);
            CreateTrainingTable(result.trainings);
            controlToUpdate.val(result.filelocation);
        }
        catch (er) {
            console.log(er);
        }
    }).error(function (xhr, status, err) {
        alert(err);
    });
    return false;
}

function CreateRolesTable(roles) {
    $('#rolesTable').children().remove();

    rolesArray = roles;
    for (var i = 0; i < roles.length; i++) {
        var html = '<tr>';
        html += '<td>' + roles[i].Name + '</td>';
        html += '<td>' + roles[i].SiteCount + '</td>';
        html += '<td>' + roles[i].RegionCount + '</td>';
        html += '<td>' + roles[i].HQCount + '</td>';
        html += '</tr>';
        $('#rolesTable').append(html);
    }    
}

function CreateRespTable(resps) {
    $('#respsTable').children().remove();

    responsibilityArray = resps;
    for (var i = 0; i < resps.length; i++) {
        var html = '<tr>';
        html += '<td>' + resps[i].Name + '</td>';
        html += '<td>' + resps[i].SiteCount + '</td>';
        html += '<td>' + resps[i].RegionCount + '</td>';
        html += '<td>' + resps[i].HQCount + '</td>';
        html += '</tr>';
        $('#respsTable').append(html);
    }
}

function CreateTrainingTable(trainings) {
    $('#trainingTable').children().remove();

    trainingArray = trainings;
    for (var i = 0; i < trainings.length; i++) {
        var html = '<tr>';

        html += '<td>' + trainings[i].NameOfTraining + '</td>';
        html += '<td>' + trainings[i].SiteDisplayDate + '</td>';
        html += '<td>' + trainings[i].RegionDisplayDate + '</td>';
        html += '<td>' + trainings[i].HQDisplayDate + '</td>';
        html += '</tr>';
        $('#trainingTable').append(html);
    }
}
 