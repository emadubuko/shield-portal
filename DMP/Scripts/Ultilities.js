
var base64Image;
function imagePreview(options) {
    var defaults = {};
    if (options) {
        $.extend(true, defaults, options);
    }
    $.each(this, function () {
        var $this = $(this);
        $this.bind('change', function (evt) {

            var files = evt.target.files; // FileList object
            // Loop through the FileList and render image files as thumbnails.
            if (files.length > 0) {
                for (var i = 0, f; f = files[i]; i++) {
                    // Only process image files.
                    if (!f.type.match('image.*')) {
                        continue;
                    }
                    var reader = new FileReader();
                    // Closure to capture the file information.
                    reader.onload = (function (theFile) {
                        return function (e) {
                            // Render thumbnail.
                            base64Image = e.target.result;
                            //$('#imageURL').attr('src', e.target.result);
                        };
                    })(f);
                    // Read in the image file as a data URL.
                    reader.readAsDataURL(f);
                    return base64Image;
                }
            }
        });
    });
}

function BindImageToControl(control, base64ImageString) {
    control.attr('src', "data:image/jpg;base64," + btoa(String.fromCharCode(...new Uint8Array(base64ImageString))));

    //$('#imageURL').attr('src', "data:image/jpg;base64,"+ btoa(String.fromCharCode(...new Uint8Array(selectedOrg.Logo))));
}