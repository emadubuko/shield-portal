
if (parseInt(ipscount) == 1) {
    $("#sltIP").prop("disabled", true);
}

var checkboxId = [];
$('#chckHead').click(function () {
    if (this.checked == false) {
        $('.chcktbl:checked').prop('checked', false);
        $.each($('.chcktbl:not(:checked)'), function (i, v) {
            var index = checkboxId.indexOf(parseInt(v.id));
            if (index > -1) {
                checkboxId.splice(index, 1);
            }
        });
    }
    else {
        checkboxId = []; //cleared the thing
        $('.chcktbl:not(:checked)').prop('checked', true);
        $.each($('.chcktbl:checked'), function (i, v) {
            var index = checkboxId.indexOf(parseInt(v.id));
            if (index < 0) {
                checkboxId.push(parseInt(v.id));
            }
        });
    }
});

function populate_all_controls() {
    var _buildLGA = [];
    var _buildFacility = [];
    var _buildState= [];
    var _buildIP = [];

    $.each(model_data, function (i, a) {
        if (checkforuniqueness(_buildIP, a.IP).length == 0) {
            _buildIP.push(a.IP);
            if (parseInt(ipscount) == 1) {
                $("#sltIP").append($('<option>', {
                    value: a.IP,
                    text: a.IP,
                    selected: true,
                }));
                $("#sltIP option[value=" + a.IP + "]");
            }
            else {
                $("#sltIP").append($('<option>', {
                    value: a.IP,
                    text: a.IP
                }));
            }
        }
        if (checkforuniqueness(_buildState, a.LGA.State.state_code).length == 0) {
            _buildState.push(a.LGA.State.state_code);
            $("#sltstate").append($('<option>', {
                value: a.LGA.State.state_code,
                text: a.LGA.State.state_name
            }));
        }
        if (checkforuniqueness(_buildLGA, a.LGA.lga_code).length == 0) {
            _buildLGA.push(a.LGA.lga_code);
            $("#sltLGA").append($('<option>', {
                value: a.LGA.lga_code,
                text: a.LGA.lga_name
            }));
        }
        if (checkforuniqueness(_buildFacility, a.FacilityName).length == 0) {
            _buildFacility.push(a.FacilityName);
            $("#sltFacility").append($('<option>', {
                value: a.FacilityName,
                text: a.FacilityName
            }));
        }
    });
}

//IP
$("#sltIP").change(function (e) {
    var _buildLGA_ = [];
    var _buildFacility_ = [];
    var _buildState_ = [];

    $("#sltLGA").html("");
    $("#sltFacility").html("");
    $("#sltstate").html("");
    $("#sltstate").select2("val", "");
    $("#sltLGA").select2("val", "");
    $("#sltFacility").select2("val", "");

    if (e.currentTarget.selectedOptions.length == 0) {
        $.each(model_data, function (i, a) {
            if (checkforuniqueness(_buildState_, a.LGA.State.state_code).length == 0) {
                _buildState_.push(a.LGA.State.state_code);
                $("#sltstate").append($('<option>', {
                    value: a.LGA.State.state_code,
                    text: a.LGA.State.state_name
                }));
            }
            if (checkforuniqueness(_buildLGA_, a.LGA.lga_code).length == 0) {
                _buildLGA_.push(a.LGA.lga_code);
                $("#sltLGA").append($('<option>', {
                    value: a.LGA.lga_code,
                    text: a.LGA.lga_name
                }));
            }
            if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                _buildFacility_.push(a.FacilityName);
                $("#sltFacility").append($('<option>', {
                    value: a.FacilityName,
                    text: a.FacilityName
                }));
            }
        });
        return;
    }
    $("#sltIP option:selected").each(function (c) {
        var val = $(this).val();

        $.grep(model_data, function (a) {

            if (a.IP == val) {
                if (checkforuniqueness(_buildState_, a.LGA.State.state_code).length == 0) {
                    _buildState_.push(a.LGA.State.state_code);
                    $("#sltstate").append($('<option>', {
                        value: a.LGA.State.state_code,
                        text: a.LGA.State.state_name
                    }));
                }
                if (checkforuniqueness(_buildLGA_, a.LGA.lga_code).length == 0) {
                    _buildLGA_.push(a.LGA.lga_code);
                    $("#sltLGA").append($('<option>', {
                        value: a.LGA.lga_code,
                        text: a.LGA.lga_name
                    }));
                }
                if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                    _buildFacility_.push(a.FacilityName);
                    $("#sltFacility").append($('<option>', {
                        value: a.FacilityName,
                        text: a.FacilityName
                    }));
                }
                return true;
            }
        });
    });
});


