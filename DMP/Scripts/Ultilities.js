
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
    control.attr('src', "data:image/jpg;base64," + btoa([].reduce.call(new Uint8Array(base64ImageString), function (p, c) { return p + String.fromCharCode(c) }, '')));
    //control.attr('src', "data:image/jpg;base64," + btoa(String.fromCharCode(new Uint8Array(base64ImageString))));

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
        try {
            CreateRolesTable(result.roles);
            CreateRespTable(result.responsibility);
            CreateTrainingTable(result.trainings);
            controlToUpdate.val(result.filelocation);
        }
        catch (er) {
            console.log(er);
        }
    }).error(function (xhr, status, err) {
        alert("System error while reading the file. Ensure you file structure is consistent with the sample file.");
        console.log(err);
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
function activateSticky() {
    var oldScrollFunction = window.onscroll;
    // When the user scrolls the page, execute myFunction 
    window.onscroll = function (ev) {
        checkSticky();
        if (oldScrollFunction)
            oldScrollFunction(ev);
    };

    // Get the header
    var $header = $(".sticky-header");
    //var $content = $(".sticky-content");

    // Get the offset position of the navbar
    var headerOffsetTop = $header.offset().top;
    //var contentOffsetTop = $content.offset().top;
    var hasSticky = false;

    // Add the sticky class to the header when you reach its scroll position. Remove "sticky" when you leave the scroll position
    function checkSticky() {
        if (window.pageYOffset > headerOffsetTop) {
            if (!hasSticky) {
                $header.addClass("sticky");
                //put an offset for the content
                //$content.offset({ top: contentOffsetTop, left: $content.offset().left });
                hasSticky = true;
            }
        } else if (hasSticky) {
            $header.removeClass("sticky");
            //$content.offset({ top: contentOffsetTop, left: $content.offset().left });
            hasSticky = false;
        }
    }
}

//export table as csv
function exportTableToCSV($table, filename, ignoredvalue, ignoredValueColumn, ignoredColumnIndex) {

    //rows containing ignoredvalue will be ignored,
    //ignoredValueColumn is the column where the ignoredvalue is expected
    //ignoredColumnIndex is totally excluded column


    var $rows = $table.find('tr:has(td),tr:has(th)'),

        // Temporary delimiter characters unlikely to be typed by keyboard
        // This is to avoid accidentally splitting the actual contents
        tmpColDelim = String.fromCharCode(11), // vertical tab character
        tmpRowDelim = String.fromCharCode(0), // null character

        // actual delimiter characters for CSV format
        colDelim = '","',
        rowDelim = '"\r\n"',

        // Grab text from table into CSV formatted string
        csv = '"' + $rows.map(function (i, row) {
            var $row = $(row), $cols = $row.find('td,th');

            //skip ignored values
            if (ignoredvalue) {
                if ($cols[ignoredValueColumn].innerHTML != ignoredvalue.trim()) {
                    return $cols.map(function (j, col) {
                        if (j != ignoredColumnIndex) {
                            var $col = $(col), text = $col.text();
                            return text.replace(/"/g, '""'); // escape double quotes 
                        }
                    }).get().join(tmpColDelim);
                }
            }
            else {
                return $cols.map(function (j, col) {
                    var $col = $(col), text = $col.text();
                    return text.replace(/"/g, '""'); // escape double quotes 
                }).get().join(tmpColDelim);
            }
             

        }).get().join(tmpRowDelim)
            .split(tmpRowDelim).join(rowDelim)
            .split(tmpColDelim).join(colDelim) + '"',

        // Data URI
        csvData = 'data:application/csv;charset=utf-8,' + encodeURIComponent(csv);
    //console.log(csv);
    if (window.navigator.msSaveBlob) {
        //alert('IE' + csv);
        window.navigator.msSaveOrOpenBlob(new Blob([csv], { type: "text/plain;charset=utf-8;" }), "csvname.csv")
    }
    else {
        $(this).attr({ 'download': filename, 'href': csvData, 'target': '_blank' });
    }


   
}