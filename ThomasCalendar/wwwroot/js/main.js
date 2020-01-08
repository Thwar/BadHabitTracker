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