//State
$("#sltstate").change(function (e) {
    var _buildLGA_ = [];
    var _buildFacility_ = [];

    $("#sltLGA").html("");
    $("#sltFacility").html("");
    $("#sltLGA").select2("val", "");
    $("#sltFacility").select2("val", "");

    if (e.currentTarget.selectedOptions.length == 0) {
        debugger;
        $.each(model_data, function (i, a) {
            if (checkforuniqueness(_buildLGA_, a.LGA.lga_code).length == 0) {
                _buildLGA_.push(a.LGA.lga_code);
                $("#sltLGA").append($('<option>', {
                    value: a.LGA.lga_code,
                    text: a.LGA.lga_name
                }));
            }
            if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                _buildFacility_.push(a.FacilityName);
                $("#sltFacility").append($('<option>', {
                    value: a.FacilityName,
                    text: a.FacilityName
                }));
            }
        });
        return;
    }

    $("#sltstate option:selected").each(function (c) {
        var val = $(this).val();

        $.grep(model_data, function (a) {
            if ($("#sltIP")[0].selectedOptions.length != 0) {
                var gg = getLGA(model_data, ($("#sltIP")[0].selectedOptions), val);
                $.each(gg, function (y, z) {
                    if (checkforuniqueness(_buildLGA_, z.LGA.lga_code).length == 0) {
                        _buildLGA_.push(z.LGA.lga_code);
                        $("#sltLGA").append($('<option>', {
                            value: z.LGA.lga_code,
                            text: z.LGA.lga_name
                        }));
                    }
                });
                var ff = getFacility(model_data, ($("#sltIP")[0].selectedOptions), ($("#sltstate")[0].selectedOptions), a.FacilityName);
                $.each(ff, function (y, z) {
                    if (checkforuniqueness(_buildFacility_, z.FacilityName).length == 0) {
                        _buildFacility_.push(z.FacilityName);
                        $("#sltFacility").append($('<option>', {
                            value: z.FacilityName,
                            text: z.FacilityName
                        }));
                    }
                });
                return true;
            }
            else if (a.LGA.State.state_code == val) {
                if (checkforuniqueness(_buildLGA_, a.LGA.lga_code).length == 0) {
                    _buildLGA_.push(a.LGA.lga_code);
                    $("#sltLGA").append($('<option>', {
                        value: a.LGA.lga_code,
                        text: a.LGA.lga_name
                    }));
                }
                if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                    _buildFacility_.push(a.FacilityName);
                    $("#sltFacility").append($('<option>', {
                        value: a.FacilityName,
                        text: a.FacilityName
                    }));
                }
                return true;
            }
        });
    });
});

