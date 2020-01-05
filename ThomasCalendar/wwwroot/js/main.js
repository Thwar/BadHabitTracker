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
}

function GetLocalDate() {

    var date = new Date().toLocaleString();
    console.log(date);
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