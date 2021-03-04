//function AddNote(date) {
//    event.stopPropagation();

//    window.WriteCSharpMessageToConsole = (instance) => {
//        instance.invokeMethodAsync('GetHelloMessage', date)
//            .then((message) => {
//                console.log(message);
//            });
//    };
//}


function OpenModal() {
    $('#addNoteModal').modal('show');
}

function CloseModal() {
    $('#addNoteModal').modal('hide');
    return true;
}

function GetLocalDate() {

    var date = new Date().toLocaleString();
    // console.log(date);
    return date;
}

// https://github.com/avivnaaman/BlazorWasmWYSIWYGEditors
function InitHtmlEditor() {

    removeTinyMceEditors();

    /* delay re-initialization */
    setTimeout(() =>
        tinymce.init({
            selector: 'textarea',
            setup: editor => {
                /* on editor content change event */
                editor.on('change', e => {
                    var ta = document.querySelector('textarea');
                    /* set the value */
                    ta.value = editor.getContent();
                    /* and trigger onchange event */
                    var event = document.createEvent("Events");
                    event.initEvent("change", true, true);
                    ta.dispatchEvent(event);
                });
            },
            plugins: 'advlist link lists autoresize',
            toolbar: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons',
            menubar: false,
            branding: false,
            toolbar_mode: 'floating',
            min_height: 350,
            content_css: 'dark'
        }), 50);
}

/**
 * Removes all the existing TinyMCE editors
 */
function removeTinyMceEditors() {
    /* Remove leftover if such one exists */
    if (typeof (tinyMCE) !== 'undefined') {
        var length = tinyMCE.editors.length;
        for (var i = length; i > 0; i--) {
            tinyMCE.editors[i - 1].remove();
        };
    }
}

function saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}

$(function () {


});

function OpenEditEventModal(color) {
    $('#picker').colpick({
        flat: true,
        layout: 'hex',
        submit: 0,
        onChange: function (hsb, hex, rgb) {
            $('#selectedColor').val(hex);
            $('#picker').val(hex);
        }
    });

    $('#picker').colpickSetColor(color);
    $('#addNoteModal').modal('show');
}


function GetColorPickerValue() {
    //console.log("getting ur color brah " + $('#selectedColor').val());
    return "#" + $('#selectedColor').val();
}


function GetQuotes() {

    fetch("https://type.fit/api/quotes")
        .then(function (response) {
            return response.json();
        })
        .then(function (data) {
            console.log(data);
            return data;
        });

}