//LGA
$("#sltLGA").change(function (e) {
    var _buildFacility_ = [];

    $("#sltFacility").html("");
    $("#sltFacility").select2("val", "");

    if (e.currentTarget.selectedOptions.length == 0) {
        if ($("#sltstate")[0].selectedOptions.length == 0) {
            $.each(model_data, function (i, a) {
                if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                    _buildFacility_.push(a.FacilityName);
                    $("#sltFacility").append($('<option>', {
                        value: a.FacilityName,
                        text: a.FacilityName
                    }));
                }
            });
        }
        else {
            $("#sltstate option:selected").each(function (c) {
                var val = $(this).val();

                $.grep(model_data, function (a) {
                    if (a.LGA.State.state_code == val) {

                        if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                            _buildFacility_.push(a.FacilityName);
                            $("#sltFacility").append($('<option>', {
                                value: a.FacilityName,
                                text: a.FacilityName
                            }));
                        }
                        return true;
                    }
                });
            });
        }
        return;
    }
    $("#sltLGA option:selected").each(function (c) {
        var val = $(this).val();

        $.grep(model_data, function (a) {
            if (a.LGA.lga_code == val) {
                if (checkforuniqueness(_buildFacility_, a.FacilityName).length == 0) {
                    _buildFacility_.push(a.FacilityName);
                    $("#sltFacility").append($('<option>', {
                        value: a.FacilityName,
                        text: a.FacilityName
                    }));
                }
                return true;
            }
        });
    });
});


function buildTable(dataList) {
    $("#output tbody").empty();
    $.each(dataList, function (i, d) {
        var html = "<tr> ";
        if (d.SelectedForDQA) {
            html += "<td><i class='icon-check' style='color: green;font-weight: bolder;margin-left: 30%;'></i></td>";
        }
        else {
            html += "<td>-</td>";
        }
        html += "<td>" + d.PatientId + "</td>";
        html += "<td>" + d.HospitalNo + "</td>";
        html += "<td>" + d.Sex + "</td>";
        html += "<td>" + d.Age_at_start_of_ART_in_years + "</td>";
        html += "<td>" + d.Age_at_start_of_ART_in_months + "</td>";
        html += "<td>" + d.ARTStartDate + "</td>";
        html += "<td>" + d.LastPickupDate + "</td>";
        html += "<td>" + d.MonthsOfARVRefill + "</td>";
        html += "<td>" + d.RegimenLineAtARTStart + "</td>";
        html += "<td>" + d.RegimenAtStartOfART + "</td>";
        html += "<td>" + d.CurrentRegimenLine + "</td>";
        html += "<td>" + d.CurrentARTRegimen + "</td>";
        html += "<td>" + d.PregnancyStatus + "</td>";
        html += "<td>" + d.CurrentViralLoad + "</td>";
        html += "<td>" + d.DateOfCurrentViralLoad + "</td>";
        html += "<td>" + d.ViralLoadIndication + "</td>";
        html += "<td>" + d.CurrentARTStatus + "</td>";
        html += "<td>" + d.RadetYear + "</td>";
        html += "</tr>";
        $("#output tbody").append(html);
    });
}

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
}

function ExportFormattedData(csvFile) {
    var blob = new Blob([csvFile], { type: 'text/csv;charset=utf-8;' });
    if (navigator.msSaveBlob) { // IE 10+
        navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement("a");
        if (link.download !== undefined) { // feature detection
            // Browsers that support HTML5 download attribute
            var url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "mydata.csv");
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }
}

function checkforuniqueness(data, key) {
    var _extz = $.grep(data, function (a) {
        return a == key;
    });
    return _extz;
}

function getLGA(data, ipArray, key) {
    var _extz = $.grep(data, function (a) {
        return a.LGA.State.state_code == key && contains.call(ipArray, a.IP);         
    });
    return _extz;
}

function getFacility(data, ipArray, stateArray, key) {
    var _extz = $.grep(data, function (a) {
        return a.FacilityName == key && contains.call(ipArray, a.IP) && contains.call(stateArray, a.LGA.State.state_code);
    });
    return _extz;
}

var contains = function (needle) {
    // Per spec, the way to identify NaN is that it is not equal to itself
    var findNaN = needle !== needle;
    var indexOf;

    indexOf = function (needle) {
        var i = -1, index = -1;

        for (i = 0; i < this.length; i++) {
            var item = this[i];
            if ((findNaN && item !== item) || item.value === needle) {
                index = i;
                break;
            }
        }
        return index;
    };
    return indexOf.call(this, needle) > -1;
};