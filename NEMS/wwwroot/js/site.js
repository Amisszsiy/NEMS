// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function addSchedule(info) {
    var modal = $('#scheduleEvent');
    document.getElementById('title').value = null;
    document.getElementById('description').value = null;
    document.getElementById('start').value = info.startStr + "T00:00";
    document.getElementById('end').value = info.endStr + "T00:00";
    document.getElementById('allDay').checked = false;
    var button = document.getElementById('button');
    button.setAttribute('class', 'btn btn-primary');
    button.innerHTML = "Add";
    button.setAttribute('value', 'add');
    $("#dButton").remove();
    modal.modal('show');
}

function editSchedule(info) {
    var modal = $('#scheduleEvent');
    document.getElementById('id').value = info.event.id;
    document.getElementById('title').value = info.event.title;
    document.getElementById('description').value = info.event.extendedProps.description;
    document.getElementById('start').value = info.event.startStr.replace(':00+07:00','');
    document.getElementById('end').value = info.event.endStr.replace(':00+07:00','');
    document.getElementById('allDay').checked = info.event.allDay;
    var button = document.getElementById('button');
    button.setAttribute('class', 'btn btn-primary');
    button.innerHTML = "Edit";
    button.setAttribute('value', 'edit');
    if (!document.getElementById("dButton")) {
        var dButton = document.createElement("button");
        dButton.setAttribute('class', 'btn btn-danger');
        dButton.innerHTML = "delete";
        dButton.setAttribute("id", "dButton");
        dButton.setAttribute("type", "submit");
        dButton.setAttribute("name", "submit");
        dButton.setAttribute("value", 'delete');
        document.getElementById("modal-footer").appendChild(dButton);
    }
    modal.modal('show');
}